using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FamilyPulse.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCoachingOrchestratorAndDomainCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Category",
                table: "CheckIns",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Category",
                table: "CheckIns");
        }
    }
}
