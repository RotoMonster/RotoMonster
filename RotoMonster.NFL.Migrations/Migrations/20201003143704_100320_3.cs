using Microsoft.EntityFrameworkCore.Migrations;

namespace RotoMonster.NFL.Migrations.Migrations
{
    public partial class _100320_3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_PlayerStatuses_PlayerStatusTagTypeId",
                table: "PlayerStatuses",
                column: "PlayerStatusTagTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerStatuses_PlayerStatusTypeId",
                table: "PlayerStatuses",
                column: "PlayerStatusTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_PlayerStatuses_PlayerStatusTagTypes_PlayerStatusTagTypeId",
                table: "PlayerStatuses",
                column: "PlayerStatusTagTypeId",
                principalTable: "PlayerStatusTagTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PlayerStatuses_PlayerStatusTypes_PlayerStatusTypeId",
                table: "PlayerStatuses",
                column: "PlayerStatusTypeId",
                principalTable: "PlayerStatusTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlayerStatuses_PlayerStatusTagTypes_PlayerStatusTagTypeId",
                table: "PlayerStatuses");

            migrationBuilder.DropForeignKey(
                name: "FK_PlayerStatuses_PlayerStatusTypes_PlayerStatusTypeId",
                table: "PlayerStatuses");

            migrationBuilder.DropIndex(
                name: "IX_PlayerStatuses_PlayerStatusTagTypeId",
                table: "PlayerStatuses");

            migrationBuilder.DropIndex(
                name: "IX_PlayerStatuses_PlayerStatusTypeId",
                table: "PlayerStatuses");
        }
    }
}
