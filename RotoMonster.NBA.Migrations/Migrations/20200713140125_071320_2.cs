using Microsoft.EntityFrameworkCore.Migrations;

namespace RotoMonster.Data.Migrations
{
    public partial class _071320_2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FantasyProviderPlayers_Players_PlayerId",
                table: "FantasyProviderPlayers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FantasyProviderPlayers",
                table: "FantasyProviderPlayers");

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

            migrationBuilder.AddForeignKey(
                name: "FK_FantasyProviderPlayers_Players_PlayerId",
                table: "FantasyProviderPlayers",
                column: "PlayerId",
                principalTable: "Players",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FantasyProviderPlayers_Players_PlayerId",
                table: "FantasyProviderPlayers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FantasyProviderPlayers",
                table: "FantasyProviderPlayers");

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
