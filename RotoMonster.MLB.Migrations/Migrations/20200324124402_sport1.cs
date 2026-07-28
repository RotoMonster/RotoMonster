using Microsoft.EntityFrameworkCore.Migrations;

namespace RotoMonster.MLB.Migrations.Migrations
{
    public partial class sport1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Sports",
                columns: table => new
                {
                    Title = table.Column<string>(maxLength: 3, nullable: true),
                    DivisionTitle = table.Column<string>(maxLength: 20, nullable: true),
                    UsesCategories = table.Column<bool>(nullable: false),
                    UsesPointsPerStat = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Sports");
        }
    }
}
