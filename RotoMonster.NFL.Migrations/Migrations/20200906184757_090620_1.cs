using Microsoft.EntityFrameworkCore.Migrations;

namespace RotoMonster.NFL.Migrations.Migrations
{
    public partial class _090620_1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CategoriesCode",
                table: "Drafts");

            migrationBuilder.CreateTable(
                name: "UserLeagueMissingPlayers",
                columns: table => new
                {
                    UserLeagueId = table.Column<int>(nullable: false),
                    ProviderId = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserLeagueMissingPlayers", x => new { x.UserLeagueId, x.ProviderId });
                    table.ForeignKey(
                        name: "FK_UserLeagueMissingPlayers_UserLeagues_UserLeagueId",
                        column: x => x.UserLeagueId,
                        principalTable: "UserLeagues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserLeagueMissingPlayers");

            migrationBuilder.AddColumn<string>(
                name: "CategoriesCode",
                table: "Drafts",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
