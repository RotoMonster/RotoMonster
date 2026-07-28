using Microsoft.EntityFrameworkCore.Migrations;

namespace RotoMonster.Data.Migrations
{
    public partial class _100521_2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDynasty",
                table: "Drafts",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDynasty",
                table: "Drafts");
        }
    }
}
