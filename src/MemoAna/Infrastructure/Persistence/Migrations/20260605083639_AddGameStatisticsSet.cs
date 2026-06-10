using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MemoAna.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddGameStatisticsSet : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GameStatistics",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    ThemeName = table.Column<string>(type: "TEXT", nullable: false),
                    Difficulty = table.Column<int>(type: "INTEGER", nullable: false),
                    PlayedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsVictory = table.Column<bool>(type: "INTEGER", nullable: false),
                    TotalMoves = table.Column<int>(type: "INTEGER", nullable: false),
                    SuccessfulMoves = table.Column<int>(type: "INTEGER", nullable: false),
                    Mistakes = table.Column<int>(type: "INTEGER", nullable: false),
                    RemainingSeconds = table.Column<int>(type: "INTEGER", nullable: false),
                    FinalScore = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameStatistics", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GameStatistics");
        }
    }
}
