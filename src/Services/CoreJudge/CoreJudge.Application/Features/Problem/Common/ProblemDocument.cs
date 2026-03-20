using CoreJudge.Domain.Models.Entities;

namespace CoreJudge.Application.Features.Problem.Common;

    public class ProblemDocument
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Difficulty Difficulty { get; set; }
        public List<int> Topics { get; set; }
    }
    public record ProblemResponse (
        int Id,
        string Name
    );