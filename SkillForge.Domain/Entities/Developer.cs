using SkillForge.Domain.ValueObjects;

namespace SkillForge.Domain.Entities;

public class Developer
{
    public int Id      { get; set; }
    public string Name { get; set; } = default!;
    public Email Email { get; set; } = default!;

    public ICollection<DeveloperSkill> Skills { get; set; } = new List<DeveloperSkill>();
}
