using Microsoft.EntityFrameworkCore.Migrations;

namespace RotoMonster.Data.Migrations
{
    public partial class ulc2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CBSId",
                table: "Categories",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ESPNId",
                table: "Categories",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FanTraxId",
                table: "Categories",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "YahooId",
                table: "Categories",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CBSId",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "ESPNId",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "FanTraxId",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "YahooId",
                table: "Categories");
        }
    }
}
