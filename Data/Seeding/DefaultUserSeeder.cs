using K8Intel.Models;
using Microsoft.AspNetCore.Identity; // For password hashing
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration; // For reading configuration
using System;
using System.Linq;
using System.Security.Cryptography; // For our simple SHA256 hashe
using System.Text;
using System.Threading.Tasks;

namespace K8Intel.Data.Seeding
{
    public static class DefaultUserSeeder
    {
        // Simple SHA256 hasher from your AuthService
        private static string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            }
        }

        public static async Task SeedDefaultAdminUserAsync(IHost app)
        {
            // Create a "scope" to resolve services. This is the correct way to
            // access services like DbContext inside a startup routine.
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var logger = services.GetRequiredService<ILogger<Program>>(); // Using Program as the category
                var context = services.GetRequiredService<AppDbContext>();
                var configuration = services.GetRequiredService<IConfiguration>();
                
                try
                {
                    // Ensure the database is created
                    await context.Database.EnsureCreatedAsync();

                    // Check if there are any users already
                    if (!context.Users.Any())
                    {
                        logger.LogInformation("No users found in the database. Seeding default Admin user...");

                        // Get the default admin credentials from configuration
                        var adminUsername = configuration.GetValue<string>("DefaultAdmin:Username") ?? "admin";
                        var adminPassword = configuration.GetValue<string>("DefaultAdmin:Password");
                        var adminRole = configuration.GetValue<string>("DefaultAdmin:Role") ?? "Admin";

                        if (string.IsNullOrWhiteSpace(adminPassword))
                        {
                            logger.LogError("Default Admin password is not set in configuration. Aborting seeding.");
                            return;
                        }

                        var adminUser = new User
                        {
                            Username = adminUsername,
                            PasswordHash = HashPassword(adminPassword),
                            Role = adminRole
                        };

                        await context.Users.AddAsync(adminUser);
                        await context.SaveChangesAsync();

                        logger.LogInformation("Default Admin user seeded successfully.");
                    }
                    else
                    {
                        logger.LogInformation("Database already has users. Skipping default user seed.");
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred while seeding the default Admin user.");
                }
            }
        }
    }
}