using Microsoft.EntityFrameworkCore.Migrations;

namespace RotoMonster.Data.Migrations
{
    public partial class _0616_3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SeasonId",
                table: "UserLeagues",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserLeagues_SeasonId",
                table: "UserLeagues",
                column: "SeasonId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserLeagues_Seasons_SeasonId",
                table: "UserLeagues",
                column: "SeasonId",
                principalTable: "Seasons",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserLeagues_Seasons_SeasonId",
                table: "UserLeagues");

            migrationBuilder.DropIndex(
                name: "IX_UserLeagues_SeasonId",
                table: "UserLeagues");

            migrationBuilder.DropColumn(
                name: "SeasonId",
                table: "UserLeagues");
        }
    }
}
