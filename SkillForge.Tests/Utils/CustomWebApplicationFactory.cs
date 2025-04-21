using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SkillForge.Api;
using SkillForge.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace SkillForge.Tests.Utils;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing"); // Garante que seu Program.cs entre no bloco de InMemory

        builder.ConfigureAppConfiguration((context, configBuilder) =>
        {
            var settings = new Dictionary<string, string?>
            {
                ["JwtSettings:Issuer"] = "TestIssuer",
                ["JwtSettings:Audience"] = "TestAudience",
                ["JwtSettings:SecretKey"] = "this_is_a_very_secure_key_123456"
            };

            configBuilder.AddInMemoryCollection(settings);
        });

        builder.ConfigureServices(services =>
        {
            // Remove o contexto atual que foi registrado com SQL Server
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));

            if (descriptor != null)
                services.Remove(descriptor);

            // Registra o contexto com InMemory para testes
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseInMemoryDatabase("TestDb");
            });

            // Registra dependências após criação de escopo
            var sp = services.BuildServiceProvider();

            using var scope = sp.CreateScope();
            var scopedServices = scope.ServiceProvider;

            var db = scopedServices.GetRequiredService<AppDbContext>();
            var userMgr = scopedServices.GetRequiredService<UserManager<IdentityUser>>();
            var roleMgr = scopedServices.GetRequiredService<RoleManager<IdentityRole>>();

            db.Database.EnsureCreated();

            // Seed admin user
            if (!roleMgr.RoleExistsAsync("Administrator").Result)
                roleMgr.CreateAsync(new IdentityRole("Administrator")).Wait();

            var adminUser = userMgr.FindByNameAsync("admin").Result;
            if (adminUser is null)
            {
                var newUser = new IdentityUser
                {
                    UserName = "admin",
                    Email = "admin@email.com",
                    EmailConfirmed = true
                };

                var result = userMgr.CreateAsync(newUser, "Admin123!@#").Result;
                if (result.Succeeded)
                    userMgr.AddToRoleAsync(newUser, "Administrator").Wait();
            }
        });
    }
}
