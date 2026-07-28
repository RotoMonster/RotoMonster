using Microsoft.EntityFrameworkCore.Migrations;

namespace RotoMonster.MLB.Migrations.Migrations
{
    public partial class _071220_1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FantasyProviderPlayers_FantasyProviders_FantasyProviderId",
                table: "FantasyProviderPlayers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FantasyProviderPlayers",
                table: "FantasyProviderPlayers");

            migrationBuilder.DropIndex(
                name: "IX_FantasyProviderPlayers_FantasyProviderId",
                table: "FantasyProviderPlayers");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "FantasyProviderPlayers");

            migrationBuilder.AlterColumn<string>(
                name: "ProviderId",
                table: "FantasyProviderPlayers",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "FantasyProviderId",
                table: "FantasyProviderPlayers",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_FantasyProviderPlayers",
                table: "FantasyProviderPlayers",
                columns: new[] { "FantasyProviderId", "ProviderId" });

            migrationBuilder.AddForeignKey(
                name: "FK_FantasyProviderPlayers_FantasyProviders_FantasyProviderId",
                table: "FantasyProviderPlayers",
                column: "FantasyProviderId",
                principalTable: "FantasyProviders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FantasyProviderPlayers_FantasyProviders_FantasyProviderId",
                table: "FantasyProviderPlayers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FantasyProviderPlayers",
                table: "FantasyProviderPlayers");

            migrationBuilder.AlterColumn<string>(
                name: "ProviderId",
                table: "FantasyProviderPlayers",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string));

            migrationBuilder.AlterColumn<int>(
                name: "FantasyProviderId",
                table: "FantasyProviderPlayers",
                type: "int",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "FantasyProviderPlayers",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddPrimaryKey(
                name: "PK_FantasyProviderPlayers",
                table: "FantasyProviderPlayers",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_FantasyProviderPlayers_FantasyProviderId",
                table: "FantasyProviderPlayers",
                column: "FantasyProviderId");

            migrationBuilder.AddForeignKey(
                name: "FK_FantasyProviderPlayers_FantasyProviders_FantasyProviderId",
                table: "FantasyProviderPlayers",
                column: "FantasyProviderId",
                principalTable: "FantasyProviders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
