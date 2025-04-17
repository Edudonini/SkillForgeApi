using Microsoft.EntityFrameworkCore;
using SkillForge.Infrastructure;
using SkillForge.Domain.Entities;
using SkillForge.Domain.ValueObjects;

var builder = WebApplication.CreateBuilder(args);

// Services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var cs = builder.Configuration.GetConnectionString("Default") 
         ?? throw new InvalidOperationException("Connection string not configurada.");
builder.Services.AddDbContext<AppDbContext>(opt => opt.UseSqlServer(cs));

var app = builder.Build();

// Middlewares

app.UseSwagger();
app.UseSwaggerUI();
app.MapGet("/", () => Results.Redirect("/swagger"));

// CRUD Developer

app.MapGet("/developers", async (AppDbContext db) =>
    await db.Developers.Include(d => d.Skills).ThenInclude(ds => ds.Skill).ToListAsync());

app.MapGet("/developers/{id:int}", async (int id, AppDbContext db) =>
    await db.Developers.FindAsync(id) is { } dev ? Results.Ok(dev) : Results.NotFound());

app.MapPost("/developers", async (DeveloperDto dto, AppDbContext db) =>
{
    var dev = new Developer { Name = dto.Name, Email = Email.Create(dto.Email) };
    db.Developers.Add(dev);
    await db.SaveChangesAsync();
    return Results.Created($"/developers/{dev.Id}", dev);
});

app.MapDelete("/developers/{id:int}", async (int id, AppDbContext db) =>
{
    var dev = await db.Developers.FindAsync(id);
    if (dev is null) return Results.NotFound();
    db.Remove(dev);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.Run();

internal record DeveloperDto(string Name, string Email);
