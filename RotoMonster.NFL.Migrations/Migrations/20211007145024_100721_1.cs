using Microsoft.EntityFrameworkCore.Migrations;

namespace RotoMonster.NFL.Migrations.Migrations
{
    public partial class _100721_1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EntryFee",
                table: "UserLeagues",
                nullable: false,
                defaultValue: 0);

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
                name: "EntryFee",
                table: "UserLeagues");

            migrationBuilder.DropColumn(
                name: "IsDynasty",
                table: "UserLeagues");

            migrationBuilder.DropColumn(
                name: "IsDynasty",
                table: "Drafts");
        }
    }
}
