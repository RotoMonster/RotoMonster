using Microsoft.EntityFrameworkCore.Migrations;

namespace RotoMonster.Data.Migrations
{
    public partial class pr2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDefault",
                table: "UserLeagues");

            migrationBuilder.AddColumn<bool>(
                name: "IsDefault",
                table: "FantasyProviders",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDefault",
                table: "FantasyProviders");

            migrationBuilder.AddColumn<bool>(
                name: "IsDefault",
                table: "UserLeagues",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
