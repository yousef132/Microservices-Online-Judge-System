using CodeSphere.Domain.Abstractions;
using CodeSphere.Domain.Abstractions.Services;
using CoreJudge.Domain.Premitives;

namespace CodeSphere.Infrastructure.Implementation.Services
{
    public class RankUpService : IRankUpService
    {
        private readonly IUnitOfWork unitOfWork;

        public RankUpService( IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }
        public Task InceaseUserRank(string userId, int problemId)
        {
            throw new NotImplementedException();
        }

        public Task LevelUpUserRank(string userId, ContestPoints points)
        {
            throw new NotImplementedException();
        }
    }
}
