using Microsoft.EntityFrameworkCore.Migrations;

namespace RotoMonster.Data.Migrations
{
    public partial class ulz : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MyProviderTeamId",
                table: "UserLeagues",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MyProviderTeamId",
                table: "UserLeagues");
        }
    }
}
