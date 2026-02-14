using CodeSphere.Domain.Abstractions.Repositories;
using CoreJudge.Domain.Models;
using CoreJudge.Domain.Premitives;
using CoreJudge.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace CoreJudge.Infrastructure.Implementation.Repositories
{
    public class SubmissionRepository : ISubmissionRepository
    {
        private readonly ApplicationDbContext _context;

        public SubmissionRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IQueryable<Submission>> GetAllSubmissions(int problemId, string userId)
         => _context.Submissions.Where(x => x.ProblemId == problemId && x.AttemperId == Guid.Parse(userId));

        public IQueryable<Submission> GetSolvedSubmissions(int problemId, string userId)
        {
            return _context.Submissions.Where(x => x.ProblemId == problemId && x.AttemperId == Guid.Parse(userId) && x.Result == SubmissionResult.Accepted);
        }

        public async Task<HashSet<int>> GetUserAcceptedSubmissionIdsAsync(string userId)
        {
            List<int> problemIds = await _context.Submissions
                        .Where(s => s.AttemperId == Guid.Parse(userId) && s.Result == SubmissionResult.Accepted)
                        .Select(s => s.ProblemId).ToListAsync();

            return new HashSet<int>(problemIds);
        }

        public async Task<Dictionary<int, SubmissionResult>> GetUserSubmissionsAsync(string userId)
        {

            // problemId: Status


            var submissions = await _context.Submissions
            .Where(s => s.AttemperId == Guid.Parse(userId))
            .ToListAsync();

            var result = new Dictionary<int, SubmissionResult>();

            foreach (var submission in submissions)
            {
                if (!result.ContainsKey(submission.ProblemId) || submission.Result == SubmissionResult.Accepted)
                {
                    result[submission.ProblemId] = submission.Result;
                }
            }

            return result;

        }

        public async Task<bool> IsUserAuthorizedToViewSubmission(string userId, int submissionId)
        {
            var submission = await _context.Submissions.Include(x => x.Contest).FirstOrDefaultAsync(x => x.Id == submissionId);
            if (submission.Contest.ContestStatus == ContestStatus.Running && submission.AttemperId != Guid.Parse(userId))
                return false;

            return true;
        }
        public async Task<List<Submission>> GetContestACSubmissionsByProblemIdsAsync(int contestId, List<int> problemIds)
        {
            return await _context.Submissions
                .Where(s => s.ContestId == contestId && problemIds.Contains(s.ProblemId) && s.Result == SubmissionResult.Accepted)
                .ToListAsync();
        }

        public async Task<IReadOnlyList<Submission>> GetUserContestSubmissions(int contestId, string userId)
        {
            return await _context.Submissions.Include(s => s.Problem)
                .Where(s => s.ContestId == contestId && s.AttemperId == Guid.Parse(userId)).ToListAsync();
        }
    }

}
