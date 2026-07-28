using Microsoft.EntityFrameworkCore.Migrations;

namespace RotoMonster.NHL.Migrations.Migrations
{
    public partial class _01082021_2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PenaltyAssists",
                table: "NHLSkaterGames");

            migrationBuilder.AlterColumn<double>(
                name: "PlusMinus",
                table: "NHLSkaterGames",
                nullable: false,
                oldClrType: typeof(byte),
                oldType: "tinyint");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<byte>(
                name: "PlusMinus",
                table: "NHLSkaterGames",
                type: "tinyint",
                nullable: false,
                oldClrType: typeof(double));

            migrationBuilder.AddColumn<byte>(
                name: "PenaltyAssists",
                table: "NHLSkaterGames",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);
        }
    }
}
