using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace RotoMonster.MLB.Migrations.Migrations
{
    public partial class _111720_1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Games_Seasons_SeasonId",
                table: "Games");

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

            migrationBuilder.AlterColumn<byte>(
                name: "XpReturned",
                table: "NFLDefenseGames",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<byte>(
                name: "Touchdowns",
                table: "NFLDefenseGames",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<byte>(
                name: "Safeties",
                table: "NFLDefenseGames",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<byte>(
                name: "Sacks",
                table: "NFLDefenseGames",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<short>(
                name: "RushYards",
                table: "NFLDefenseGames",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<byte>(
                name: "RushTouchdowns",
                table: "NFLDefenseGames",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<byte>(
                name: "RushAttempts",
                table: "NFLDefenseGames",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<short>(
                name: "ReceivingAirYards",
                table: "NFLDefenseGames",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<byte>(
                name: "Points7to13",
                table: "NFLDefenseGames",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<byte>(
                name: "Points35",
                table: "NFLDefenseGames",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<byte>(
                name: "Points2to10",
                table: "NFLDefenseGames",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<byte>(
                name: "Points28to34",
                table: "NFLDefenseGames",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<byte>(
                name: "Points21to27",
                table: "NFLDefenseGames",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<byte>(
                name: "Points1to6",
                table: "NFLDefenseGames",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<byte>(
                name: "Points14to20",
                table: "NFLDefenseGames",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<byte>(
                name: "Points11to20",
                table: "NFLDefenseGames",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<byte>(
                name: "Points0",
                table: "NFLDefenseGames",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<byte>(
                name: "Points",
                table: "NFLDefenseGames",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<short>(
                name: "PassYards",
                table: "NFLDefenseGames",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<byte>(
                name: "PassTouchdowns",
                table: "NFLDefenseGames",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<byte>(
                name: "PassSacks",
                table: "NFLDefenseGames",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<byte>(
                name: "PassCompletion",
                table: "NFLDefenseGames",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<byte>(
                name: "PassAttempts",
                table: "NFLDefenseGames",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<double>(
                name: "Minutes",
                table: "NFLDefenseGames",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AlterColumn<byte>(
                name: "Interceptions",
                table: "NFLDefenseGames",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<byte>(
                name: "FumbleRecoveries",
                table: "NFLDefenseGames",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<byte>(
                name: "BlockedKicks",
                table: "NFLDefenseGames",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "SeasonId",
                table: "Games",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDefaultDisplayCategory",
                table: "Categories",
                nullable: false,
                defaultValue: false);

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
                name: "FK_NFLKickerGames_Games_GameId",
                table: "NFLKickerGames");

            migrationBuilder.DropForeignKey(
                name: "FK_NFLKickerGames_Players_PlayerId",
                table: "NFLKickerGames");

            migrationBuilder.DropTable(
                name: "PlayerStatuses");

            migrationBuilder.DropTable(
                name: "UserLeagueWaiverPlayers");

            migrationBuilder.DropTable(
                name: "PlayerStatusTagTypes");

            migrationBuilder.DropTable(
                name: "PlayerStatusTypes");

            migrationBuilder.DropIndex(
                name: "IX_NFLKickerGames_GameId",
                table: "NFLKickerGames");

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
                name: "IsDefaultDisplayCategory",
                table: "Categories");

            migrationBuilder.AlterColumn<int>(
                name: "XpReturned",
                table: "NFLDefenseGames",
                type: "int",
                nullable: false,
                oldClrType: typeof(byte),
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Touchdowns",
                table: "NFLDefenseGames",
                type: "int",
                nullable: false,
                oldClrType: typeof(byte),
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Safeties",
                table: "NFLDefenseGames",
                type: "int",
                nullable: false,
                oldClrType: typeof(byte),
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Sacks",
                table: "NFLDefenseGames",
                type: "int",
                nullable: false,
                oldClrType: typeof(byte),
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "RushYards",
                table: "NFLDefenseGames",
                type: "int",
                nullable: false,
                oldClrType: typeof(short),
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "RushTouchdowns",
                table: "NFLDefenseGames",
                type: "int",
                nullable: false,
                oldClrType: typeof(byte),
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "RushAttempts",
                table: "NFLDefenseGames",
                type: "int",
                nullable: false,
                oldClrType: typeof(byte),
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ReceivingAirYards",
                table: "NFLDefenseGames",
                type: "int",
                nullable: false,
                oldClrType: typeof(short),
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Points7to13",
                table: "NFLDefenseGames",
                type: "int",
                nullable: false,
                oldClrType: typeof(byte),
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Points35",
                table: "NFLDefenseGames",
                type: "int",
                nullable: false,
                oldClrType: typeof(byte),
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Points2to10",
                table: "NFLDefenseGames",
                type: "int",
                nullable: false,
                oldClrType: typeof(byte),
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Points28to34",
                table: "NFLDefenseGames",
                type: "int",
                nullable: false,
                oldClrType: typeof(byte),
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Points21to27",
                table: "NFLDefenseGames",
                type: "int",
                nullable: false,
                oldClrType: typeof(byte),
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Points1to6",
                table: "NFLDefenseGames",
                type: "int",
                nullable: false,
                oldClrType: typeof(byte),
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Points14to20",
                table: "NFLDefenseGames",
                type: "int",
                nullable: false,
                oldClrType: typeof(byte),
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Points11to20",
                table: "NFLDefenseGames",
                type: "int",
                nullable: false,
                oldClrType: typeof(byte),
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Points0",
                table: "NFLDefenseGames",
                type: "int",
                nullable: false,
                oldClrType: typeof(byte),
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Points",
                table: "NFLDefenseGames",
                type: "int",
                nullable: false,
                oldClrType: typeof(byte),
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "PassYards",
                table: "NFLDefenseGames",
                type: "int",
                nullable: false,
                oldClrType: typeof(short),
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "PassTouchdowns",
                table: "NFLDefenseGames",
                type: "int",
                nullable: false,
                oldClrType: typeof(byte),
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "PassSacks",
                table: "NFLDefenseGames",
                type: "int",
                nullable: false,
                oldClrType: typeof(byte),
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "PassCompletion",
                table: "NFLDefenseGames",
                type: "int",
                nullable: false,
                oldClrType: typeof(byte),
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "PassAttempts",
                table: "NFLDefenseGames",
                type: "int",
                nullable: false,
                oldClrType: typeof(byte),
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "Minutes",
                table: "NFLDefenseGames",
                type: "float",
                nullable: false,
                oldClrType: typeof(double),
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Interceptions",
                table: "NFLDefenseGames",
                type: "int",
                nullable: false,
                oldClrType: typeof(byte),
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "FumbleRecoveries",
                table: "NFLDefenseGames",
                type: "int",
                nullable: false,
                oldClrType: typeof(byte),
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "BlockedKicks",
                table: "NFLDefenseGames",
                type: "int",
                nullable: false,
                oldClrType: typeof(byte),
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "SeasonId",
                table: "Games",
                type: "int",
                nullable: true,
                oldClrType: typeof(int));

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
