using Microsoft.EntityFrameworkCore.Migrations;

namespace RotoMonster.MLB.Migrations.Migrations
{
    public partial class _082420_1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CategoriesCode",
                table: "UserLeagues",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CategoriesCode",
                table: "UserLeagues");
        }
    }
}
