using Microsoft.EntityFrameworkCore.Migrations;

namespace RotoMonster.Data.Migrations
{
    public partial class _020221_1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDefaultDisplay",
                table: "PerValues",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDefaultDisplay",
                table: "PerValues");
        }
    }
}
