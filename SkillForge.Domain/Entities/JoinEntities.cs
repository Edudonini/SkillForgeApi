using SkillForge.Domain.ValueObjects;

namespace SkillForge.Domain.Entities;

public class DeveloperSkill
{
    public int DeveloperId { get; set; }
    public required Developer Developer { get; set; }

    public int SkillId { get; set; }
    public required Skill Skill { get; set; }

    public SkillLevel Level { get; set; }
}

public class ProjectSkill
{
    public int ProjectId { get; set; }
    public required Project Project { get; set; }

    public int SkillId { get; set; }
    public required Skill Skill { get; set; }

    public SkillLevel RequiredLevel { get; set; }
}