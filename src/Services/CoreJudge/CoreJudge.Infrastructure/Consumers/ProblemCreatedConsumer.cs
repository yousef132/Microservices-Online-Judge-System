using CoreJudge.Domain.Events;
using CoreJudge.Domain.Premitives;
using Elastic.Clients.Elasticsearch;
using MassTransit;
using CoreJudge.Domain.Models.Entities;
using CoreJudge.Application.Abstractions.Elasticsearch;
namespace CoreJudge.Infrastructure.Consumers;

public class ProblemCreatedConsumer : IConsumer<ProblemCreatedEvent>
{
    private readonly ElasticsearchClient _elasticClient;

    public ProblemCreatedConsumer(ElasticsearchClient elasticClient)
    {
        _elasticClient = elasticClient;
    }

    public async Task Consume(ConsumeContext<ProblemCreatedEvent> context)
    {
        Console.WriteLine($"[Consumer START] ProblemCreatedEvent received. ProblemId: {context.Message.ProblemId}");

        var evt = context.Message;

        Console.WriteLine($"[Consumer INFO] Mapping event to document. ProblemId: {evt.ProblemId}");

        var document = new ProblemDocument
        {
            Id = evt.ProblemId,
            Name = evt.Title,
            Difficulty = Enum.TryParse<Difficulty>(evt.Difficulty, out var d) ? d : Difficulty.Easy
        };

        Console.WriteLine($"[Consumer INFO] Sending document to Elasticsearch. ProblemId: {evt.ProblemId}");

        var response = await _elasticClient.IndexAsync(document, idx => idx
            .Index(ElasticSearchIndexes.Problems)
            .Id(evt.ProblemId));

        if (!response.IsValidResponse)
        {
            Console.WriteLine($"[Consumer ERROR] Failed to index ProblemId: {evt.ProblemId}");
            Console.WriteLine(response.DebugInformation);

            throw new Exception($"Failed to index problem {evt.ProblemId} to Elasticsearch: {response.DebugInformation}");
        }

        Console.WriteLine($"[Consumer SUCCESS] Indexed successfully. ProblemId: {evt.ProblemId}");
    }
}
