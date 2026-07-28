using Microsoft.EntityFrameworkCore.Migrations;

namespace RotoMonster.Data.Migrations
{
    public partial class ul5 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserLeagueTeamPlayers",
                columns: table => new
                {
                    UserLeagueTeamId = table.Column<long>(nullable: false),
                    PlayerId = table.Column<int>(nullable: false),
                    IsActive = table.Column<bool>(nullable: false),
                    IsIR = table.Column<bool>(nullable: false),
                    PickNumber = table.Column<int>(nullable: false),
                    AuctionPrice = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserLeagueTeamPlayers", x => new { x.UserLeagueTeamId, x.PlayerId });
                    table.ForeignKey(
                        name: "FK_UserLeagueTeamPlayers_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserLeagueTeamPlayers_UserLeagueTeams_UserLeagueTeamId",
                        column: x => x.UserLeagueTeamId,
                        principalTable: "UserLeagueTeams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserLeagueTeamPlayers_PlayerId",
                table: "UserLeagueTeamPlayers",
                column: "PlayerId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserLeagueTeamPlayers");
        }
    }
}
