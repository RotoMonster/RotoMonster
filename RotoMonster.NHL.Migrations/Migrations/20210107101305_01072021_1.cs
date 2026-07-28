using Microsoft.EntityFrameworkCore.Migrations;

namespace RotoMonster.NHL.Migrations.Migrations
{
    public partial class _01072021_1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_OwnershipPlayers",
                table: "OwnershipPlayers");

            migrationBuilder.AddColumn<string>(
                name: "LineupFrequency",
                table: "OwnershipPlayers",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "TeamId",
                table: "NBAPlayerGames",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_OwnershipPlayers",
                table: "OwnershipPlayers",
                columns: new[] { "GameDate", "PlayerId", "CategoriesCode", "LineupFrequency" });

            migrationBuilder.CreateIndex(
                name: "IX_NBAPlayerGames_TeamId",
                table: "NBAPlayerGames",
                column: "TeamId");

            migrationBuilder.AddForeignKey(
                name: "FK_NBAPlayerGames_Teams_TeamId",
                table: "NBAPlayerGames",
                column: "TeamId",
                principalTable: "Teams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NBAPlayerGames_Teams_TeamId",
                table: "NBAPlayerGames");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OwnershipPlayers",
                table: "OwnershipPlayers");

            migrationBuilder.DropIndex(
                name: "IX_NBAPlayerGames_TeamId",
                table: "NBAPlayerGames");

            migrationBuilder.DropColumn(
                name: "LineupFrequency",
                table: "OwnershipPlayers");

            migrationBuilder.DropColumn(
                name: "TeamId",
                table: "NBAPlayerGames");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OwnershipPlayers",
                table: "OwnershipPlayers",
                columns: new[] { "GameDate", "PlayerId", "CategoriesCode" });
        }
    }
}
