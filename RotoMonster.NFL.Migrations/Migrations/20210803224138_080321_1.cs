using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace RotoMonster.NFL.Migrations.Migrations
{
    public partial class _080321_1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Games_Teams_AwayTeamId",
                table: "Games");

            migrationBuilder.DropForeignKey(
                name: "FK_Games_Teams_HomeTeamId",
                table: "Games");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OwnershipPlayers",
                table: "OwnershipPlayers");

            migrationBuilder.AddColumn<bool>(
                name: "IsStreamable",
                table: "PlayerTypes",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDefaultDisplay",
                table: "PerValues",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LineupFrequency",
                table: "OwnershipPlayers",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "AwayMoneyLine",
                table: "NFLGames",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "HomeMoneyLine",
                table: "NFLGames",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TeamId",
                table: "NBAPlayerGames",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "HomeTeamId",
                table: "Games",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "AwayTeamId",
                table: "Games",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<bool>(
                name: "IsPostponed",
                table: "Games",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ESPNTitle",
                table: "ActiveRosterSpots",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "UsesEase",
                table: "ActiveRosterSpots",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddPrimaryKey(
                name: "PK_OwnershipPlayers",
                table: "OwnershipPlayers",
                columns: new[] { "GameDate", "PlayerId", "CategoriesCode", "LineupFrequency" });

            migrationBuilder.CreateTable(
                name: "CompletedTasks",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TaskId = table.Column<string>(nullable: true),
                    DateCompleted = table.Column<DateTime>(nullable: false),
                    WasSuccess = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompletedTasks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NHLGoalieGames",
                columns: table => new
                {
                    PlayerId = table.Column<int>(nullable: false),
                    GameId = table.Column<int>(nullable: false),
                    TeamId = table.Column<int>(nullable: false),
                    Started = table.Column<byte>(nullable: false),
                    Shifts = table.Column<byte>(nullable: false),
                    Credit = table.Column<string>(nullable: true),
                    Wins = table.Column<byte>(nullable: false),
                    Shutouts = table.Column<byte>(nullable: false),
                    Assists = table.Column<byte>(nullable: false),
                    PowerPlayTimeOnIce = table.Column<double>(nullable: false),
                    PowerPlayShotsAgainst = table.Column<byte>(nullable: false),
                    PowerPlayGoalsAgainst = table.Column<byte>(nullable: false),
                    PowerPlaySaves = table.Column<byte>(nullable: false),
                    ShorthandedTimeOnIce = table.Column<double>(nullable: false),
                    ShorthandedShotsAgainst = table.Column<byte>(nullable: false),
                    ShorthandedGoalsAgainst = table.Column<byte>(nullable: false),
                    ShorthandedPlaySaves = table.Column<byte>(nullable: false),
                    EvenstrengthTimeOnIce = table.Column<double>(nullable: false),
                    EvenstrengthShotsAgainst = table.Column<byte>(nullable: false),
                    EvenstrengthGoalsAgainst = table.Column<byte>(nullable: false),
                    EvenstrengthPlaySaves = table.Column<byte>(nullable: false),
                    PenaltyShotsAgainst = table.Column<byte>(nullable: false),
                    PenaltyGoalsAgainst = table.Column<byte>(nullable: false),
                    PenaltySaves = table.Column<byte>(nullable: false),
                    ShootoutShotsAgainst = table.Column<byte>(nullable: false),
                    ShootoutGoalsAgainst = table.Column<byte>(nullable: false),
                    ShootoutSaves = table.Column<byte>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NHLGoalieGames", x => new { x.PlayerId, x.GameId });
                    table.ForeignKey(
                        name: "FK_NHLGoalieGames_Games_GameId",
                        column: x => x.GameId,
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NHLGoalieGames_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NHLGoalieGames_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NHLSkaterGames",
                columns: table => new
                {
                    PlayerId = table.Column<int>(nullable: false),
                    GameId = table.Column<int>(nullable: false),
                    TeamId = table.Column<int>(nullable: false),
                    Started = table.Column<byte>(nullable: false),
                    PowerPlayTimeOnIce = table.Column<double>(nullable: false),
                    PowerPlayShots = table.Column<byte>(nullable: false),
                    PowerPlayGoals = table.Column<byte>(nullable: false),
                    PowerPlayMissedShots = table.Column<byte>(nullable: false),
                    PowerPlayAssists = table.Column<byte>(nullable: false),
                    PowerPlayFaceoffsWon = table.Column<byte>(nullable: false),
                    PowerPlayFaceoffsLost = table.Column<byte>(nullable: false),
                    ShorthandedTimeOnIce = table.Column<double>(nullable: false),
                    ShorthandedShots = table.Column<byte>(nullable: false),
                    ShorthandedGoals = table.Column<byte>(nullable: false),
                    ShorthandedMissedShots = table.Column<byte>(nullable: false),
                    ShorthandedAssists = table.Column<byte>(nullable: false),
                    ShorthandedFaceoffsWon = table.Column<byte>(nullable: false),
                    ShorthandedFaceoffsLost = table.Column<byte>(nullable: false),
                    EvenstrengthTimeOnIce = table.Column<double>(nullable: false),
                    EvenstrengthShots = table.Column<byte>(nullable: false),
                    EvenstrengthGoals = table.Column<byte>(nullable: false),
                    EvenstrengthMissedShots = table.Column<byte>(nullable: false),
                    EvenstrengthAssists = table.Column<byte>(nullable: false),
                    EvenstrengthFaceoffsWon = table.Column<byte>(nullable: false),
                    EvenstrengthFaceoffsLost = table.Column<byte>(nullable: false),
                    PenaltyShots = table.Column<byte>(nullable: false),
                    PenaltyGoals = table.Column<byte>(nullable: false),
                    PenaltyMissedShots = table.Column<byte>(nullable: false),
                    ShootoutShots = table.Column<byte>(nullable: false),
                    ShootoutGoals = table.Column<byte>(nullable: false),
                    ShootoutMissedShots = table.Column<byte>(nullable: false),
                    Penalties = table.Column<byte>(nullable: false),
                    PenaltyMinutes = table.Column<double>(nullable: false),
                    BlockedAttempts = table.Column<byte>(nullable: false),
                    Hits = table.Column<byte>(nullable: false),
                    Giveaways = table.Column<byte>(nullable: false),
                    Takeaways = table.Column<byte>(nullable: false),
                    BlockedShots = table.Column<byte>(nullable: false),
                    PlusMinus = table.Column<double>(nullable: false),
                    OvertimeGoals = table.Column<byte>(nullable: false),
                    OvertimeAssists = table.Column<byte>(nullable: false),
                    OvertimeShots = table.Column<byte>(nullable: false),
                    PenaltiesMajor = table.Column<byte>(nullable: false),
                    PenaltiesMinor = table.Column<byte>(nullable: false),
                    PenaltiesMisconduct = table.Column<byte>(nullable: false),
                    EmptynetGoals = table.Column<byte>(nullable: false),
                    Shifts = table.Column<byte>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NHLSkaterGames", x => new { x.PlayerId, x.GameId });
                    table.ForeignKey(
                        name: "FK_NHLSkaterGames_Games_GameId",
                        column: x => x.GameId,
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NHLSkaterGames_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NHLSkaterGames_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlayerGameStateTypes",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(nullable: true),
                    Descrition = table.Column<string>(nullable: true),
                    Icon = table.Column<string>(nullable: true),
                    TextColor = table.Column<string>(nullable: true),
                    BackgroundColor = table.Column<string>(nullable: true),
                    IsStarter = table.Column<bool>(nullable: false),
                    IsBench = table.Column<bool>(nullable: false),
                    ShowLockAfterStart = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerGameStateTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PlayerGameStates",
                columns: table => new
                {
                    GameId = table.Column<int>(nullable: false),
                    PlayerId = table.Column<int>(nullable: false),
                    TeamId = table.Column<int>(nullable: false),
                    PositionId = table.Column<int>(nullable: true),
                    PlayerGameStateTypeId = table.Column<int>(nullable: false),
                    DateAdded = table.Column<DateTime>(nullable: false),
                    Details = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerGameStates", x => new { x.GameId, x.PlayerId });
                    table.ForeignKey(
                        name: "FK_PlayerGameStates_Games_GameId",
                        column: x => x.GameId,
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlayerGameStates_PlayerGameStateTypes_PlayerGameStateTypeId",
                        column: x => x.PlayerGameStateTypeId,
                        principalTable: "PlayerGameStateTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlayerGameStates_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlayerGameStates_Positions_PositionId",
                        column: x => x.PositionId,
                        principalTable: "Positions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PlayerGameStates_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NBAPlayerGames_TeamId",
                table: "NBAPlayerGames",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_NHLGoalieGames_GameId",
                table: "NHLGoalieGames",
                column: "GameId");

            migrationBuilder.CreateIndex(
                name: "IX_NHLGoalieGames_TeamId",
                table: "NHLGoalieGames",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_NHLSkaterGames_GameId",
                table: "NHLSkaterGames",
                column: "GameId");

            migrationBuilder.CreateIndex(
                name: "IX_NHLSkaterGames_TeamId",
                table: "NHLSkaterGames",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerGameStates_PlayerGameStateTypeId",
                table: "PlayerGameStates",
                column: "PlayerGameStateTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerGameStates_PlayerId",
                table: "PlayerGameStates",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerGameStates_PositionId",
                table: "PlayerGameStates",
                column: "PositionId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerGameStates_TeamId",
                table: "PlayerGameStates",
                column: "TeamId");

            migrationBuilder.AddForeignKey(
                name: "FK_Games_Teams_AwayTeamId",
                table: "Games",
                column: "AwayTeamId",
                principalTable: "Teams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Games_Teams_HomeTeamId",
                table: "Games",
                column: "HomeTeamId",
                principalTable: "Teams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_NBAPlayerGames_Teams_TeamId",
                table: "NBAPlayerGames",
                column: "TeamId",
                principalTable: "Teams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Games_Teams_AwayTeamId",
                table: "Games");

            migrationBuilder.DropForeignKey(
                name: "FK_Games_Teams_HomeTeamId",
                table: "Games");

            migrationBuilder.DropForeignKey(
                name: "FK_NBAPlayerGames_Teams_TeamId",
                table: "NBAPlayerGames");

            migrationBuilder.DropTable(
                name: "CompletedTasks");

            migrationBuilder.DropTable(
                name: "NHLGoalieGames");

            migrationBuilder.DropTable(
                name: "NHLSkaterGames");

            migrationBuilder.DropTable(
                name: "PlayerGameStates");

            migrationBuilder.DropTable(
                name: "PlayerGameStateTypes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OwnershipPlayers",
                table: "OwnershipPlayers");

            migrationBuilder.DropIndex(
                name: "IX_NBAPlayerGames_TeamId",
                table: "NBAPlayerGames");

            migrationBuilder.DropColumn(
                name: "IsStreamable",
                table: "PlayerTypes");

            migrationBuilder.DropColumn(
                name: "IsDefaultDisplay",
                table: "PerValues");

            migrationBuilder.DropColumn(
                name: "LineupFrequency",
                table: "OwnershipPlayers");

            migrationBuilder.DropColumn(
                name: "AwayMoneyLine",
                table: "NFLGames");

            migrationBuilder.DropColumn(
                name: "HomeMoneyLine",
                table: "NFLGames");

            migrationBuilder.DropColumn(
                name: "TeamId",
                table: "NBAPlayerGames");

            migrationBuilder.DropColumn(
                name: "IsPostponed",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "ESPNTitle",
                table: "ActiveRosterSpots");

            migrationBuilder.DropColumn(
                name: "UsesEase",
                table: "ActiveRosterSpots");

            migrationBuilder.AlterColumn<int>(
                name: "HomeTeamId",
                table: "Games",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "AwayTeamId",
                table: "Games",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_OwnershipPlayers",
                table: "OwnershipPlayers",
                columns: new[] { "GameDate", "PlayerId", "CategoriesCode" });

            migrationBuilder.AddForeignKey(
                name: "FK_Games_Teams_AwayTeamId",
                table: "Games",
                column: "AwayTeamId",
                principalTable: "Teams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Games_Teams_HomeTeamId",
                table: "Games",
                column: "HomeTeamId",
                principalTable: "Teams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
