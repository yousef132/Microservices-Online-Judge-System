using CodeSphere.Domain.Abstractions;
using CodeSphere.Domain.Abstractions.Repositories;
using CodeSphere.Domain.Abstractions.Services;
using CodeSphere.Infrastructure.Implementation.Services;
using CoreJudge.Infrastructure.Context;
using CoreJudge.Infrastructure.Implementation;
using CoreJudge.Infrastructure.Implementation.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CoreJudge.Infrastructure;

public static class InfrastructureDependencies
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        string connectionString = configuration.GetConnectionString("Default");
        services.AddDbContext<ApplicationDbContext>(opt => { opt.UseNpgsql(connectionString); });
        services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        services.AddScoped(typeof(IExecutionService), typeof(ExecutionService));
        services.AddScoped(typeof(ISubmissionRepository), typeof(SubmissionRepository));
        services.AddScoped(typeof(IProblemRepository), typeof(ProblemRepository));
        services.AddScoped(typeof(IFileService), typeof(FileService));
        services.AddScoped(typeof(IContestRepository), typeof(ContestRepository));
        services.AddScoped(typeof(IBlogRepository), typeof(BlogRepository));
        // services.AddScoped(typeof(IElasticSearchRepository), typeof(ElasticSearchRepository));
        services.AddScoped(typeof(IUserContestRepository), typeof(UserContestRepository));
        services.AddScoped(typeof(ITopicRepository), typeof(TopicRepository));
        return services;
    }

}