using Microsoft.EntityFrameworkCore.Migrations;

namespace RotoMonster.NHL.Migrations.Migrations
{
    public partial class _012921_1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDefaultDisplay",
                table: "PerValues",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ESPNTitle",
                table: "ActiveRosterSpots",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDefaultDisplay",
                table: "PerValues");

            migrationBuilder.DropColumn(
                name: "ESPNTitle",
                table: "ActiveRosterSpots");
        }
    }
}
