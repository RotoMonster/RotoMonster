using Microsoft.EntityFrameworkCore.Migrations;

namespace RotoMonster.Data.Migrations
{
    public partial class xx : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CategoryPerValues",
                columns: table => new
                {
                    CategoryId = table.Column<int>(nullable: false),
                    PerValueId = table.Column<int>(nullable: false),
                    DisplayFormat = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoryPerValues", x => new { x.CategoryId, x.PerValueId });
                    table.ForeignKey(
                        name: "FK_CategoryPerValues_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CategoryPerValues_PerValues_PerValueId",
                        column: x => x.PerValueId,
                        principalTable: "PerValues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CategoryPerValues_PerValueId",
                table: "CategoryPerValues",
                column: "PerValueId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CategoryPerValues");
        }
    }
}
