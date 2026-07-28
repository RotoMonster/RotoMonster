using Microsoft.EntityFrameworkCore.Migrations;

namespace RotoMonster.NFL.Migrations.Migrations
{
    public partial class _100320_4 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_PlayerStatuses_PlayerId",
                table: "PlayerStatuses",
                column: "PlayerId");

            migrationBuilder.AddForeignKey(
                name: "FK_PlayerStatuses_Players_PlayerId",
                table: "PlayerStatuses",
                column: "PlayerId",
                principalTable: "Players",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlayerStatuses_Players_PlayerId",
                table: "PlayerStatuses");

            migrationBuilder.DropIndex(
                name: "IX_PlayerStatuses_PlayerId",
                table: "PlayerStatuses");
        }
    }
}
