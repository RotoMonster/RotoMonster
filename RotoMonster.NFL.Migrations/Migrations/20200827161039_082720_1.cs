using Microsoft.EntityFrameworkCore.Migrations;

namespace RotoMonster.NFL.Migrations.Migrations
{
    public partial class _082720_1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_OwnershipPlayers",
                table: "OwnershipPlayers");

            migrationBuilder.AddColumn<string>(
                name: "CategoriesCode",
                table: "UserLeagues",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CategoriesCode",
                table: "OwnershipPlayers",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "IRCount",
                table: "OwnershipPlayers",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TeamId",
                table: "MLBPitcherGames",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TeamId",
                table: "MLBHitterGames",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "CategoriesCode",
                table: "Drafts",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_OwnershipPlayers",
                table: "OwnershipPlayers",
                columns: new[] { "GameDate", "PlayerId", "CategoriesCode" });

            migrationBuilder.CreateIndex(
                name: "IX_MLBPitcherGames_TeamId",
                table: "MLBPitcherGames",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_MLBHitterGames_TeamId",
                table: "MLBHitterGames",
                column: "TeamId");

            migrationBuilder.AddForeignKey(
                name: "FK_MLBHitterGames_Teams_TeamId",
                table: "MLBHitterGames",
                column: "TeamId",
                principalTable: "Teams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MLBPitcherGames_Teams_TeamId",
                table: "MLBPitcherGames",
                column: "TeamId",
                principalTable: "Teams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MLBHitterGames_Teams_TeamId",
                table: "MLBHitterGames");

            migrationBuilder.DropForeignKey(
                name: "FK_MLBPitcherGames_Teams_TeamId",
                table: "MLBPitcherGames");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OwnershipPlayers",
                table: "OwnershipPlayers");

            migrationBuilder.DropIndex(
                name: "IX_MLBPitcherGames_TeamId",
                table: "MLBPitcherGames");

            migrationBuilder.DropIndex(
                name: "IX_MLBHitterGames_TeamId",
                table: "MLBHitterGames");

            migrationBuilder.DropColumn(
                name: "CategoriesCode",
                table: "UserLeagues");

            migrationBuilder.DropColumn(
                name: "CategoriesCode",
                table: "OwnershipPlayers");

            migrationBuilder.DropColumn(
                name: "IRCount",
                table: "OwnershipPlayers");

            migrationBuilder.DropColumn(
                name: "TeamId",
                table: "MLBPitcherGames");

            migrationBuilder.DropColumn(
                name: "TeamId",
                table: "MLBHitterGames");

            migrationBuilder.DropColumn(
                name: "CategoriesCode",
                table: "Drafts");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OwnershipPlayers",
                table: "OwnershipPlayers",
                columns: new[] { "GameDate", "PlayerId" });
        }
    }
}
