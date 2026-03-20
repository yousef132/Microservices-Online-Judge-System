
using CoreJudge.Domain.Models;
using CoreJudge.Domain.Premitives;

namespace CodeSphere.Domain.Abstractions.Repositories
{
    public interface ISubmissionRepository
    {
        Task<IQueryable<Submission>> GetAllSubmissions(int problemId, string userId);
        IQueryable<Submission> GetSolvedSubmissions(int problemId, string userId);
        Task<HashSet<int>> GetUserAcceptedSubmissionIdsAsync(string userId);
        Task<Dictionary<int, SubmissionResult>> GetUserSubmissionsAsync(string userId);
        Task<bool> IsUserAuthorizedToViewSubmission(string userId, int submissionId);
        Task<List<Submission>> GetContestACSubmissionsByProblemIdsAsync(int contestId, List<int> problemIds);

        Task<IReadOnlyList<Submission>> GetUserContestSubmissions(int contestId, string userId);


    }

}
