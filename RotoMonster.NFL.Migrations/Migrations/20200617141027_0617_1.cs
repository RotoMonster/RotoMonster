using Microsoft.EntityFrameworkCore.Migrations;

namespace RotoMonster.NFL.Migrations.Migrations
{
    public partial class _0617_1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsAuction",
                table: "UserLeagues",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "SeasonId",
                table: "UserLeagues",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsProLeague",
                table: "Drafts",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "SeasonId",
                table: "Drafts",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_UserLeagues_SeasonId",
                table: "UserLeagues",
                column: "SeasonId");

            migrationBuilder.CreateIndex(
                name: "IX_Drafts_SeasonId",
                table: "Drafts",
                column: "SeasonId");

            migrationBuilder.AddForeignKey(
                name: "FK_Drafts_Seasons_SeasonId",
                table: "Drafts",
                column: "SeasonId",
                principalTable: "Seasons",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserLeagues_Seasons_SeasonId",
                table: "UserLeagues",
                column: "SeasonId",
                principalTable: "Seasons",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Drafts_Seasons_SeasonId",
                table: "Drafts");

            migrationBuilder.DropForeignKey(
                name: "FK_UserLeagues_Seasons_SeasonId",
                table: "UserLeagues");

            migrationBuilder.DropIndex(
                name: "IX_UserLeagues_SeasonId",
                table: "UserLeagues");

            migrationBuilder.DropIndex(
                name: "IX_Drafts_SeasonId",
                table: "Drafts");

            migrationBuilder.DropColumn(
                name: "IsAuction",
                table: "UserLeagues");

            migrationBuilder.DropColumn(
                name: "SeasonId",
                table: "UserLeagues");

            migrationBuilder.DropColumn(
                name: "IsProLeague",
                table: "Drafts");

            migrationBuilder.DropColumn(
                name: "SeasonId",
                table: "Drafts");
        }
    }
}
