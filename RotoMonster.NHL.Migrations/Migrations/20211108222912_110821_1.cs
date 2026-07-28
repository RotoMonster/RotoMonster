using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace RotoMonster.NHL.Migrations.Migrations
{
    public partial class _110821_1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GameClock",
                table: "Games",
                maxLength: 12,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PercentComplete",
                table: "Games",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Period",
                table: "Games",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Articles",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    UpdatedDate = table.Column<DateTime>(nullable: false),
                    Type = table.Column<string>(nullable: true),
                    Title = table.Column<string>(nullable: true),
                    SportRadarId = table.Column<string>(nullable: true),
                    Byline = table.Column<string>(nullable: true),
                    Dateline = table.Column<string>(nullable: true),
                    Credit = table.Column<string>(nullable: true),
                    Content = table.Column<string>(nullable: true),
                    IsInjury = table.Column<bool>(nullable: false),
                    IsTransaction = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Articles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ArticleGames",
                columns: table => new
                {
                    ArticleId = table.Column<int>(nullable: false),
                    GameId = table.Column<int>(nullable: false)
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
                    ArticleId = table.Column<int>(nullable: false),
                    PlayerId = table.Column<int>(nullable: false)
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
                    ArticleId = table.Column<int>(nullable: false),
                    TeamId = table.Column<int>(nullable: false)
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
                name: "Articles");

            migrationBuilder.DropColumn(
                name: "GameClock",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "PercentComplete",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "Period",
                table: "Games");
        }
    }
}
