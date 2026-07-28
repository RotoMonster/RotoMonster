using Microsoft.EntityFrameworkCore.Migrations;

namespace RotoMonster.NFL.Migrations.Migrations
{
    public partial class _101320_1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_NFLKickerGames_GameId",
                table: "NFLKickerGames",
                column: "GameId");

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
                name: "FK_NFLKickerGames_Games_GameId",
                table: "NFLKickerGames");

            migrationBuilder.DropForeignKey(
                name: "FK_NFLKickerGames_Players_PlayerId",
                table: "NFLKickerGames");

            migrationBuilder.DropIndex(
                name: "IX_NFLKickerGames_GameId",
                table: "NFLKickerGames");
        }
    }
}
