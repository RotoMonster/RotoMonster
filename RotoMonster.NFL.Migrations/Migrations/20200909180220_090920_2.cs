using Microsoft.EntityFrameworkCore.Migrations;

namespace RotoMonster.NFL.Migrations.Migrations
{
    public partial class _090920_2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActualPositon",
                table: "Positions");

            migrationBuilder.AddColumn<bool>(
                name: "IsActualPosition",
                table: "Positions",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActualPosition",
                table: "Positions");

            migrationBuilder.AddColumn<bool>(
                name: "IsActualPositon",
                table: "Positions",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
