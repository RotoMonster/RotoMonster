using Microsoft.EntityFrameworkCore.Migrations;

namespace RotoMonster.NFL.Migrations.Migrations
{
    public partial class _090120_2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserLeagueImportErrors",
                columns: table => new
                {
                    UserLeagueId = table.Column<int>(nullable: false),
                    Error = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserLeagueImportErrors", x => new { x.UserLeagueId, x.Error });
                    table.ForeignKey(
                        name: "FK_UserLeagueImportErrors_UserLeagues_UserLeagueId",
                        column: x => x.UserLeagueId,
                        principalTable: "UserLeagues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserLeagueImportErrors");
        }
    }
}
