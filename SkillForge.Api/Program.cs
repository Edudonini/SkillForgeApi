using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using SkillForge.Api.Auth;
using SkillForge.Domain.Entities;
using SkillForge.Domain.ValueObjects;
using SkillForge.Infrastructure;
using SkillForge.Infrastructure.Extensions;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Diagnostics.HealthChecks;


// ───────────────────────────────────────────────────────────── builder
var builder = WebApplication.CreateBuilder(args);

// Serilog
builder.Host.UseSerilog((ctx, cfg) =>
    cfg.ReadFrom.Configuration(ctx.Configuration)
       .Enrich.FromLogContext()
       .WriteTo.Console());

//Cors simples
builder.Services.AddCors(opt =>
    opt.AddPolicy("Open", p => p.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin()));

//Heath Checks
builder.Services.AddHealthChecks()
                .AddDbContextCheck<AppDbContext>("SqlServer");      

// Serviços
builder.Services.AddEndpointsApiExplorer();
//Swagger com botão "Authorize" para JWT
builder.Services.AddSwaggerGen(o =>
{
    o.SwaggerDoc("v1", new() { Title = "SkillForge API", Version = "v1" });
    var jwtScheme = new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        In   = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Reference = new() { Id = "Bearer", Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme }
    };
    o.AddSecurityDefinition("Bearer", jwtScheme);
    o.AddSecurityRequirement(new()
    {
        [jwtScheme] = new[] { "Bearer" }
    });
});
builder.Services.AddSingleton<IRefreshTokenService, RefreshTokenService>();


// DbContext
if (builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseInMemoryDatabase("TestDb"));
}
else
{
    var cs = builder.Configuration.GetConnectionString("Default")
             ?? throw new InvalidOperationException("Conn string não configurada.");
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlServer(cs));
}

// JWT
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
var jwt = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>()!;

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer              = jwt.Issuer,
            ValidAudience            = jwt.Audience,
            IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SecretKey))
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddSingleton<ITokenService, TokenService>();

// Identity
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// ───────────────────────────────────────────────────────────── app
var app = builder.Build();
await DbInitializer.SeedAsync(app.Services);


app.UseSerilogRequestLogging();
app.UseSwagger(); app.UseSwaggerUI();
app.UseAuthentication();
app.UseAuthorization();
app.UseCors("Open");

// Health endpoint
app.MapHealthChecks("/healthz");

app.MapGet("/", () => Results.Redirect("/swagger"));

// Login
app.MapPost("/login", async (
        LoginDto dto,
        SignInManager<IdentityUser> signIn,
        UserManager<IdentityUser> userMgr,
        ITokenService ts,
        IRefreshTokenService rt) =>
{
    var result = await signIn.PasswordSignInAsync(dto.UserName, dto.Password, false, false);
    if (!result.Succeeded) return Results.Unauthorized();

    var user  = await userMgr.FindByNameAsync(dto.UserName);
    var roles = await userMgr.GetRolesAsync(user!);
    var roleClaims = roles.Select(r => new Claim(ClaimTypes.Role, r));

    var (access, refresh) = rt.GeneratePair(dto.UserName, roleClaims);
    return Results.Ok(new { access, refresh });
});

// Refresh
app.MapPost("/refresh", (string refreshToken, IRefreshTokenService rt) =>
{
    var newAccess = rt.Rotate(refreshToken);
    return Results.Ok(new { access = newAccess });
});

// CRUD Developer protegido
var devGroup = app.MapGroup("/developers").RequireAuthorization(p => p.RequireRole("Administrator"));

devGroup.MapGet("/", async (AppDbContext db) =>
        await db.Developers.Include(d => d.Skills).ThenInclude(s => s.Skill).ToListAsync());

devGroup.MapGet("/{id:int}", async (int id, AppDbContext db) =>
        await db.Developers.FindAsync(id) is { } dev ? Results.Ok(dev) : Results.NotFound());

devGroup.MapPost("/", async (DeveloperDto dto, AppDbContext db) =>
{
    var dev = new Developer { Name = dto.Name, Email = Email.Create(dto.Email) };
    db.Developers.Add(dev);
    await db.SaveChangesAsync();
    return Results.Created($"/developers/{dev.Id}", dev);
});

devGroup.MapDelete("/{id:int}", async (int id, AppDbContext db) =>
{
    var dev = await db.Developers.FindAsync(id);
    if (dev is null) return Results.NotFound();
    db.Remove(dev);
    await db.SaveChangesAsync();
    return Results.NoContent();
});


app.Run();

// ───────────────────────────────────────────────────────────── tipos (depois dos top‑level!)
public partial class Program { }

internal record LoginDto(string UserName, string Password);
internal record DeveloperDto(string Name, string Email);
