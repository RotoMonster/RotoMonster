using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RotoMonster.MLB.Migrations.Migrations
{
    public partial class _220222_1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EntryFee",
                table: "UserLeagues",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsDynasty",
                table: "UserLeagues",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<double>(
                name: "HighPoints",
                table: "Sports",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "LowPoints",
                table: "Sports",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "MinutesPerPeriod",
                table: "Sports",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PeriodsPerGame",
                table: "Sports",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "AwayMoneyLine",
                table: "Games",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AwayScore",
                table: "Games",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "GameClock",
                table: "Games",
                type: "nvarchar(12)",
                maxLength: 12,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "HomeMoneyLine",
                table: "Games",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "HomeScore",
                table: "Games",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "HomeSpread",
                table: "Games",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "OverUnder",
                table: "Games",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "PercentComplete",
                table: "Games",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Period",
                table: "Games",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsDynasty",
                table: "Drafts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsMeasureCategory",
                table: "Categories",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsScoringAlertCategory",
                table: "Categories",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "Articles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SportRadarId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Byline = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Dateline = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Credit = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsInjury = table.Column<bool>(type: "bit", nullable: false),
                    IsTransaction = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Articles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GameScoringAlerts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GameId = table.Column<int>(type: "int", nullable: false),
                    PlayerId = table.Column<int>(type: "int", nullable: false),
                    TeamId = table.Column<int>(type: "int", nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    ScoringDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CurrentValue = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameScoringAlerts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GameScoringAlerts_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GameScoringAlerts_Games_GameId",
                        column: x => x.GameId,
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GameScoringAlerts_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GameScoringAlerts_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlayerGamePositionCategories",
                columns: table => new
                {
                    GameId = table.Column<int>(type: "int", nullable: false),
                    PlayerId = table.Column<int>(type: "int", nullable: false),
                    TeamId = table.Column<int>(type: "int", nullable: false),
                    PositionId = table.Column<int>(type: "int", nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    Percent = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerGamePositionCategories", x => new { x.PlayerId, x.GameId, x.TeamId, x.PositionId, x.CategoryId });
                    table.ForeignKey(
                        name: "FK_PlayerGamePositionCategories_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlayerGamePositionCategories_Games_GameId",
                        column: x => x.GameId,
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlayerGamePositionCategories_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlayerGamePositionCategories_Positions_PositionId",
                        column: x => x.PositionId,
                        principalTable: "Positions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlayerGamePositionCategories_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserOptionTypes",
                columns: table => new
                {
                    Id = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Abbreviation = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    OptionGroup = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    DisplayOrder = table.Column<short>(type: "smallint", nullable: false),
                    DataType = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: true),
                    DefaultValueBool = table.Column<bool>(type: "bit", nullable: true),
                    DefaultValueByte = table.Column<byte>(type: "tinyint", nullable: true),
                    DefaultValueShort = table.Column<short>(type: "smallint", nullable: true),
                    DefaultValueInt = table.Column<int>(type: "int", nullable: true),
                    DefaultValueDouble = table.Column<double>(type: "float", nullable: true),
                    DefaultValueString = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserOptionTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ArticleGames",
                columns: table => new
                {
                    ArticleId = table.Column<int>(type: "int", nullable: false),
                    GameId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArticleGames", x => new { x.ArticleId, x.GameId });
                    table.ForeignKey(
                        name: "FK_ArticleGames_Articles_ArticleId",
                        column: x => x.ArticleId,
                        principalTable: "Articles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ArticleGames_Games_GameId",
                        column: x => x.GameId,
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ArticlePlayers",
                columns: table => new
                {
                    ArticleId = table.Column<int>(type: "int", nullable: false),
                    PlayerId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArticlePlayers", x => new { x.ArticleId, x.PlayerId });
                    table.ForeignKey(
                        name: "FK_ArticlePlayers_Articles_ArticleId",
                        column: x => x.ArticleId,
                        principalTable: "Articles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ArticlePlayers_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ArticleTeams",
                columns: table => new
                {
                    ArticleId = table.Column<int>(type: "int", nullable: false),
                    TeamId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArticleTeams", x => new { x.ArticleId, x.TeamId });
                    table.ForeignKey(
                        name: "FK_ArticleTeams_Articles_ArticleId",
                        column: x => x.ArticleId,
                        principalTable: "Articles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ArticleTeams_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserOptions",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    UserOptionTypeId = table.Column<short>(type: "smallint", nullable: false),
                    ValueBool = table.Column<bool>(type: "bit", nullable: true),
                    ValueByte = table.Column<byte>(type: "tinyint", nullable: true),
                    ValueShort = table.Column<short>(type: "smallint", nullable: true),
                    ValueInt = table.Column<int>(type: "int", nullable: true),
                    ValueDouble = table.Column<double>(type: "float", nullable: true),
                    ValueString = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserOptions", x => new { x.UserId, x.UserOptionTypeId });
                    table.ForeignKey(
                        name: "FK_UserOptions_UserOptionTypes_UserOptionTypeId",
                        column: x => x.UserOptionTypeId,
                        principalTable: "UserOptionTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ArticleGames_GameId",
                table: "ArticleGames",
                column: "GameId");

            migrationBuilder.CreateIndex(
                name: "IX_ArticlePlayers_PlayerId",
                table: "ArticlePlayers",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_ArticleTeams_TeamId",
                table: "ArticleTeams",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_GameScoringAlerts_CategoryId",
                table: "GameScoringAlerts",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_GameScoringAlerts_GameId",
                table: "GameScoringAlerts",
                column: "GameId");

            migrationBuilder.CreateIndex(
                name: "IX_GameScoringAlerts_PlayerId",
                table: "GameScoringAlerts",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_GameScoringAlerts_TeamId",
                table: "GameScoringAlerts",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerGamePositionCategories_CategoryId",
                table: "PlayerGamePositionCategories",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerGamePositionCategories_GameId",
                table: "PlayerGamePositionCategories",
                column: "GameId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerGamePositionCategories_PositionId",
                table: "PlayerGamePositionCategories",
                column: "PositionId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerGamePositionCategories_TeamId",
                table: "PlayerGamePositionCategories",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_UserOptions_UserOptionTypeId",
                table: "UserOptions",
                column: "UserOptionTypeId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ArticleGames");

            migrationBuilder.DropTable(
                name: "ArticlePlayers");

            migrationBuilder.DropTable(
                name: "ArticleTeams");

            migrationBuilder.DropTable(
                name: "GameScoringAlerts");

            migrationBuilder.DropTable(
                name: "PlayerGamePositionCategories");

            migrationBuilder.DropTable(
                name: "UserOptions");

            migrationBuilder.DropTable(
                name: "Articles");

            migrationBuilder.DropTable(
                name: "UserOptionTypes");

            migrationBuilder.DropColumn(
                name: "EntryFee",
                table: "UserLeagues");

            migrationBuilder.DropColumn(
                name: "IsDynasty",
                table: "UserLeagues");

            migrationBuilder.DropColumn(
                name: "HighPoints",
                table: "Sports");

            migrationBuilder.DropColumn(
                name: "LowPoints",
                table: "Sports");

            migrationBuilder.DropColumn(
                name: "MinutesPerPeriod",
                table: "Sports");

            migrationBuilder.DropColumn(
                name: "PeriodsPerGame",
                table: "Sports");

            migrationBuilder.DropColumn(
                name: "AwayMoneyLine",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "AwayScore",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "GameClock",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "HomeMoneyLine",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "HomeScore",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "HomeSpread",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "OverUnder",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "PercentComplete",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "Period",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "IsDynasty",
                table: "Drafts");

            migrationBuilder.DropColumn(
                name: "IsMeasureCategory",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "IsScoringAlertCategory",
                table: "Categories");
        }
    }
}
