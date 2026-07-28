using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RotoMonster.NFL.Migrations.Migrations
{
    public partial class _220623_1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDefault",
                table: "UserLeagues",
                type: "bit",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDefault",
                table: "UserLeagues");
        }
    }
}
