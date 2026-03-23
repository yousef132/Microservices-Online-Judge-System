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
        // [INBOX PATTERN]
        // This consumer is protected by MassTransit's Inbox Pattern (configured via UseEntityFrameworkOutbox).
        // It uses the InboxState table to track processed message IDs, preventing duplicate work.

        var evt = context.Message;

        var document = new ProblemDocument
        {
            Id = evt.ProblemId,
            Name = evt.Title,
            Difficulty = Enum.TryParse<Difficulty>(evt.Difficulty, out var d) ? d : Difficulty.Easy
        };

        // For true idempotency with the Inbox Pattern, we use the ProblemId as the Elasticsearch document ID.
        // This ensures that if the same message is ever processed twice (due to external retries),
        // it simply overwrites the same document instead of creating a duplicate.
        var response = await _elasticClient.IndexAsync(document, idx => idx
            .Index(ElasticSearchIndexes.Problems)
            .Id(evt.ProblemId));

        if (!response.IsValidResponse)
        {
            throw new Exception($"Failed to index problem {evt.ProblemId} to Elasticsearch: {response.DebugInformation}");
        }
    }
}
