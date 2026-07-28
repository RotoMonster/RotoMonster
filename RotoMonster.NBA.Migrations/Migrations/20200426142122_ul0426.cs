using Microsoft.EntityFrameworkCore.Migrations;

namespace RotoMonster.Data.Migrations
{
    public partial class ul0426 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DisplayTitle",
                table: "UserLeagues",
                maxLength: 250,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PositionSourcePlayers_PositionId",
                table: "PositionSourcePlayers",
                column: "PositionId");

            migrationBuilder.AddForeignKey(
                name: "FK_PositionSourcePlayers_Positions_PositionId",
                table: "PositionSourcePlayers",
                column: "PositionId",
                principalTable: "Positions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PositionSourcePlayers_Positions_PositionId",
                table: "PositionSourcePlayers");

            migrationBuilder.DropIndex(
                name: "IX_PositionSourcePlayers_PositionId",
                table: "PositionSourcePlayers");

            migrationBuilder.DropColumn(
                name: "DisplayTitle",
                table: "UserLeagues");
        }
    }
}
