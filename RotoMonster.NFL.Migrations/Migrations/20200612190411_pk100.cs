using Microsoft.EntityFrameworkCore.Migrations;

namespace RotoMonster.NFL.Migrations.Migrations
{
    public partial class pk100 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte>(
                name: "FieldGoals0to19",
                table: "NFLKickerGames",
                nullable: true);

            migrationBuilder.AddColumn<byte>(
                name: "FieldGoals20to29",
                table: "NFLKickerGames",
                nullable: true);

            migrationBuilder.AddColumn<byte>(
                name: "FieldGoals30to39",
                table: "NFLKickerGames",
                nullable: true);

            migrationBuilder.AddColumn<byte>(
                name: "FieldGoals40to49",
                table: "NFLKickerGames",
                nullable: true);

            migrationBuilder.AddColumn<byte>(
                name: "FieldGoals50",
                table: "NFLKickerGames",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FieldGoals0to19",
                table: "NFLKickerGames");

            migrationBuilder.DropColumn(
                name: "FieldGoals20to29",
                table: "NFLKickerGames");

            migrationBuilder.DropColumn(
                name: "FieldGoals30to39",
                table: "NFLKickerGames");

            migrationBuilder.DropColumn(
                name: "FieldGoals40to49",
                table: "NFLKickerGames");

            migrationBuilder.DropColumn(
                name: "FieldGoals50",
                table: "NFLKickerGames");
        }
    }
}
