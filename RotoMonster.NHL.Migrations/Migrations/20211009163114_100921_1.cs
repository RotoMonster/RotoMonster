using Microsoft.EntityFrameworkCore.Migrations;

namespace RotoMonster.NHL.Migrations.Migrations
{
    public partial class _100921_1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDynasty",
                table: "UserLeagues",
                nullable: false,
                defaultValue: false);

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
                table: "UserLeagues");

            migrationBuilder.DropColumn(
                name: "IsDynasty",
                table: "Drafts");
        }
    }
}
