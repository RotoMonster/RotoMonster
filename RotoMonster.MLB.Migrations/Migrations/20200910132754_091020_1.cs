using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace RotoMonster.MLB.Migrations.Migrations
{
    public partial class _091020_1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CategoriesCode",
                table: "UserLeagues");

            migrationBuilder.DropColumn(
                name: "CategoriesCode",
                table: "Drafts");

            migrationBuilder.AddColumn<bool>(
                name: "IsActualPosition",
                table: "Positions",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "FanTraxGroup",
                table: "Categories",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DraftPlayerTypes",
                columns: table => new
                {
                    DraftId = table.Column<int>(nullable: false),
                    PlayerTypeId = table.Column<int>(nullable: false),
                    CategoriesCode = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DraftPlayerTypes", x => new { x.DraftId, x.PlayerTypeId });
                    table.ForeignKey(
                        name: "FK_DraftPlayerTypes_Drafts_DraftId",
                        column: x => x.DraftId,
                        principalTable: "Drafts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DraftPlayerTypes_PlayerTypes_PlayerTypeId",
                        column: x => x.PlayerTypeId,
                        principalTable: "PlayerTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExtraAnalysisLeagues",
                columns: table => new
                {
                    FantasyProviderId = table.Column<int>(nullable: false),
                    ProviderId = table.Column<string>(nullable: false),
                    Title = table.Column<string>(nullable: true),
                    EntryFee = table.Column<int>(nullable: true),
                    NumberOfTeams = table.Column<int>(nullable: true),
                    DraftDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExtraAnalysisLeagues", x => new { x.FantasyProviderId, x.ProviderId });
                    table.ForeignKey(
                        name: "FK_ExtraAnalysisLeagues_FantasyProviders_FantasyProviderId",
                        column: x => x.FantasyProviderId,
                        principalTable: "FantasyProviders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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
                name: "NFLGames",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GameId = table.Column<int>(nullable: false),
                    OverUnder = table.Column<double>(nullable: false),
                    HomeSpread = table.Column<double>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NFLGames", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NFLGames_Games_GameId",
                        column: x => x.GameId,
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TeamAliases",
                columns: table => new
                {
                    TeamId = table.Column<int>(nullable: false),
                    Alias = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeamAliases", x => new { x.TeamId, x.Alias });
                    table.ForeignKey(
                        name: "FK_TeamAliases_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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

            migrationBuilder.CreateIndex(
                name: "IX_DraftPlayerTypes_PlayerTypeId",
                table: "DraftPlayerTypes",
                column: "PlayerTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_NFLDefenseGames_GameId",
                table: "NFLDefenseGames",
                column: "GameId");

            migrationBuilder.CreateIndex(
                name: "IX_NFLGames_GameId",
                table: "NFLGames",
                column: "GameId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DraftPlayerTypes");

            migrationBuilder.DropTable(
                name: "ExtraAnalysisLeagues");

            migrationBuilder.DropTable(
                name: "NFLDefenseGames");

            migrationBuilder.DropTable(
                name: "NFLGames");

            migrationBuilder.DropTable(
                name: "TeamAliases");

            migrationBuilder.DropTable(
                name: "UserLeagueImportErrors");

            migrationBuilder.DropTable(
                name: "UserLeagueMissingPlayers");

            migrationBuilder.DropColumn(
                name: "IsActualPosition",
                table: "Positions");

            migrationBuilder.DropColumn(
                name: "FanTraxGroup",
                table: "Categories");

            migrationBuilder.AddColumn<string>(
                name: "CategoriesCode",
                table: "UserLeagues",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CategoriesCode",
                table: "Drafts",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
