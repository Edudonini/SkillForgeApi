using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SkillForge.Domain.Entities;
using SkillForge.Domain.ValueObjects;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;


namespace SkillForge.Infrastructure;

public class AppDbContext : IdentityDbContext<IdentityUser>
{
    public DbSet<Developer> Developers => Set<Developer>();
    public DbSet<Skill> Skills => Set<Skill>();
    public DbSet<Project> Projects => Set<Project>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        var emailConverter = new ValueConverter<Email, string>(
            v => v.ToString(),
            v => Email.Create(v));

        b.Entity<Developer>(e =>
        {
            e.Property(d => d.Email)
             .HasConversion(emailConverter)
             .IsRequired()
             .HasMaxLength(160);

            e.HasIndex(d => d.Email).IsUnique();
        });

        b.Entity<DeveloperSkill>().HasKey(ds => new { ds.DeveloperId, ds.SkillId });
        b.Entity<DeveloperSkill>()
            .HasOne(ds => ds.Developer).WithMany(d => d.Skills).HasForeignKey(ds => ds.DeveloperId);
        b.Entity<DeveloperSkill>()
            .HasOne(ds => ds.Skill).WithMany(s => s.Developers).HasForeignKey(ds => ds.SkillId);

        b.Entity<ProjectSkill>().HasKey(ps => new { ps.ProjectId, ps.SkillId });
        b.Entity<ProjectSkill>()
            .HasOne(ps => ps.Project).WithMany(p => p.Skills).HasForeignKey(ps => ps.ProjectId);
        b.Entity<ProjectSkill>()
            .HasOne(ps => ps.Skill).WithMany(s => s.Projects).HasForeignKey(ps => ps.SkillId);
    }
}
