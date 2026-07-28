using Microsoft.EntityFrameworkCore.Migrations;

namespace RotoMonster.Data.Migrations
{
    public partial class mlbg : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NBAPlayerGames_Games_GameId",
                table: "NBAPlayerGames");

            migrationBuilder.DropForeignKey(
                name: "FK_NBAPlayerGames_Players_PlayerId",
                table: "NBAPlayerGames");

            migrationBuilder.DropIndex(
                name: "IX_NBAPlayerGames_GameId",
                table: "NBAPlayerGames");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
        }
    }
}
