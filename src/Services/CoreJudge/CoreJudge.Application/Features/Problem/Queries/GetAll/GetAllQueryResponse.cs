
using CoreJudge.Domain.Models.Entities;

namespace CoreJudge.Application.Features.Problems.Queries.GetAll
{
    public class GetAllQueryResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public Difficulty Difficulty { get; set; }
        public List<string> Topics { get; set; } = new();
        public bool IsSolved { get; set; }
        public decimal AcceptanceRate { get; set; }
    }
}
