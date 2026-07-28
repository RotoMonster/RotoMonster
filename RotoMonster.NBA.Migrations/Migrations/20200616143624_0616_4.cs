using Microsoft.EntityFrameworkCore.Migrations;

namespace RotoMonster.Data.Migrations
{
    public partial class _0616_4 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserLeagues_Seasons_SeasonId",
                table: "UserLeagues");

            migrationBuilder.AlterColumn<int>(
                name: "SeasonId",
                table: "UserLeagues",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SeasonId",
                table: "Drafts",
                nullable: false,
                defaultValue: 0);

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
                name: "IX_Drafts_SeasonId",
                table: "Drafts");

            migrationBuilder.DropColumn(
                name: "SeasonId",
                table: "Drafts");

            migrationBuilder.AlterColumn<int>(
                name: "SeasonId",
                table: "UserLeagues",
                type: "int",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AddForeignKey(
                name: "FK_UserLeagues_Seasons_SeasonId",
                table: "UserLeagues",
                column: "SeasonId",
                principalTable: "Seasons",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
