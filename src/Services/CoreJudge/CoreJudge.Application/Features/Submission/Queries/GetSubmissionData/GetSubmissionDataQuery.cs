using BuildingBlocks.Core.CQRS;
using CoreJudge.Domain.Premitives;
using MediatR;

namespace CoreJudge.Application.Features.Submissions.Queries.GetSubmissionData
{
    public sealed class GetSubmissionDataQuery : IQuery<Response>
    {
        public GetSubmissionDataQuery(int submissionId)
        {
            SubmissionId = submissionId;
        }

        public int SubmissionId { get; set; }


    }
}
