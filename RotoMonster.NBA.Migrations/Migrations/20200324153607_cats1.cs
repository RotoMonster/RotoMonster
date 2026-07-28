using Microsoft.EntityFrameworkCore.Migrations;

namespace RotoMonster.Data.Migrations
{
    public partial class cats1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FullInnings",
                table: "MLBPitcherGames");

            migrationBuilder.DropColumn(
                name: "ThirdInnings",
                table: "MLBPitcherGames");

            migrationBuilder.DropColumn(
                name: "Hits",
                table: "MLBHitterGames");

            migrationBuilder.DropColumn(
                name: "SBCaught",
                table: "MLBHitterGames");

            migrationBuilder.AddColumn<double>(
                name: "Innings",
                table: "MLBPitcherGames",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<byte>(
                name: "CS",
                table: "MLBHitterGames",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<byte>(
                name: "H",
                table: "MLBHitterGames",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<string>(
                name: "SourceField",
                table: "Categories",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Innings",
                table: "MLBPitcherGames");

            migrationBuilder.DropColumn(
                name: "CS",
                table: "MLBHitterGames");

            migrationBuilder.DropColumn(
                name: "H",
                table: "MLBHitterGames");

            migrationBuilder.DropColumn(
                name: "SourceField",
                table: "Categories");

            migrationBuilder.AddColumn<byte>(
                name: "FullInnings",
                table: "MLBPitcherGames",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<byte>(
                name: "ThirdInnings",
                table: "MLBPitcherGames",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<byte>(
                name: "Hits",
                table: "MLBHitterGames",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<byte>(
                name: "SBCaught",
                table: "MLBHitterGames",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);
        }
    }
}
