using Microsoft.EntityFrameworkCore.Migrations;

namespace RotoMonster.Data.Migrations
{
    public partial class psp203 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_PositionSources_FantasyProviderId",
                table: "PositionSources",
                column: "FantasyProviderId");

            migrationBuilder.AddForeignKey(
                name: "FK_PositionSources_FantasyProviders_FantasyProviderId",
                table: "PositionSources",
                column: "FantasyProviderId",
                principalTable: "FantasyProviders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PositionSources_FantasyProviders_FantasyProviderId",
                table: "PositionSources");

            migrationBuilder.DropIndex(
                name: "IX_PositionSources_FantasyProviderId",
                table: "PositionSources");
        }
    }
}
