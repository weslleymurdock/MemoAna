using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MemoAna.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCardThemeManifest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDefault",
                table: "CardThemes");

            migrationBuilder.RenameColumn(
                name: "ThemeName",
                table: "CardThemes",
                newName: "ManifestId");

            migrationBuilder.CreateTable(
                name: "CardThemeManifests",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    ThemeName = table.Column<string>(type: "TEXT", nullable: false),
                    IsDefault = table.Column<bool>(type: "INTEGER", nullable: false),
                    PreviewBase64Image = table.Column<string>(type: "TEXT", nullable: false),
                    CardThemeId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CardThemeManifests", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CardThemes_ManifestId",
                table: "CardThemes",
                column: "ManifestId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_CardThemes_CardThemeManifests_ManifestId",
                table: "CardThemes",
                column: "ManifestId",
                principalTable: "CardThemeManifests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CardThemes_CardThemeManifests_ManifestId",
                table: "CardThemes");

            migrationBuilder.DropTable(
                name: "CardThemeManifests");

            migrationBuilder.DropIndex(
                name: "IX_CardThemes_ManifestId",
                table: "CardThemes");

            migrationBuilder.RenameColumn(
                name: "ManifestId",
                table: "CardThemes",
                newName: "ThemeName");

            migrationBuilder.AddColumn<bool>(
                name: "IsDefault",
                table: "CardThemes",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }
    }
}
