using CodeSphere.Domain.Abstractions.Repositories;
using CoreJudge.Application.Abstractions.Repositories;
using CoreJudge.Domain.Models;

namespace CodeSphere.Domain.Abstractions
{
    public interface IUnitOfWork
    {
        IGenericRepository<TEntity> Repository<TEntity>() where TEntity : BaseEntity;
        IElasticSearchRepository ElasticSearchRepository { get; }
        IContestRepository ContestRepository { get; }
        IUserContestRepository UserContestRepository { get; }
        IProblemRepository ProblemRepository { get; }
        ISubmissionRepository SubmissionRepository { get; }
        ITopicRepository TopicRepository { get; }
        IBlogRepository BlogRepository { get; } 
        Task<int> CompleteAsync();
    }
}
