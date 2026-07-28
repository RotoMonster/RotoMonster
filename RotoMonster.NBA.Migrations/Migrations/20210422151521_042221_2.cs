using Microsoft.EntityFrameworkCore.Migrations;

namespace RotoMonster.Data.Migrations
{
    public partial class _042221_2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TeamId",
                table: "PlayerGameStates",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_PlayerGameStates_TeamId",
                table: "PlayerGameStates",
                column: "TeamId");

            migrationBuilder.AddForeignKey(
                name: "FK_PlayerGameStates_Teams_TeamId",
                table: "PlayerGameStates",
                column: "TeamId",
                principalTable: "Teams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlayerGameStates_Teams_TeamId",
                table: "PlayerGameStates");

            migrationBuilder.DropIndex(
                name: "IX_PlayerGameStates_TeamId",
                table: "PlayerGameStates");

            migrationBuilder.DropColumn(
                name: "TeamId",
                table: "PlayerGameStates");
        }
    }
}
