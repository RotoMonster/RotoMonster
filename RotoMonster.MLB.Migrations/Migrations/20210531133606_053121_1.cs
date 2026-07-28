using Microsoft.EntityFrameworkCore.Migrations;

namespace RotoMonster.MLB.Migrations.Migrations
{
    public partial class _053121_1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsBench",
                table: "PlayerGameStateTypes",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsStarter",
                table: "PlayerGameStateTypes",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ShowLockAfterStart",
                table: "PlayerGameStateTypes",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsBench",
                table: "PlayerGameStateTypes");

            migrationBuilder.DropColumn(
                name: "IsStarter",
                table: "PlayerGameStateTypes");

            migrationBuilder.DropColumn(
                name: "ShowLockAfterStart",
                table: "PlayerGameStateTypes");
        }
    }
}
