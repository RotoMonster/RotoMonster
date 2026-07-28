using Microsoft.EntityFrameworkCore.Migrations;

namespace RotoMonster.Data.Migrations
{
    public partial class _111920_1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TeamId",
                table: "NBAPlayerGames",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_NBAPlayerGames_TeamId",
                table: "NBAPlayerGames",
                column: "TeamId");

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
                name: "FK_NBAPlayerGames_Teams_TeamId",
                table: "NBAPlayerGames");

            migrationBuilder.DropIndex(
                name: "IX_NBAPlayerGames_TeamId",
                table: "NBAPlayerGames");

            migrationBuilder.DropColumn(
                name: "TeamId",
                table: "NBAPlayerGames");
        }
    }
}
