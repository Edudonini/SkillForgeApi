# ---------- BUILD ----------
    FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
    WORKDIR /src
    COPY SkillForge.*.sln .
    COPY SkillForge.Domain/ ./SkillForge.Domain/
    COPY SkillForge.Infrastructure/ ./SkillForge.Infrastructure/
    COPY SkillForge.Api/ ./SkillForge.Api/
    RUN dotnet restore SkillForge.Api/SkillForge.Api.csproj
    RUN dotnet publish SkillForge.Api/SkillForge.Api.csproj -c Release -o /app/publish
    
# ---------- RUNTIME ----------
    FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
    WORKDIR /app
    COPY --from=build /app/publish .
    # Porta exposta no Program.cs
    ENV ASPNETCORE_URLS=http://+:8080
    ENTRYPOINT ["dotnet","SkillForge.Api.dll"]
    