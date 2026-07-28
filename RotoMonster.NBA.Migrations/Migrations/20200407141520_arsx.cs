using Microsoft.EntityFrameworkCore.Migrations;

namespace RotoMonster.Data.Migrations
{
    public partial class arsx : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ActiveRosterSpotPositions_ActiveRosterSpot_ActiveRosterSpotId",
                table: "ActiveRosterSpotPositions");

            migrationBuilder.DropForeignKey(
                name: "FK_UserLeagueActiveRosterSpots_ActiveRosterSpot_ActiveRosterSpotId",
                table: "UserLeagueActiveRosterSpots");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ActiveRosterSpot",
                table: "ActiveRosterSpot");

            migrationBuilder.RenameTable(
                name: "ActiveRosterSpot",
                newName: "ActiveRosterSpots");

            migrationBuilder.AddColumn<int>(
                name: "PlayerTypeId",
                table: "ActiveRosterSpots",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ActiveRosterSpots",
                table: "ActiveRosterSpots",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_ActiveRosterSpots_PlayerTypeId",
                table: "ActiveRosterSpots",
                column: "PlayerTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_ActiveRosterSpotPositions_ActiveRosterSpots_ActiveRosterSpotId",
                table: "ActiveRosterSpotPositions",
                column: "ActiveRosterSpotId",
                principalTable: "ActiveRosterSpots",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ActiveRosterSpots_PlayerTypes_PlayerTypeId",
                table: "ActiveRosterSpots",
                column: "PlayerTypeId",
                principalTable: "PlayerTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserLeagueActiveRosterSpots_ActiveRosterSpots_ActiveRosterSpotId",
                table: "UserLeagueActiveRosterSpots",
                column: "ActiveRosterSpotId",
                principalTable: "ActiveRosterSpots",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ActiveRosterSpotPositions_ActiveRosterSpots_ActiveRosterSpotId",
                table: "ActiveRosterSpotPositions");

            migrationBuilder.DropForeignKey(
                name: "FK_ActiveRosterSpots_PlayerTypes_PlayerTypeId",
                table: "ActiveRosterSpots");

            migrationBuilder.DropForeignKey(
                name: "FK_UserLeagueActiveRosterSpots_ActiveRosterSpots_ActiveRosterSpotId",
                table: "UserLeagueActiveRosterSpots");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ActiveRosterSpots",
                table: "ActiveRosterSpots");

            migrationBuilder.DropIndex(
                name: "IX_ActiveRosterSpots_PlayerTypeId",
                table: "ActiveRosterSpots");

            migrationBuilder.DropColumn(
                name: "PlayerTypeId",
                table: "ActiveRosterSpots");

            migrationBuilder.RenameTable(
                name: "ActiveRosterSpots",
                newName: "ActiveRosterSpot");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ActiveRosterSpot",
                table: "ActiveRosterSpot",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ActiveRosterSpotPositions_ActiveRosterSpot_ActiveRosterSpotId",
                table: "ActiveRosterSpotPositions",
                column: "ActiveRosterSpotId",
                principalTable: "ActiveRosterSpot",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserLeagueActiveRosterSpots_ActiveRosterSpot_ActiveRosterSpotId",
                table: "UserLeagueActiveRosterSpots",
                column: "ActiveRosterSpotId",
                principalTable: "ActiveRosterSpot",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
