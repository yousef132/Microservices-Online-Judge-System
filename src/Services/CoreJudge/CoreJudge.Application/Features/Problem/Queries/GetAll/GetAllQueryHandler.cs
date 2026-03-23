using AutoMapper;
using BuildingBlocks.Core.CQRS;
using CodeSphere.Domain.Abstractions;
using CoreJudge.Domain.Premitives;
using MediatR;
using Microsoft.AspNetCore.Http;
using System.Net;
using System.Security.Claims;
using CoreJudge.Application.Abstractions.Elasticsearch;

namespace CoreJudge.Application.Features.Problems.Queries.GetAll
{
    public class GetAllQueryHandler : IQueryHandler<GetAllProblemsQuery, Response>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor contextAccessor;
        private readonly Elastic.Clients.Elasticsearch.ElasticsearchClient _elasticClient;
        private string UserId;


        public GetAllQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, IHttpContextAccessor contextAccessor, Elastic.Clients.Elasticsearch.ElasticsearchClient elasticClient)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            this.contextAccessor = contextAccessor;
            _elasticClient = elasticClient;

            var user = contextAccessor.HttpContext?.User;
            UserId = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
        }

        public async Task<Response> Handle(GetAllProblemsQuery request, CancellationToken cancellationToken)
        {
            // Set up SearchRequest on problems index
            var searchResponse = await _elasticClient.SearchAsync<ProblemDocument>(s => s
                .Indices("problems")
                .From((request.PageNumber - 1) * request.PageSize)
                .Size(request.PageSize)
                .Query(q => q
                    .Bool(b =>
                    {
                        var mustClauses = new List<Action<Elastic.Clients.Elasticsearch.QueryDsl.QueryDescriptor<ProblemDocument>>>();

                        if (!string.IsNullOrEmpty(request.ProblemName))
                        {
                            mustClauses.Add(mq => mq.Match(m => m.Field(f => f.Name).Query(request.ProblemName)));
                        }

                        if (request.Difficulty.HasValue)
                        {
                            mustClauses.Add(mq => mq.Term(t => t.Field(f => f.Difficulty).Value(request.Difficulty.Value.ToString())));
                        }

                        if (mustClauses.Any())
                        {
                            b.Must(mustClauses.ToArray());
                        }
                    })
                ), cancellationToken);

            if (!searchResponse.IsValidResponse)
            {
                return await Response.FailureAsync($"Elasticsearch query failed: {searchResponse.DebugInformation}", HttpStatusCode.InternalServerError);
            }

            var problems = searchResponse.Documents.ToList();
            var totalDocuments = searchResponse.Total;
            var totalNumberOfPages = (int)Math.Ceiling(totalDocuments / (double)request.PageSize);

            // Fetch the user attempt status from the `user_attempts` index
            var userStatusDict = new Dictionary<int, CoreJudge.Domain.Premitives.SubmissionResult>();
            if (!string.IsNullOrEmpty(UserId) && problems.Any())
            {
                var problemIds = problems.Select(p => p.Id).ToList();

                var attemptsResponse = await _elasticClient.SearchAsync<UserAttemptDocument>(s => s
                    .Indices("user_attempts")
                    .Size(request.PageSize)
                    .Query(q => q
                        .Bool(b => b
                            .Must(
                                m1 => m1.Match(m => m.Field(f => f.UserId).Query(UserId)),
                                m2 => m2.Terms(t => t.Field(f => f.ProblemId).Terms(new Elastic.Clients.Elasticsearch.QueryDsl.TermsQueryField(problemIds.Select(id => (Elastic.Clients.Elasticsearch.FieldValue)id).ToList())))
                            )
                        )
                    ), cancellationToken);

                if (attemptsResponse.IsValidResponse && attemptsResponse.Documents.Any())
                {
                    foreach (var attempt in attemptsResponse.Documents)
                    {
                        if (Enum.TryParse<CoreJudge.Domain.Premitives.SubmissionResult>(attempt.Status, out var parsedStatus))
                        {
                            userStatusDict[attempt.ProblemId] = parsedStatus;
                        }
                    }
                }
            }

            var mappedProblems = problems.Select(p => new GetAllQueryResponse
            {
                Id = p.Id,
                Name = p.Name,
                Difficulty = p.Difficulty,
                AcceptanceRate = p.AcceptanceRate,
                Status = string.IsNullOrEmpty(UserId)
                            ? null
                            : (userStatusDict.TryGetValue(p.Id, out var stat) ? stat : null)
            }).ToList();

            return await Response.SuccessAsync(new { mappedProblems, totalNumberOfPages }, "Problems Found", HttpStatusCode.OK);
        }
    }
}
