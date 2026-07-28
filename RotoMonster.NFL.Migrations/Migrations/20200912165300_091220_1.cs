using Microsoft.EntityFrameworkCore.Migrations;

namespace RotoMonster.NFL.Migrations.Migrations
{
    public partial class _091220_1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LogoColor",
                table: "Sports",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MenuColor",
                table: "Sports",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LogoColor",
                table: "Sports");

            migrationBuilder.DropColumn(
                name: "MenuColor",
                table: "Sports");
        }
    }
}
