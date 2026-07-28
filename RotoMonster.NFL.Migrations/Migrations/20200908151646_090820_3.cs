using Microsoft.EntityFrameworkCore.Migrations;

namespace RotoMonster.NFL.Migrations.Migrations
{
    public partial class _090820_3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NFLGames_Games_GameId1",
                table: "NFLGames");

            migrationBuilder.DropIndex(
                name: "IX_NFLGames_GameId1",
                table: "NFLGames");

            migrationBuilder.DropColumn(
                name: "GameId1",
                table: "NFLGames");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GameId1",
                table: "NFLGames",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_NFLGames_GameId1",
                table: "NFLGames",
                column: "GameId1");

            migrationBuilder.AddForeignKey(
                name: "FK_NFLGames_Games_GameId1",
                table: "NFLGames",
                column: "GameId1",
                principalTable: "Games",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
