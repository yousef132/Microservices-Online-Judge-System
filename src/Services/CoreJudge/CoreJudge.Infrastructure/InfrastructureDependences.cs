using MassTransit;
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
using StackExchange.Redis;

namespace CoreJudge.Infrastructure;

public static class InfrastructureDependencies
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        string connectionString = configuration.GetConnectionString("Default");
        services.AddDbContext<ApplicationDbContext>(opt => { opt.UseNpgsql(connectionString); });
        services.AddAutoMapper(cfg =>
        {
            cfg.AddProfile<CoreJudge.Application.Mapping.TopicProfile>();
            cfg.AddProfile<CoreJudge.Application.Mapping.TestCaseProfile>();
            cfg.AddProfile<CoreJudge.Application.Mapping.SubmissionProfile>();
            cfg.AddProfile<CoreJudge.Application.Mapping.ProblemProfile>();
            cfg.AddProfile<CoreJudge.Application.Mapping.ContestProfile>();
        });
        var redisConnectionString = configuration.GetConnectionString("Redis");

        services.AddSingleton<IConnectionMultiplexer>(sp =>
            ConnectionMultiplexer.Connect(redisConnectionString)
        );

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

        // Register Elasticsearch Extension logic
        services.AddElasticsearch(configuration);

        services.AddMassTransit(x =>
        {
            // Set kebab-case for queue names automatically based on consumer names
            x.SetKebabCaseEndpointNameFormatter();

            // Configure Entity Framework Outbox
            x.AddEntityFrameworkOutbox<ApplicationDbContext>(o =>
            {
                o.UsePostgres();
                o.UseBusOutbox();
            });

            // Automatically scan and register all consumers across loaded assemblies
            //x.AddConsumers(AppDomain.CurrentDomain.GetAssemblies());

            // Globally configure all automatically registered endpoints 
            // to use a specific dead-letter queue (DLQ) via RabbitMQ BindDeadLetterQueue
            x.AddConfigureEndpointsCallback((context, name, cfg) =>
            {
                // Enable the Inbox Pattern on all endpoints for deduplication
                cfg.UseEntityFrameworkOutbox<ApplicationDbContext>(context);

                if (cfg is MassTransit.IRabbitMqReceiveEndpointConfigurator rmq)
                {
                    rmq.BindDeadLetterQueue("core-judge-deadletter-queue");
                }
            });
            // get rabbitmq configurations
            string host = configuration["RabbitMQ:Host"];
            string username = configuration["RabbitMQ:Username"];
            string password = configuration["Password"];

            x.UsingRabbitMq((context, rabbitCfg) =>
            {
                rabbitCfg.Host(host, "/", h =>
                {
                    h.Username(username ?? "guest");
                    h.Password(password ?? "guest");
                });

                // Configure Global Retry Policy (Exponential Backoff)
                rabbitCfg.UseMessageRetry(r => r.Exponential(
                    retryLimit: 3,
                    minInterval: TimeSpan.FromSeconds(2),
                    maxInterval: TimeSpan.FromSeconds(10),
                    intervalDelta: TimeSpan.FromSeconds(2)
                ));

                // Note: Dead Letter Queues (DLQ) are handled by MassTransit automatically!
                // Any message that exhausts the retry policy will be moved to a queue named `<queue-name>_error`.

                // Automatically configure endpoints for all discovered consumers
                rabbitCfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }

    public static IServiceCollection AddElasticsearch(this IServiceCollection services, IConfiguration configuration)
    {
        var url = configuration["ElasticSearch:Uri"] ?? "http://localhost:9200";
        var settings = new Elastic.Clients.Elasticsearch.ElasticsearchClientSettings(new Uri(url))
            .DefaultIndex("problems"); // Fallback default

        var client = new Elastic.Clients.Elasticsearch.ElasticsearchClient(settings);
        services.AddSingleton(client);

        CreateIndices(client);

        return services;
    }

    private static void CreateIndices(Elastic.Clients.Elasticsearch.ElasticsearchClient client)
    {
        // 1. Create Problems Index
        var problemsIndexName = "problems";
        var problemsExists = client.Indices.Exists(problemsIndexName).Exists;
        if (!problemsExists)
        {
            client.Indices.Create(problemsIndexName, c => c
                .Mappings(m => m
                    .Properties<CoreJudge.Application.Abstractions.Elasticsearch.ProblemDocument>(p => p
                        .IntegerNumber(n => n.Id)
                        .Text(t => t.Name)
                        .Keyword(k => k.Difficulty)
                    )
                )
            );
        }

        // 2. Create UserAttempts Index
        var userAttemptsIndexName = "user_attempts";
        var userAttemptsExists = client.Indices.Exists(userAttemptsIndexName).Exists;
        if (!userAttemptsExists)
        {
            client.Indices.Create(userAttemptsIndexName, c => c
                .Mappings(m => m
                    .Properties<CoreJudge.Application.Abstractions.Elasticsearch.UserAttemptDocument>(p => p
                        .Keyword(k => k.UserId)
                        .IntegerNumber(n => n.ProblemId)
                        .Keyword(k => k.Status)
                    )
                )
            );
        }
    }
}