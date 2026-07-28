using Microsoft.EntityFrameworkCore.Migrations;

namespace RotoMonster.Data.Migrations
{
    public partial class arsx2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ActiveRosterSpots_PlayerTypes_PlayerTypeId",
                table: "ActiveRosterSpots");

            migrationBuilder.DropIndex(
                name: "IX_ActiveRosterSpots_PlayerTypeId",
                table: "ActiveRosterSpots");

            migrationBuilder.DropColumn(
                name: "PlayerTypeId",
                table: "ActiveRosterSpots");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PlayerTypeId",
                table: "ActiveRosterSpots",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ActiveRosterSpots_PlayerTypeId",
                table: "ActiveRosterSpots",
                column: "PlayerTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_ActiveRosterSpots_PlayerTypes_PlayerTypeId",
                table: "ActiveRosterSpots",
                column: "PlayerTypeId",
                principalTable: "PlayerTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
