using Microsoft.EntityFrameworkCore.Migrations;

namespace RotoMonster.Data.Migrations
{
    public partial class ps1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_PositionSourcePlayers",
                table: "PositionSourcePlayers");

            migrationBuilder.AddColumn<int>(
                name: "SeasonId",
                table: "PositionSourcePlayers",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_PositionSourcePlayers",
                table: "PositionSourcePlayers",
                columns: new[] { "SeasonId", "PositionSourceId", "PlayerId", "PositionId" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_PositionSourcePlayers",
                table: "PositionSourcePlayers");

            migrationBuilder.DropColumn(
                name: "SeasonId",
                table: "PositionSourcePlayers");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PositionSourcePlayers",
                table: "PositionSourcePlayers",
                columns: new[] { "PositionSourceId", "PlayerId", "PositionId" });
        }
    }
}
