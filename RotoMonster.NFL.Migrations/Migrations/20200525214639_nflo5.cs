using Microsoft.EntityFrameworkCore.Migrations;

namespace RotoMonster.NFL.Migrations.Migrations
{
    public partial class nflo5 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "RushYards",
                table: "NFLOffensiveGames",
                nullable: true,
                oldClrType: typeof(byte),
                oldType: "tinyint",
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "PassPocketTime",
                table: "NFLOffensiveGames",
                nullable: true,
                oldClrType: typeof(byte),
                oldType: "tinyint",
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<byte>(
                name: "RushYards",
                table: "NFLOffensiveGames",
                type: "tinyint",
                nullable: true,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AlterColumn<byte>(
                name: "PassPocketTime",
                table: "NFLOffensiveGames",
                type: "tinyint",
                nullable: true,
                oldClrType: typeof(double),
                oldNullable: true);
        }
    }
}
