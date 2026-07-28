using Microsoft.EntityFrameworkCore.Migrations;

namespace RotoMonster.Data.Migrations
{
    public partial class ulars : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserLeagueActiveRosterSpots",
                columns: table => new
                {
                    UserLeagueId = table.Column<int>(nullable: false),
                    ActiveRosterSpotId = table.Column<int>(nullable: false),
                    NumberOfPlayers = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserLeagueActiveRosterSpots", x => new { x.UserLeagueId, x.ActiveRosterSpotId });
                    table.ForeignKey(
                        name: "FK_UserLeagueActiveRosterSpots_ActiveRosterSpot_ActiveRosterSpotId",
                        column: x => x.ActiveRosterSpotId,
                        principalTable: "ActiveRosterSpot",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserLeagueActiveRosterSpots_UserLeagues_UserLeagueId",
                        column: x => x.UserLeagueId,
                        principalTable: "UserLeagues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserLeagueActiveRosterSpots_ActiveRosterSpotId",
                table: "UserLeagueActiveRosterSpots",
                column: "ActiveRosterSpotId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserLeagueActiveRosterSpots");
        }
    }
}
