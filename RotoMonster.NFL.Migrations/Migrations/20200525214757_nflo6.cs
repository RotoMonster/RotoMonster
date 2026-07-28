using Microsoft.EntityFrameworkCore.Migrations;

namespace RotoMonster.NFL.Migrations.Migrations
{
    public partial class nflo6 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "RushYardsLost",
                table: "NFLOffensiveGames",
                nullable: true,
                oldClrType: typeof(byte),
                oldType: "tinyint",
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<byte>(
                name: "RushYardsLost",
                table: "NFLOffensiveGames",
                type: "tinyint",
                nullable: true,
                oldClrType: typeof(int),
                oldNullable: true);
        }
    }
}
