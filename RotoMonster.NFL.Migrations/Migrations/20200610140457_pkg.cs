using Microsoft.EntityFrameworkCore.Migrations;

namespace RotoMonster.NFL.Migrations.Migrations
{
    public partial class pkg : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NFLKickerGames",
                columns: table => new
                {
                    PlayerId = table.Column<int>(nullable: false),
                    GameId = table.Column<int>(nullable: false),
                    TeamId = table.Column<int>(nullable: false),
                    FieldGoals = table.Column<byte>(nullable: true),
                    FieldGoalsMade = table.Column<byte>(nullable: true),
                    FieldGoalsBlocked = table.Column<byte>(nullable: true),
                    FieldGoalsYards = table.Column<byte>(nullable: true),
                    FieldGoalsLongest = table.Column<byte>(nullable: true),
                    ExtraPointsAttempts = table.Column<byte>(nullable: true),
                    ExtraPointsBlocked = table.Column<byte>(nullable: true),
                    ExtraPointsMade = table.Column<byte>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NFLKickerGames", x => new { x.PlayerId, x.GameId });
                });

            migrationBuilder.CreateIndex(
                name: "IX_NFLOffensiveGames_GameId",
                table: "NFLOffensiveGames",
                column: "GameId");

            migrationBuilder.CreateIndex(
                name: "IX_NBAPlayerGames_GameId",
                table: "NBAPlayerGames",
                column: "GameId");

            migrationBuilder.AddForeignKey(
                name: "FK_NBAPlayerGames_Games_GameId",
                table: "NBAPlayerGames",
                column: "GameId",
                principalTable: "Games",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_NBAPlayerGames_Players_PlayerId",
                table: "NBAPlayerGames",
                column: "PlayerId",
                principalTable: "Players",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_NFLOffensiveGames_Games_GameId",
                table: "NFLOffensiveGames",
                column: "GameId",
                principalTable: "Games",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_NFLOffensiveGames_Players_PlayerId",
                table: "NFLOffensiveGames",
                column: "PlayerId",
                principalTable: "Players",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NBAPlayerGames_Games_GameId",
                table: "NBAPlayerGames");

            migrationBuilder.DropForeignKey(
                name: "FK_NBAPlayerGames_Players_PlayerId",
                table: "NBAPlayerGames");

            migrationBuilder.DropForeignKey(
                name: "FK_NFLOffensiveGames_Games_GameId",
                table: "NFLOffensiveGames");

            migrationBuilder.DropForeignKey(
                name: "FK_NFLOffensiveGames_Players_PlayerId",
                table: "NFLOffensiveGames");

            migrationBuilder.DropTable(
                name: "NFLKickerGames");

            migrationBuilder.DropIndex(
                name: "IX_NFLOffensiveGames_GameId",
                table: "NFLOffensiveGames");

            migrationBuilder.DropIndex(
                name: "IX_NBAPlayerGames_GameId",
                table: "NBAPlayerGames");
        }
    }
}
