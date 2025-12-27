using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using BrainstormingApp.Core.Entities;
using BrainstormingApp.Core.Enums;

namespace BrainstormingApp.Infrastructure.Data;

public static class DataSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context, ILogger logger)
    {
        try
        {
            // Apply pending migrations
            if (context.Database.IsRelational())
            {
                var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
                if (pendingMigrations.Any())
                {
                    logger.LogInformation("Applying {Count} pending migrations...", pendingMigrations.Count());
                    await context.Database.MigrateAsync();
                    logger.LogInformation("Migrations applied successfully.");
                }
            }

            // Seed admin user if not exists
            if (!await context.Users.AnyAsync(u => u.Email == "admin@brainstorming.com"))
            {
                logger.LogInformation("Seeding admin user...");

                var adminUser = new User
                {
                    Id = Guid.NewGuid(),
                    FirstName = "Admin",
                    LastName = "User",
                    Email = "admin@brainstorming.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                    Role = UserRole.EventManager,
                    CreatedAt = DateTime.UtcNow
                };

                context.Users.Add(adminUser);
                await context.SaveChangesAsync();

                logger.LogInformation("Admin user created successfully. Email: admin@brainstorming.com, Password: Admin123!");
            }
            else
            {
                logger.LogInformation("Admin user already exists. Skipping seed.");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }
}
