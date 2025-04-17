# SkillForge API

API para gestão de skills, projetos e desenvolvedores.

**Stack:** .NET 8 · EF Core 8 · SQL Server · Minimal API · xUnit

## Rodar localmente

```bash
git clone https://github.com/Edudonini/SkillForgeApi.git
cd SkillForgeApi
dotnet restore
dotnet ef database update -p SkillForge.Infrastructure -s SkillForge.Api
dotnet run --project SkillForge.Api
