using Microsoft.EntityFrameworkCore.Migrations;

namespace RotoMonster.NFL.Migrations.Migrations
{
    public partial class ars11 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FantraxTitle",
                table: "ActiveRosterSpots",
                newName: "FanTraxTitle");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FanTraxTitle",
                table: "ActiveRosterSpots",
                newName: "FantraxTitle");
        }
    }
}
