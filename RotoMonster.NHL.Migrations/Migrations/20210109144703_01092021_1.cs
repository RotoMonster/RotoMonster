using Microsoft.EntityFrameworkCore.Migrations;

namespace RotoMonster.NHL.Migrations.Migrations
{
    public partial class _01092021_1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_NHLSkaterGames_GameId",
                table: "NHLSkaterGames",
                column: "GameId");

            migrationBuilder.CreateIndex(
                name: "IX_NHLSkaterGames_TeamId",
                table: "NHLSkaterGames",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_NHLGoalieGames_GameId",
                table: "NHLGoalieGames",
                column: "GameId");

            migrationBuilder.CreateIndex(
                name: "IX_NHLGoalieGames_TeamId",
                table: "NHLGoalieGames",
                column: "TeamId");

            migrationBuilder.AddForeignKey(
                name: "FK_NHLGoalieGames_Games_GameId",
                table: "NHLGoalieGames",
                column: "GameId",
                principalTable: "Games",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_NHLGoalieGames_Players_PlayerId",
                table: "NHLGoalieGames",
                column: "PlayerId",
                principalTable: "Players",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_NHLGoalieGames_Teams_TeamId",
                table: "NHLGoalieGames",
                column: "TeamId",
                principalTable: "Teams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_NHLSkaterGames_Games_GameId",
                table: "NHLSkaterGames",
                column: "GameId",
                principalTable: "Games",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_NHLSkaterGames_Players_PlayerId",
                table: "NHLSkaterGames",
                column: "PlayerId",
                principalTable: "Players",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_NHLSkaterGames_Teams_TeamId",
                table: "NHLSkaterGames",
                column: "TeamId",
                principalTable: "Teams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NHLGoalieGames_Games_GameId",
                table: "NHLGoalieGames");

            migrationBuilder.DropForeignKey(
                name: "FK_NHLGoalieGames_Players_PlayerId",
                table: "NHLGoalieGames");

            migrationBuilder.DropForeignKey(
                name: "FK_NHLGoalieGames_Teams_TeamId",
                table: "NHLGoalieGames");

            migrationBuilder.DropForeignKey(
                name: "FK_NHLSkaterGames_Games_GameId",
                table: "NHLSkaterGames");

            migrationBuilder.DropForeignKey(
                name: "FK_NHLSkaterGames_Players_PlayerId",
                table: "NHLSkaterGames");

            migrationBuilder.DropForeignKey(
                name: "FK_NHLSkaterGames_Teams_TeamId",
                table: "NHLSkaterGames");

            migrationBuilder.DropIndex(
                name: "IX_NHLSkaterGames_GameId",
                table: "NHLSkaterGames");

            migrationBuilder.DropIndex(
                name: "IX_NHLSkaterGames_TeamId",
                table: "NHLSkaterGames");

            migrationBuilder.DropIndex(
                name: "IX_NHLGoalieGames_GameId",
                table: "NHLGoalieGames");

            migrationBuilder.DropIndex(
                name: "IX_NHLGoalieGames_TeamId",
                table: "NHLGoalieGames");
        }
    }
}
