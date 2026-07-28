using Microsoft.EntityFrameworkCore.Migrations;

namespace RotoMonster.MLB.Migrations.Migrations
{
    public partial class _082520_1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CategoriesCode",
                table: "OwnershipPlayers",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CategoriesCode",
                table: "OwnershipPlayers");
        }
    }
}
