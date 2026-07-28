using Microsoft.EntityFrameworkCore.Migrations;

namespace RotoMonster.NFL.Migrations.Migrations
{
    public partial class _082521_1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CategoriesCode",
                table: "UserLeaguePlayerTypes");

            migrationBuilder.DropColumn(
                name: "CategoriesCode",
                table: "OwnershipPlayers");

            migrationBuilder.DropColumn(
                name: "CategoriesCode",
                table: "DraftPlayerTypes");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CategoriesCode",
                table: "UserLeaguePlayerTypes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CategoriesCode",
                table: "OwnershipPlayers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CategoriesCode",
                table: "DraftPlayerTypes",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
