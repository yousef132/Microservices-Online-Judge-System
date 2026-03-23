
using CoreJudge.Domain.Models.Entities;
using CoreJudge.Domain.Premitives;

namespace CoreJudge.Application.Features.Problems.Queries.GetAll
{
    public class GetAllQueryResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public Difficulty Difficulty { get; set; }
        public SubmissionResult? Status { get; set; }
        public decimal AcceptanceRate { get; set; }
        public List<string> Topics { get; set; } = new();
    }
}
