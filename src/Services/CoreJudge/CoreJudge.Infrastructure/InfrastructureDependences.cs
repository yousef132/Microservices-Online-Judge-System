using CodeSphere.Domain.Abstractions;
using CodeSphere.Domain.Abstractions.Repositories;
using CodeSphere.Domain.Abstractions.Services;
using CodeSphere.Infrastructure.Implementation.Services;
using CoreJudge.Application.Abstractions.Elasticsearch;
using CoreJudge.Application.Mapping;
using CoreJudge.Domain.Premitives;
using CoreJudge.Infrastructure.Consumers;
using CoreJudge.Infrastructure.Context;
using CoreJudge.Infrastructure.Implementation;
using CoreJudge.Infrastructure.Implementation.Repositories;
using Elastic.Clients.Elasticsearch;
using MassTransit;
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
            cfg.AddProfile<TopicProfile>();
            cfg.AddProfile<TestCaseProfile>();
            cfg.AddProfile<SubmissionProfile>();
            cfg.AddProfile<ProblemProfile>();
            cfg.AddProfile<ContestProfile>();
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
                o.QueryDelay = TimeSpan.FromSeconds(10);
                o.UsePostgres();

                o.UseBusOutbox();
                // this uses in-memory outbox [fast]
                // commented it to use EF Outbox (DB-based)which is : 
                //1.durable
                //2.resilient to crash
                //3.full audit trail
            });

            // Automatically scan and register all consumers across loaded assemblies
            //x.AddConsumers(AppDomain.CurrentDomain.GetAssemblies());

            // Globally configure all automatically registered endpoints 
            // to use a specific dead-letter queue (DLQ) via RabbitMQ BindDeadLetterQueue
            x.AddConsumer<ProblemCreatedConsumer>();
            x.AddConfigureEndpointsCallback((context, name, cfg) =>
            {
                // Enable the Inbox Pattern on all endpoints for deduplication
                cfg.UseEntityFrameworkOutbox<ApplicationDbContext>(context);

                if (cfg is MassTransit.IRabbitMqReceiveEndpointConfigurator rmq)
                {
                    // Note: Dead Letter Queues (DLQ) are handled by MassTransit automatically!
                    // Any message that exhausts the retry policy will be moved to a queue named `core-judge-deadletter-queue`.

                    rmq.BindDeadLetterQueue("core-judge-deadletter-queue");
                }
            });
            // get rabbitmq configurations
            string host = configuration["RabbitMQ:Host"];
            string username = configuration["RabbitMQ:Username"];
            string password = configuration["RabbitMQ:Password"];

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


                // Automatically configure endpoints for all discovered consumers
                rabbitCfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }

    public static IServiceCollection AddElasticsearch(this IServiceCollection services, IConfiguration configuration)
    {
        var url = configuration["ElasticSearch:Uri"]; // host-> localhost, container -> elasticsearch url
        var settings = new ElasticsearchClientSettings(new Uri(url))
            .DefaultIndex("problems"); // Fallback default

        var client = new ElasticsearchClient(settings);
        services.AddSingleton(client);

        CreateIndices(client);

        return services;
    }

    private static void CreateIndices(ElasticsearchClient client)
    {
        // 1. Create Problems Index
        try
        {
            var problemsIndexName = ElasticSearchIndexes.Problems;
            var problemsExists = client.Indices.Exists(problemsIndexName).Exists;
            if (!problemsExists)
            {
                client.Indices.Create(problemsIndexName, c => c
                    .Mappings(m => m
                        .Properties<ProblemDocument>(p => p
                            .IntegerNumber(n => n.Id)
                            .Text(t => t.Name)
                            .Keyword(k => k.Difficulty)
                        )
                    )
                );
            }

            // 2. Create UserAttempts Index
            var userAttemptsIndexName = ElasticSearchIndexes.UserAttempts;
            var userAttemptsExists = client.Indices.Exists(userAttemptsIndexName).Exists;
            if (!userAttemptsExists)
            {
                client.Indices.Create(userAttemptsIndexName, c => c
                    .Mappings(m => m
                        .Properties<UserAttemptDocument>(p => p
                            .Keyword(k => k.UserId)
                            .IntegerNumber(n => n.ProblemId)
                            .Keyword(k => k.Status)
                        )
                    )
                );
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Index Creation Problem : {ex.Message}");
        }
    }
}