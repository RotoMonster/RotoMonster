using Microsoft.EntityFrameworkCore.Migrations;

namespace RotoMonster.NFL.Migrations.Migrations
{
    public partial class _082920_1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
                name: "IsFinished",
                table: "Games",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "NFLDefenseGames",
                columns: table => new
                {
                    PlayerId = table.Column<int>(nullable: false),
                    GameId = table.Column<int>(nullable: false),
                    TeamId = table.Column<int>(nullable: false),
                    Sacks = table.Column<int>(nullable: false),
                    Interceptions = table.Column<int>(nullable: false),
                    FumbleRecoveries = table.Column<int>(nullable: false),
                    Touchdowns = table.Column<int>(nullable: false),
                    Safeties = table.Column<int>(nullable: false),
                    BlockedKicks = table.Column<int>(nullable: false),
                    XpReturned = table.Column<int>(nullable: false),
                    Points = table.Column<int>(nullable: false),
                    PassAttempts = table.Column<int>(nullable: false),
                    PassCompletion = table.Column<int>(nullable: false),
                    PassYards = table.Column<int>(nullable: false),
                    PassTouchdowns = table.Column<int>(nullable: false),
                    RushAttempts = table.Column<int>(nullable: false),
                    RushYards = table.Column<int>(nullable: false),
                    RushTouchdowns = table.Column<int>(nullable: false),
                    ReceivingAirYards = table.Column<int>(nullable: false),
                    PassSacks = table.Column<int>(nullable: false),
                    Minutes = table.Column<double>(nullable: false),
                    Points0 = table.Column<int>(nullable: false),
                    Points1to6 = table.Column<int>(nullable: false),
                    Points7to13 = table.Column<int>(nullable: false),
                    Points14to20 = table.Column<int>(nullable: false),
                    Points21to27 = table.Column<int>(nullable: false),
                    Points28to34 = table.Column<int>(nullable: false),
                    Points35 = table.Column<int>(nullable: false),
                    Points2to10 = table.Column<int>(nullable: false),
                    Points11to20 = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NFLDefenseGames", x => new { x.PlayerId, x.GameId });
                    table.ForeignKey(
                        name: "FK_NFLDefenseGames_Games_GameId",
                        column: x => x.GameId,
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NFLDefenseGames_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserLeaguePlayerTypes",
                columns: table => new
                {
                    UserLeagueId = table.Column<int>(nullable: false),
                    PlayerTypeId = table.Column<int>(nullable: false),
                    CategoriesCode = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserLeaguePlayerTypes", x => new { x.UserLeagueId, x.PlayerTypeId });
                    table.ForeignKey(
                        name: "FK_UserLeaguePlayerTypes_PlayerTypes_PlayerTypeId",
                        column: x => x.PlayerTypeId,
                        principalTable: "PlayerTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserLeaguePlayerTypes_UserLeagues_UserLeagueId",
                        column: x => x.UserLeagueId,
                        principalTable: "UserLeagues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NFLDefenseGames_GameId",
                table: "NFLDefenseGames",
                column: "GameId");

            migrationBuilder.CreateIndex(
                name: "IX_UserLeaguePlayerTypes_PlayerTypeId",
                table: "UserLeaguePlayerTypes",
                column: "PlayerTypeId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NFLDefenseGames");

            migrationBuilder.DropTable(
                name: "UserLeaguePlayerTypes");

            migrationBuilder.AlterColumn<bool>(
                name: "IsFinished",
                table: "Games",
                type: "bit",
                nullable: true,
                oldClrType: typeof(bool));
        }
    }
}
