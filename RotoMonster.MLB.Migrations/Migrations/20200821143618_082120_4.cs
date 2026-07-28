using Microsoft.EntityFrameworkCore.Migrations;

namespace RotoMonster.MLB.Migrations.Migrations
{
    public partial class _082120_4 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FantasyProviderPlayers_Players_PlayerId",
                table: "FantasyProviderPlayers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FantasyProviderPlayers",
                table: "FantasyProviderPlayers");

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

            migrationBuilder.AlterColumn<int>(
                name: "PlayerId",
                table: "FantasyProviderPlayers",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ProviderId",
                table: "FantasyProviderPlayers",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddPrimaryKey(
                name: "PK_FantasyProviderPlayers",
                table: "FantasyProviderPlayers",
                columns: new[] { "FantasyProviderId", "PlayerId" });

            migrationBuilder.CreateIndex(
                name: "IX_MLBPitcherGames_TeamId",
                table: "MLBPitcherGames",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_MLBHitterGames_TeamId",
                table: "MLBHitterGames",
                column: "TeamId");

            migrationBuilder.AddForeignKey(
                name: "FK_FantasyProviderPlayers_Players_PlayerId",
                table: "FantasyProviderPlayers",
                column: "PlayerId",
                principalTable: "Players",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

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
                name: "FK_FantasyProviderPlayers_Players_PlayerId",
                table: "FantasyProviderPlayers");

            migrationBuilder.DropForeignKey(
                name: "FK_MLBHitterGames_Teams_TeamId",
                table: "MLBHitterGames");

            migrationBuilder.DropForeignKey(
                name: "FK_MLBPitcherGames_Teams_TeamId",
                table: "MLBPitcherGames");

            migrationBuilder.DropIndex(
                name: "IX_MLBPitcherGames_TeamId",
                table: "MLBPitcherGames");

            migrationBuilder.DropIndex(
                name: "IX_MLBHitterGames_TeamId",
                table: "MLBHitterGames");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FantasyProviderPlayers",
                table: "FantasyProviderPlayers");

            migrationBuilder.DropColumn(
                name: "TeamId",
                table: "MLBPitcherGames");

            migrationBuilder.DropColumn(
                name: "TeamId",
                table: "MLBHitterGames");

            migrationBuilder.AlterColumn<string>(
                name: "ProviderId",
                table: "FantasyProviderPlayers",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "PlayerId",
                table: "FantasyProviderPlayers",
                type: "int",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AddPrimaryKey(
                name: "PK_FantasyProviderPlayers",
                table: "FantasyProviderPlayers",
                columns: new[] { "FantasyProviderId", "ProviderId" });

            migrationBuilder.AddForeignKey(
                name: "FK_FantasyProviderPlayers_Players_PlayerId",
                table: "FantasyProviderPlayers",
                column: "PlayerId",
                principalTable: "Players",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
