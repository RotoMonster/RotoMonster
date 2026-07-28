using Microsoft.EntityFrameworkCore.Migrations;

namespace RotoMonster.NFL.Migrations.Migrations
{
    public partial class nflo3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PassTards",
                table: "NFLOffensiveGames");

            migrationBuilder.AddColumn<byte>(
                name: "PassYards",
                table: "NFLOffensiveGames",
                nullable: false,
                defaultValue: (byte)0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PassYards",
                table: "NFLOffensiveGames");

            migrationBuilder.AddColumn<byte>(
                name: "PassTards",
                table: "NFLOffensiveGames",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);
        }
    }
}
