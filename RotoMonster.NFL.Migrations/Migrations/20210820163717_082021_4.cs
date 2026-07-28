using Microsoft.EntityFrameworkCore.Migrations;

namespace RotoMonster.NFL.Migrations.Migrations
{
    public partial class _082021_4 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_DraftPlayerTypes_CategoriesStringId",
                table: "DraftPlayerTypes",
                column: "CategoriesStringId");

            migrationBuilder.AddForeignKey(
                name: "FK_DraftPlayerTypes_CategoriesStrings_CategoriesStringId",
                table: "DraftPlayerTypes",
                column: "CategoriesStringId",
                principalTable: "CategoriesStrings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DraftPlayerTypes_CategoriesStrings_CategoriesStringId",
                table: "DraftPlayerTypes");

            migrationBuilder.DropIndex(
                name: "IX_DraftPlayerTypes_CategoriesStringId",
                table: "DraftPlayerTypes");
        }
    }
}
