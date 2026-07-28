using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RotoMonster.NHL.Migrations.Migrations
{
    public partial class _220102_1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AwayMoneyLine",
                table: "Games",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "HomeMoneyLine",
                table: "Games",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "HomeSpread",
                table: "Games",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "OverUnder",
                table: "Games",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AwayMoneyLine",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "HomeMoneyLine",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "HomeSpread",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "OverUnder",
                table: "Games");
        }
    }
}
