#!/bin/bash
set -e

# 1. Create solution
dotnet new sln -n FamilyPulse

# 2. Create projects inside src/
dotnet new classlib -o src/FamilyPulse.Domain
dotnet new classlib -o src/FamilyPulse.Application
dotnet new classlib -o src/FamilyPulse.Infrastructure
dotnet new webapi -o src/FamilyPulse.Api

# 3. Add project references
dotnet add src/FamilyPulse.Application reference src/FamilyPulse.Domain
dotnet add src/FamilyPulse.Infrastructure reference src/FamilyPulse.Domain src/FamilyPulse.Application
dotnet add src/FamilyPulse.Api reference src/FamilyPulse.Application src/FamilyPulse.Infrastructure


# 4. Add EF Core packages
dotnet add src/FamilyPulse.Infrastructure package Microsoft.EntityFrameworkCore.Sqlite
dotnet add src/FamilyPulse.Infrastructure package Microsoft.EntityFrameworkCore.Design
dotnet add src/FamilyPulse.Api package Microsoft.EntityFrameworkCore.Design
dotnet add src/FamilyPulse.Application package Microsoft.EntityFrameworkCore

# 5 Other packages

dotnet add src/FamilyPulse.Api package Swashbuckle.AspNetCore



# 6. Attach projects to solution
dotnet sln add src/FamilyPulse.Domain src/FamilyPulse.Application src/FamilyPulse.Infrastructure src/FamilyPulse.Api

echo "----------------------------------------"
echo "Solution setup completed successfully!"
echo "----------------------------------------"