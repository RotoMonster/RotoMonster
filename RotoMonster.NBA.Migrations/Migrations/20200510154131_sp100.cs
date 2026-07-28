using Microsoft.EntityFrameworkCore.Migrations;

namespace RotoMonster.Data.Migrations
{
    public partial class sp100 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_SeasonPlayers_TeamId",
                table: "SeasonPlayers",
                column: "TeamId");

            migrationBuilder.AddForeignKey(
                name: "FK_SeasonPlayers_Teams_TeamId",
                table: "SeasonPlayers",
                column: "TeamId",
                principalTable: "Teams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SeasonPlayers_Teams_TeamId",
                table: "SeasonPlayers");

            migrationBuilder.DropIndex(
                name: "IX_SeasonPlayers_TeamId",
                table: "SeasonPlayers");
        }
    }
}
