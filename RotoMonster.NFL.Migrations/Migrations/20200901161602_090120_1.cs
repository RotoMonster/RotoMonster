using Microsoft.EntityFrameworkCore.Migrations;

namespace RotoMonster.NFL.Migrations.Migrations
{
    public partial class _090120_1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CategoriesCode",
                table: "UserLeagues");

            migrationBuilder.AddColumn<string>(
                name: "FanTraxGroup",
                table: "Categories",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FanTraxGroup",
                table: "Categories");

            migrationBuilder.AddColumn<string>(
                name: "CategoriesCode",
                table: "UserLeagues",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
