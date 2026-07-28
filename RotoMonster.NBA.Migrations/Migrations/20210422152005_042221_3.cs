using Microsoft.EntityFrameworkCore.Migrations;

namespace RotoMonster.Data.Migrations
{
    public partial class _042221_3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PositionId",
                table: "PlayerGameStates",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlayerGameStates_PositionId",
                table: "PlayerGameStates",
                column: "PositionId");

            migrationBuilder.AddForeignKey(
                name: "FK_PlayerGameStates_Positions_PositionId",
                table: "PlayerGameStates",
                column: "PositionId",
                principalTable: "Positions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlayerGameStates_Positions_PositionId",
                table: "PlayerGameStates");

            migrationBuilder.DropIndex(
                name: "IX_PlayerGameStates_PositionId",
                table: "PlayerGameStates");

            migrationBuilder.DropColumn(
                name: "PositionId",
                table: "PlayerGameStates");
        }
    }
}
