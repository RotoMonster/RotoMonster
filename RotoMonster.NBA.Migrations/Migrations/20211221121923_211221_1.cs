using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RotoMonster.Data.Migrations
{
    public partial class _211221_1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CurrentValue",
                table: "GameScoringAlerts",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentValue",
                table: "GameScoringAlerts");
        }
    }
}
