using Microsoft.EntityFrameworkCore.Migrations;

namespace RotoMonster.NFL.Migrations.Migrations
{
    public partial class psp200 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_PositionSourcePositions_PositionId",
                table: "PositionSourcePositions",
                column: "PositionId");

            migrationBuilder.AddForeignKey(
                name: "FK_PositionSourcePositions_Positions_PositionId",
                table: "PositionSourcePositions",
                column: "PositionId",
                principalTable: "Positions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PositionSourcePositions_PositionSources_PositionSourceId",
                table: "PositionSourcePositions",
                column: "PositionSourceId",
                principalTable: "PositionSources",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PositionSourcePositions_Positions_PositionId",
                table: "PositionSourcePositions");

            migrationBuilder.DropForeignKey(
                name: "FK_PositionSourcePositions_PositionSources_PositionSourceId",
                table: "PositionSourcePositions");

            migrationBuilder.DropIndex(
                name: "IX_PositionSourcePositions_PositionId",
                table: "PositionSourcePositions");
        }
    }
}
