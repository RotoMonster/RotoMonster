using Microsoft.EntityFrameworkCore.Migrations;

namespace RotoMonster.Data.Migrations
{
    public partial class dc2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_DisplayCategories_CategoryId",
                table: "DisplayCategories",
                column: "CategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_DisplayCategories_Categories_CategoryId",
                table: "DisplayCategories",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DisplayCategories_Categories_CategoryId",
                table: "DisplayCategories");

            migrationBuilder.DropIndex(
                name: "IX_DisplayCategories_CategoryId",
                table: "DisplayCategories");
        }
    }
}
