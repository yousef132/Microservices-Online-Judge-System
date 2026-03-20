using CoreJudge.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace CoreJudge.API.Extentions;

    public static class MigrationExtensions
    {
        public static async Task ApplyMigrationsWithRetryAsync(
            this IServiceProvider services,
            int retries = 5)
        {
            for (int i = 1; i <= retries; i++)
            {
                try
                {
                    using var scope = services.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                    await db.Database.MigrateAsync();
                    Console.WriteLine("EF migrations applied");
                    return;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Migration attempt {i} failed: {ex.Message}");
                    await Task.Delay(2000);
                }
            }

            throw new Exception("Failed to apply EF migrations");
        }

    }
