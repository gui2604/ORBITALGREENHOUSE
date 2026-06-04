using Microsoft.AspNetCore.Identity;
using OrbitalGreenhouse.Api.Models;
using OrbitalGreenhouse.Api.Repositories;

namespace OrbitalGreenhouse.Api.Data;

/// <summary>
/// Ensures a known demo operator exists in Development for Swagger/Postman testing.
/// </summary>
public static class DevelopmentDemoUserSeeder
{
    public const string DemoEmail = "operador@orbital.space";
    public const string DemoPassword = "orbital123";

    public static async Task SeedAsync(IServiceProvider services)
    {
        var users = services.GetRequiredService<IUserRepository>();
        var email = DemoEmail.Trim().ToLowerInvariant();

        if (await users.ExistsAsync(u => u.Email == email))
            return;

        var hasher = new PasswordHasher<User>();
        var user = new User
        {
            Email = email,
            Role = "Operator",
            PasswordHash = string.Empty
        };
        user.PasswordHash = hasher.HashPassword(user, DemoPassword);

        await users.AddAsync(user);
        await users.SaveChangesAsync();
    }
}
