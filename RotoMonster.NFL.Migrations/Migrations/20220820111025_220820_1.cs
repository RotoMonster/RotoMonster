using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RotoMonster.NFL.Migrations.Migrations
{
    public partial class _220820_1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte>(
                name: "Points35to45",
                table: "NFLDefenseGames",
                type: "tinyint",
                nullable: true);

            migrationBuilder.AddColumn<byte>(
                name: "Points46",
                table: "NFLDefenseGames",
                type: "tinyint",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Points35to45",
                table: "NFLDefenseGames");

            migrationBuilder.DropColumn(
                name: "Points46",
                table: "NFLDefenseGames");
        }
    }
}
