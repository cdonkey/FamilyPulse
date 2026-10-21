using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FamilyPulse.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCheckInAndMemberContext : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CheckIns",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    FamilyAccountId = table.Column<Guid>(type: "TEXT", nullable: false),
                    MemberId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    StressLevel = table.Column<int>(type: "INTEGER", nullable: false),
                    ClosenessLevel = table.Column<int>(type: "INTEGER", nullable: false),
                    EnergyLevel = table.Column<int>(type: "INTEGER", nullable: false),
                    Note = table.Column<string>(type: "TEXT", nullable: true),
                    Tags = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CheckIns", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MemberContexts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    MemberId = table.Column<Guid>(type: "TEXT", nullable: false),
                    FamilyAccountId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Age = table.Column<int>(type: "INTEGER", nullable: true),
                    CommunicationPreference = table.Column<string>(type: "TEXT", nullable: true),
                    Interests = table.Column<string>(type: "TEXT", nullable: false),
                    TopicsToAvoid = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastUpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MemberContexts", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CheckIns");

            migrationBuilder.DropTable(
                name: "MemberContexts");
        }
    }
}
