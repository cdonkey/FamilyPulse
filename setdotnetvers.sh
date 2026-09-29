#!/bin/bash
set -e
# Optional: Force all 4 project files to target net8.0
find src/ -name "*.csproj" -exec sed -i 's/<TargetFramework>net8.0<\/TargetFramework>/<TargetFramework>net10.0<\/TargetFramework>/g' {} +

# 2. Add matching EF Core 10 packages
dotnet add src/FamilyPulse.Infrastructure package Microsoft.EntityFrameworkCore.Sqlite
dotnet add src/FamilyPulse.Infrastructure package Microsoft.EntityFrameworkCore.Design
dotnet add src/FamilyPulse.Api package Microsoft.EntityFrameworkCore.Design

# 3. Clean and verify build
dotnet build