using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RotoMonster.Data.Migrations
{
    public partial class _220823_1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ESPNCode",
                table: "Sports",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ESPNYear",
                table: "Seasons",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<byte>(
                name: "FieldGoals0to39",
                table: "NFLKickerGames",
                type: "tinyint",
                nullable: true);

            migrationBuilder.AddColumn<byte>(
                name: "Points14to17",
                table: "NFLDefenseGames",
                type: "tinyint",
                nullable: true);

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
                name: "ESPNCode",
                table: "Sports");

            migrationBuilder.DropColumn(
                name: "ESPNYear",
                table: "Seasons");

            migrationBuilder.DropColumn(
                name: "FieldGoals0to39",
                table: "NFLKickerGames");

            migrationBuilder.DropColumn(
                name: "Points14to17",
                table: "NFLDefenseGames");

            migrationBuilder.DropColumn(
                name: "Points35to45",
                table: "NFLDefenseGames");

            migrationBuilder.DropColumn(
                name: "Points46",
                table: "NFLDefenseGames");
        }
    }
}
