using AutoMapper;
using BuildingBlocks.Core.CQRS;
using CodeSphere.Domain.Abstractions;
using CoreJudge.Domain.Premitives;
using MediatR;
using Microsoft.AspNetCore.Http;
using System.Net;
using System.Security.Claims;
using CoreJudge.Application.Abstractions.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Ingest;

namespace CoreJudge.Application.Features.Problems.Queries.GetAll
{
    public class GetAllProblemsQueryHandler : IQueryHandler<GetAllProblemsQuery, Response>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly ElasticsearchClient _elasticClient;
        private readonly string _userId;

        public GetAllProblemsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, IHttpContextAccessor contextAccessor, ElasticsearchClient elasticClient)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _contextAccessor = contextAccessor;
            _elasticClient = elasticClient;

            var user = _contextAccessor.HttpContext?.User;
            // Get the user ID from claims, default to empty string if not authenticated
            _userId = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
        }

        public async Task<Response> Handle(GetAllProblemsQuery request, CancellationToken cancellationToken)
        {
            // 1. Fetch the filtered problems from Elasticsearch
            var searchResponse = await FetchProblemsAsync(request, cancellationToken);

            if (!searchResponse.IsValidResponse)
            {
                return await Response.FailureAsync($"Elasticsearch query failed: {searchResponse.DebugInformation}", HttpStatusCode.InternalServerError);
            }

            var problems = searchResponse.Documents.ToList();
            var totalNumberOfPages = (int)Math.Ceiling(searchResponse.Total / (double)request.PageSize);

            // 2. Fetch the user's attempt records for these exact problems (if user is logged in)
            var problemStatusDict = await FetchUserAttemptsAsync(problems, cancellationToken);

            // 3. Map to final response objects
            var mappedProblems = MapToResponse(problems, problemStatusDict);

            return await Response.SuccessAsync(new { mappedProblems, totalNumberOfPages }, "Problems Found", HttpStatusCode.OK);
        }

        private async Task<SearchResponse<ProblemDocument>> FetchProblemsAsync(GetAllProblemsQuery request, CancellationToken cancellationToken)
        {
            return await _elasticClient.SearchAsync<ProblemDocument>(s => s
                .Indices(ElasticSearchIndexes.Problems)
                .From((request.PageNumber - 1) * request.PageSize)
                .Size(request.PageSize)
                .Query(q => q
                    .Bool(b =>
                    {
                        var mustClauses = new List<Action<QueryDescriptor<ProblemDocument>>>();

                        if (!string.IsNullOrEmpty(request.ProblemName))
                            mustClauses.Add(mq => mq.Match(m => m.Field(f => f.Name).Query(request.ProblemName)));

                        if (request.Difficulty.HasValue)
                            mustClauses.Add(mq => mq.Term(t => t.Field(f => f.Difficulty).Value(request.Difficulty.Value.ToString())));

                        if (request.Topics != null && request.Topics.Any())
                        {
                            var topicValues = request.Topics.Select(id => (FieldValue)id).ToList();
                            mustClauses.Add(mq => mq.Terms(t => t.Field("topics.id").Terms(new TermsQueryField(topicValues))));
                        }

                        if (mustClauses.Any())
                            b.Must(mustClauses.ToArray());
                    })
                ), cancellationToken);
        }

        private async Task<Dictionary<int, SubmissionResult>> FetchUserAttemptsAsync(List<ProblemDocument> problems, CancellationToken cancellationToken)
        {
            var problemStatusDict = new Dictionary<int, SubmissionResult>();

            if (string.IsNullOrEmpty(_userId) || !problems.Any())
                return problemStatusDict;

            var problemIds = problems.Select(p => p.Id).ToList();

            var attemptsResponse = await _elasticClient.SearchAsync<UserAttemptDocument>(s => s
                .Indices(ElasticSearchIndexes.UserAttempts)
                .Size(problemIds.Count) // Fetch all attempts tightly related to these problems, bypassing ES default limit of 10
                .Query(q => q
                    .Bool(b => b
                        .Must(
                            m1 => m1.Match(m => m.Field(f => f.UserId).Query(_userId)),
                            m2 => m2.Terms(t => t.Field(f => f.ProblemId).Terms(new TermsQueryField(problemIds.Select(id => (FieldValue)id).ToList())))
                        )
                    )
                ), cancellationToken);

            if (attemptsResponse.IsValidResponse && attemptsResponse.Documents.Any())
            {
                foreach (var attempt in attemptsResponse.Documents)
                {
                    if (attempt.Status.HasValue)
                    {
                        problemStatusDict[attempt.ProblemId] = attempt.Status.Value;
                    }
                }
            }

            return problemStatusDict;
        }

        private List<GetAllQueryResponse> MapToResponse(List<ProblemDocument> problems, Dictionary<int, SubmissionResult> userStatusDict)
        {
            return problems.Select(p => new GetAllQueryResponse
            {
                Id = p.Id,
                Name = p.Name,
                Difficulty = p.Difficulty,
                AcceptanceRate = p.AcceptanceRate,
                Status = string.IsNullOrEmpty(_userId)
                            ? null
                            : (userStatusDict.TryGetValue(p.Id, out var stat) ? stat : null),
                Topics = p.Topics?.Select(t => t.Name).ToList() ?? []
            }).ToList();
        }
    }
}
