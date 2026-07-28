using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RotoMonster.NFL.Migrations.Migrations
{
    public partial class _211121_2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MinutesPerPeriod",
                table: "Sports",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PeriodsPerGame",
                table: "Sports",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MinutesPerPeriod",
                table: "Sports");

            migrationBuilder.DropColumn(
                name: "PeriodsPerGame",
                table: "Sports");
        }
    }
}
