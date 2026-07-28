using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace RotoMonster.Data.Migrations
{
    public partial class _111720_1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Games_Seasons_SeasonId",
                table: "Games");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OwnershipPlayers",
                table: "OwnershipPlayers");

            migrationBuilder.AddColumn<string>(
                name: "WaiverRule",
                table: "UserLeagues",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WaiverType",
                table: "UserLeagues",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SportRadarId",
                table: "Teams",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LogoColor",
                table: "Sports",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MenuColor",
                table: "Sports",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SportType",
                table: "Sports",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActualPosition",
                table: "Positions",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "CategoriesCode",
                table: "OwnershipPlayers",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "IRCount",
                table: "OwnershipPlayers",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TeamId",
                table: "MLBPitcherGames",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TeamId",
                table: "MLBHitterGames",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "SeasonId",
                table: "Games",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsFinished",
                table: "Games",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FanTraxGroup",
                table: "Categories",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDefaultDisplayCategory",
                table: "Categories",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddPrimaryKey(
                name: "PK_OwnershipPlayers",
                table: "OwnershipPlayers",
                columns: new[] { "GameDate", "PlayerId", "CategoriesCode" });

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
                    Sacks = table.Column<byte>(nullable: true),
                    Interceptions = table.Column<byte>(nullable: true),
                    FumbleRecoveries = table.Column<byte>(nullable: true),
                    Touchdowns = table.Column<byte>(nullable: true),
                    Safeties = table.Column<byte>(nullable: true),
                    BlockedKicks = table.Column<byte>(nullable: true),
                    XpReturned = table.Column<byte>(nullable: true),
                    Points = table.Column<byte>(nullable: true),
                    PassAttempts = table.Column<byte>(nullable: true),
                    PassCompletion = table.Column<byte>(nullable: true),
                    PassYards = table.Column<short>(nullable: true),
                    PassTouchdowns = table.Column<byte>(nullable: true),
                    RushAttempts = table.Column<byte>(nullable: true),
                    RushYards = table.Column<short>(nullable: true),
                    RushTouchdowns = table.Column<byte>(nullable: true),
                    ReceivingAirYards = table.Column<short>(nullable: true),
                    PassSacks = table.Column<byte>(nullable: true),
                    Minutes = table.Column<double>(nullable: true),
                    Points0 = table.Column<byte>(nullable: true),
                    Points1to6 = table.Column<byte>(nullable: true),
                    Points7to13 = table.Column<byte>(nullable: true),
                    Points14to20 = table.Column<byte>(nullable: true),
                    Points21to27 = table.Column<byte>(nullable: true),
                    Points28to34 = table.Column<byte>(nullable: true),
                    Points35 = table.Column<byte>(nullable: true),
                    Points2to10 = table.Column<byte>(nullable: true),
                    Points11to20 = table.Column<byte>(nullable: true)
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
                name: "PlayerStatusTagTypes",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(nullable: true),
                    Icon = table.Column<string>(nullable: true),
                    IsDefault = table.Column<bool>(nullable: true),
                    DisplayOrder = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerStatusTagTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PlayerStatusTypes",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(nullable: true),
                    BackgroundColor = table.Column<string>(nullable: true),
                    TextColor = table.Column<string>(nullable: true),
                    TextFormat = table.Column<string>(nullable: true),
                    AutoClear = table.Column<bool>(nullable: true),
                    UsesDate = table.Column<bool>(nullable: true),
                    ShowInDaily = table.Column<bool>(nullable: true),
                    AllowFilter = table.Column<bool>(nullable: true),
                    AppliesToNextGame = table.Column<bool>(nullable: true),
                    IsInGame = table.Column<bool>(nullable: true),
                    IsUndetermined = table.Column<bool>(nullable: true),
                    ShowOnPlayerProfile = table.Column<bool>(nullable: true),
                    EndOfGameMissedPlayerStatusTypeId = table.Column<int>(nullable: true),
                    EndOfGamePlayedPlayerStatusTypeId = table.Column<int>(nullable: true),
                    TweetTemplate = table.Column<string>(nullable: true),
                    UpdateTemplate = table.Column<string>(nullable: true),
                    PlayType = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerStatusTypes", x => x.Id);
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

            migrationBuilder.CreateTable(
                name: "UserLeagueWaiverPlayers",
                columns: table => new
                {
                    UserLeagueId = table.Column<int>(nullable: false),
                    PlayerId = table.Column<int>(nullable: false),
                    AddedDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserLeagueWaiverPlayers", x => new { x.UserLeagueId, x.PlayerId });
                    table.ForeignKey(
                        name: "FK_UserLeagueWaiverPlayers_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserLeagueWaiverPlayers_UserLeagues_UserLeagueId",
                        column: x => x.UserLeagueId,
                        principalTable: "UserLeagues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlayerStatuses",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlayerId = table.Column<int>(nullable: false),
                    GameId = table.Column<int>(nullable: true),
                    IsActive = table.Column<bool>(nullable: false),
                    PlayerStatusTypeId = table.Column<int>(nullable: false),
                    PlayerStatusTagTypeId = table.Column<int>(nullable: true),
                    OwningUserId = table.Column<string>(nullable: true),
                    DateAdded = table.Column<DateTime>(nullable: false),
                    DateDeactivated = table.Column<DateTime>(nullable: true),
                    Comment = table.Column<string>(nullable: true),
                    Subject = table.Column<string>(nullable: true),
                    Source = table.Column<string>(nullable: true),
                    SourceUrl = table.Column<string>(nullable: true),
                    DateDeleted = table.Column<DateTime>(nullable: true),
                    DeletedByUserId = table.Column<string>(nullable: true),
                    GamePercent = table.Column<short>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerStatuses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlayerStatuses_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlayerStatuses_PlayerStatusTagTypes_PlayerStatusTagTypeId",
                        column: x => x.PlayerStatusTagTypeId,
                        principalTable: "PlayerStatusTagTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PlayerStatuses_PlayerStatusTypes_PlayerStatusTypeId",
                        column: x => x.PlayerStatusTypeId,
                        principalTable: "PlayerStatusTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NFLKickerGames_GameId",
                table: "NFLKickerGames",
                column: "GameId");

            migrationBuilder.CreateIndex(
                name: "IX_MLBPitcherGames_TeamId",
                table: "MLBPitcherGames",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_MLBHitterGames_TeamId",
                table: "MLBHitterGames",
                column: "TeamId");

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

            migrationBuilder.CreateIndex(
                name: "IX_PlayerStatuses_PlayerId",
                table: "PlayerStatuses",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerStatuses_PlayerStatusTagTypeId",
                table: "PlayerStatuses",
                column: "PlayerStatusTagTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerStatuses_PlayerStatusTypeId",
                table: "PlayerStatuses",
                column: "PlayerStatusTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_UserLeaguePlayerTypes_PlayerTypeId",
                table: "UserLeaguePlayerTypes",
                column: "PlayerTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_UserLeagueWaiverPlayers_PlayerId",
                table: "UserLeagueWaiverPlayers",
                column: "PlayerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Games_Seasons_SeasonId",
                table: "Games",
                column: "SeasonId",
                principalTable: "Seasons",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MLBHitterGames_Teams_TeamId",
                table: "MLBHitterGames",
                column: "TeamId",
                principalTable: "Teams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MLBPitcherGames_Teams_TeamId",
                table: "MLBPitcherGames",
                column: "TeamId",
                principalTable: "Teams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_NFLKickerGames_Games_GameId",
                table: "NFLKickerGames",
                column: "GameId",
                principalTable: "Games",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_NFLKickerGames_Players_PlayerId",
                table: "NFLKickerGames",
                column: "PlayerId",
                principalTable: "Players",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Games_Seasons_SeasonId",
                table: "Games");

            migrationBuilder.DropForeignKey(
                name: "FK_MLBHitterGames_Teams_TeamId",
                table: "MLBHitterGames");

            migrationBuilder.DropForeignKey(
                name: "FK_MLBPitcherGames_Teams_TeamId",
                table: "MLBPitcherGames");

            migrationBuilder.DropForeignKey(
                name: "FK_NFLKickerGames_Games_GameId",
                table: "NFLKickerGames");

            migrationBuilder.DropForeignKey(
                name: "FK_NFLKickerGames_Players_PlayerId",
                table: "NFLKickerGames");

            migrationBuilder.DropTable(
                name: "DraftPlayerTypes");

            migrationBuilder.DropTable(
                name: "ExtraAnalysisLeagues");

            migrationBuilder.DropTable(
                name: "NFLDefenseGames");

            migrationBuilder.DropTable(
                name: "NFLGames");

            migrationBuilder.DropTable(
                name: "PlayerStatuses");

            migrationBuilder.DropTable(
                name: "TeamAliases");

            migrationBuilder.DropTable(
                name: "UserLeagueImportErrors");

            migrationBuilder.DropTable(
                name: "UserLeagueMissingPlayers");

            migrationBuilder.DropTable(
                name: "UserLeaguePlayerTypes");

            migrationBuilder.DropTable(
                name: "UserLeagueWaiverPlayers");

            migrationBuilder.DropTable(
                name: "PlayerStatusTagTypes");

            migrationBuilder.DropTable(
                name: "PlayerStatusTypes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OwnershipPlayers",
                table: "OwnershipPlayers");

            migrationBuilder.DropIndex(
                name: "IX_NFLKickerGames_GameId",
                table: "NFLKickerGames");

            migrationBuilder.DropIndex(
                name: "IX_MLBPitcherGames_TeamId",
                table: "MLBPitcherGames");

            migrationBuilder.DropIndex(
                name: "IX_MLBHitterGames_TeamId",
                table: "MLBHitterGames");

            migrationBuilder.DropColumn(
                name: "WaiverRule",
                table: "UserLeagues");

            migrationBuilder.DropColumn(
                name: "WaiverType",
                table: "UserLeagues");

            migrationBuilder.DropColumn(
                name: "SportRadarId",
                table: "Teams");

            migrationBuilder.DropColumn(
                name: "LogoColor",
                table: "Sports");

            migrationBuilder.DropColumn(
                name: "MenuColor",
                table: "Sports");

            migrationBuilder.DropColumn(
                name: "SportType",
                table: "Sports");

            migrationBuilder.DropColumn(
                name: "IsActualPosition",
                table: "Positions");

            migrationBuilder.DropColumn(
                name: "CategoriesCode",
                table: "OwnershipPlayers");

            migrationBuilder.DropColumn(
                name: "IRCount",
                table: "OwnershipPlayers");

            migrationBuilder.DropColumn(
                name: "TeamId",
                table: "MLBPitcherGames");

            migrationBuilder.DropColumn(
                name: "TeamId",
                table: "MLBHitterGames");

            migrationBuilder.DropColumn(
                name: "FanTraxGroup",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "IsDefaultDisplayCategory",
                table: "Categories");

            migrationBuilder.AlterColumn<int>(
                name: "SeasonId",
                table: "Games",
                type: "int",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AlterColumn<bool>(
                name: "IsFinished",
                table: "Games",
                type: "bit",
                nullable: true,
                oldClrType: typeof(bool));

            migrationBuilder.AddPrimaryKey(
                name: "PK_OwnershipPlayers",
                table: "OwnershipPlayers",
                columns: new[] { "GameDate", "PlayerId" });

            migrationBuilder.AddForeignKey(
                name: "FK_Games_Seasons_SeasonId",
                table: "Games",
                column: "SeasonId",
                principalTable: "Seasons",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
