using Microsoft.EntityFrameworkCore.Migrations;

namespace RotoMonster.NFL.Migrations.Migrations
{
    public partial class nflo2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte>(
                name: "PassAttempts",
                table: "NFLOffensiveGames",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<byte>(
                name: "PassCompletions",
                table: "NFLOffensiveGames",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<byte>(
                name: "PassInt",
                table: "NFLOffensiveGames",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<byte>(
                name: "PassSackYards",
                table: "NFLOffensiveGames",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<byte>(
                name: "PassSacks",
                table: "NFLOffensiveGames",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<byte>(
                name: "PassTD",
                table: "NFLOffensiveGames",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<byte>(
                name: "PassTards",
                table: "NFLOffensiveGames",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<byte>(
                name: "RushAttempts",
                table: "NFLOffensiveGames",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<byte>(
                name: "RushYards",
                table: "NFLOffensiveGames",
                nullable: false,
                defaultValue: (byte)0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PassAttempts",
                table: "NFLOffensiveGames");

            migrationBuilder.DropColumn(
                name: "PassCompletions",
                table: "NFLOffensiveGames");

            migrationBuilder.DropColumn(
                name: "PassInt",
                table: "NFLOffensiveGames");

            migrationBuilder.DropColumn(
                name: "PassSackYards",
                table: "NFLOffensiveGames");

            migrationBuilder.DropColumn(
                name: "PassSacks",
                table: "NFLOffensiveGames");

            migrationBuilder.DropColumn(
                name: "PassTD",
                table: "NFLOffensiveGames");

            migrationBuilder.DropColumn(
                name: "PassTards",
                table: "NFLOffensiveGames");

            migrationBuilder.DropColumn(
                name: "RushAttempts",
                table: "NFLOffensiveGames");

            migrationBuilder.DropColumn(
                name: "RushYards",
                table: "NFLOffensiveGames");
        }
    }
}
