using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace SkillForge.Infrastructure.Extensions;

public static class DbInitializer
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var roleMgr = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        var databaseProvider = ctx.Database.ProviderName;

        // Apenas roda Migrate em bancos relacionais
        if (databaseProvider != "Microsoft.EntityFrameworkCore.InMemory")
        {
            await ctx.Database.MigrateAsync();
        }
        else
        {
            ctx.Database.EnsureCreated(); // opcional, mas pode evitar erros
        }

        // ─── Create Role ─────────────────────
        if (!await roleMgr.RoleExistsAsync("Administrator"))
            await roleMgr.CreateAsync(new IdentityRole("Administrator"));

        // ─── Create Admin User ───────────────
        var user = await userMgr.FindByNameAsync("admin");
        if (user is null)
        {
            var newUser = new IdentityUser
            {
                UserName = "admin",
                Email = "admin@email.com",
                EmailConfirmed = true
            };

            var result = await userMgr.CreateAsync(newUser, "Admin123!@#");
            if (result.Succeeded)
                await userMgr.AddToRoleAsync(newUser, "Administrator");
        }
    }
}
