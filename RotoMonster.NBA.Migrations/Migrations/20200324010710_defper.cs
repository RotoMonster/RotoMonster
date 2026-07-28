using Microsoft.EntityFrameworkCore.Migrations;

namespace RotoMonster.Data.Migrations
{
    public partial class defper : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Bats",
                table: "Players",
                maxLength: 1,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Throws",
                table: "Players",
                maxLength: 1,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDefault",
                table: "PerValues",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Bats",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "Throws",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "IsDefault",
                table: "PerValues");
        }
    }
}
