using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RotoMonster.MLB.Migrations.Migrations
{
    public partial class _220429_1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OnPageDetails",
                table: "Helpers",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OnPageDetails",
                table: "Helpers");
        }
    }
}
