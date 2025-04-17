namespace SkillForge.Domain.Entities;

public class Project
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public string Description { get; set; } = string.Empty;

    public ICollection<ProjectSkill> Skills {get; set;} = new List<ProjectSkill>();
}