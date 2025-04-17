namespace SkillForge.Domain.Entities;

public class Skill
{
    public int    Id   { get; set; }
    public string Name { get; set; } = default!;

    public ICollection<DeveloperSkill> Developers { get; set; } = new List<DeveloperSkill>();
    public ICollection<ProjectSkill>  Projects   { get; set; } = new List<ProjectSkill>();
}
