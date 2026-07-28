using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RotoMonster.NFL.Migrations.Migrations
{
    public partial class _220820_2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FieldGoals0to39",
                table: "NFLKickerGames");

            migrationBuilder.DropColumn(
                name: "Points14to17",
                table: "NFLDefenseGames");
        }
    }
}
