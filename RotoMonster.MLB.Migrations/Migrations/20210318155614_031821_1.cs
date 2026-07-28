using Microsoft.EntityFrameworkCore.Migrations;

namespace RotoMonster.MLB.Migrations.Migrations
{
    public partial class _031821_1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AwayMoneyLine",
                table: "NFLGames",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "HomeMoneyLine",
                table: "NFLGames",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AwayMoneyLine",
                table: "NFLGames");

            migrationBuilder.DropColumn(
                name: "HomeMoneyLine",
                table: "NFLGames");
        }
    }
}
