using Microsoft.EntityFrameworkCore;

namespace Users.API.Extentions;

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
                var db = scope.ServiceProvider.GetRequiredService<UserDbContext>();

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