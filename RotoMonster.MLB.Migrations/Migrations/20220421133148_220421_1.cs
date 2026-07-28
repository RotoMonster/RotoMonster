using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RotoMonster.MLB.Migrations.Migrations
{
    public partial class _220421_1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "PerValuesSameAsTotal",
                table: "Categories",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PerValuesSameAsTotal",
                table: "Categories");
        }
    }
}
