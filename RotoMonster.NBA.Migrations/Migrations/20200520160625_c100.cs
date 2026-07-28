using Microsoft.EntityFrameworkCore.Migrations;

namespace RotoMonster.Data.Migrations
{
    public partial class c100 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PerDisplayFormat",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "TotalDisplayFormat",
                table: "Categories");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PerDisplayFormat",
                table: "Categories",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TotalDisplayFormat",
                table: "Categories",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
