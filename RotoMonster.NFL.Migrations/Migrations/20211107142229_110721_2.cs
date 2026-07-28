using Microsoft.EntityFrameworkCore.Migrations;

namespace RotoMonster.NFL.Migrations.Migrations
{
    public partial class _110721_2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GameClock",
                table: "Games",
                maxLength: 12,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Period",
                table: "Games",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GameClock",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "Period",
                table: "Games");
        }
    }
}
