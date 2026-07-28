using Microsoft.EntityFrameworkCore.Migrations;

namespace RotoMonster.NFL.Migrations.Migrations
{
    public partial class _082021_2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_UserLeaguePlayerTypes_CategoriesStringId",
                table: "UserLeaguePlayerTypes",
                column: "CategoriesStringId");

            migrationBuilder.CreateIndex(
                name: "IX_OwnershipPlayers_CategoriesStringId",
                table: "OwnershipPlayers",
                column: "CategoriesStringId");

            migrationBuilder.AddForeignKey(
                name: "FK_OwnershipPlayers_CategoriesStrings_CategoriesStringId",
                table: "OwnershipPlayers",
                column: "CategoriesStringId",
                principalTable: "CategoriesStrings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserLeaguePlayerTypes_CategoriesStrings_CategoriesStringId",
                table: "UserLeaguePlayerTypes",
                column: "CategoriesStringId",
                principalTable: "CategoriesStrings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OwnershipPlayers_CategoriesStrings_CategoriesStringId",
                table: "OwnershipPlayers");

            migrationBuilder.DropForeignKey(
                name: "FK_UserLeaguePlayerTypes_CategoriesStrings_CategoriesStringId",
                table: "UserLeaguePlayerTypes");

            migrationBuilder.DropIndex(
                name: "IX_UserLeaguePlayerTypes_CategoriesStringId",
                table: "UserLeaguePlayerTypes");

            migrationBuilder.DropIndex(
                name: "IX_OwnershipPlayers_CategoriesStringId",
                table: "OwnershipPlayers");
        }
    }
}
