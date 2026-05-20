using Community.API.Entities;
using Community.API.Persistence;
using MongoDB.Driver;

namespace Community.API.BackgroundJobs;

public class HotScoreRecalculator(
    ILogger<HotScoreRecalculator> logger,
    IServiceScopeFactory scopeFactory) : BackgroundService
{
    private static readonly TimeSpan _interval = TimeSpan.FromHours(1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("HotScoreRecalculator is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                logger.LogInformation("Running hot score recalculation...");

                using var scope = scopeFactory.CreateScope();
                var articleRepository = scope.ServiceProvider.GetRequiredService<IArticleRepository>();

                await RecalculateHotScores(articleRepository);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error during hot score recalculation.");
            }

            await Task.Delay(_interval, stoppingToken);
        }
    }

    private async Task RecalculateHotScores(IArticleRepository articleRepository)
    {
        var articles = await articleRepository.GetAllPublishedAsync();
        var updates = new List<WriteModel<Article>>();
        var now = DateTime.UtcNow;

        foreach (var article in articles)
        {
            var ageInHours = (now - (article.PublishedAt ?? article.CreatedAt)).TotalHours;
            var gravity = 1.8;
            var hotScore = (article.VoteCount - 1) / Math.Pow(ageInHours + 2, gravity);

            var filter = Builders<Article>.Filter.Eq(a => a.Id, article.Id);
            var update = Builders<Article>.Update.Set(a => a.HotScore, hotScore);
            updates.Add(new UpdateOneModel<Article>(filter, update));
        }

        if (updates.Any())
        {
            await articleRepository.BulkUpdateHotScoresAsync(updates);
            logger.LogInformation("Hot scores updated for {Count} articles.", updates.Count);
        }
    }
}
