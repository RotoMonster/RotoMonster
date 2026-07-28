using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RotoMonster.MLB.Migrations.Migrations
{
    public partial class _230404_1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsProbableStarter",
                table: "PlayerGameStateTypes",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsProbableStarter",
                table: "PlayerGameStateTypes");
        }
    }
}
