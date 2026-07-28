using Microsoft.EntityFrameworkCore.Migrations;

namespace RotoMonster.NFL.Migrations.Migrations
{
    public partial class ars10 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FantraxTitle",
                table: "ActiveRosterSpots",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "YahooTitle",
                table: "ActiveRosterSpots",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FantraxTitle",
                table: "ActiveRosterSpots");

            migrationBuilder.DropColumn(
                name: "YahooTitle",
                table: "ActiveRosterSpots");
        }
    }
}
