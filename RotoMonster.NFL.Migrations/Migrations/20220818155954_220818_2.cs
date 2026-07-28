using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RotoMonster.NFL.Migrations.Migrations
{
    public partial class _220818_2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ESPNCode",
                table: "Sports",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ESPNCode",
                table: "Sports");
        }
    }
}
