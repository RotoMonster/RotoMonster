using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace RotoMonster.NFL.Migrations.Migrations
{
    public partial class _072120_1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FantasyProviderPlayers_FantasyProviders_FantasyProviderId",
                table: "FantasyProviderPlayers");

            migrationBuilder.DropForeignKey(
                name: "FK_FantasyProviderPlayers_Players_PlayerId",
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

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "UserLeagues",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "PlayerId",
                table: "FantasyProviderPlayers",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
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
                columns: new[] { "FantasyProviderId", "PlayerId" });

            migrationBuilder.AddForeignKey(
                name: "FK_FantasyProviderPlayers_FantasyProviders_FantasyProviderId",
                table: "FantasyProviderPlayers",
                column: "FantasyProviderId",
                principalTable: "FantasyProviders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

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
                name: "FK_FantasyProviderPlayers_FantasyProviders_FantasyProviderId",
                table: "FantasyProviderPlayers");

            migrationBuilder.DropForeignKey(
                name: "FK_FantasyProviderPlayers_Players_PlayerId",
                table: "FantasyProviderPlayers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FantasyProviderPlayers",
                table: "FantasyProviderPlayers");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "UserLeagues");

            migrationBuilder.AlterColumn<int>(
                name: "PlayerId",
                table: "FantasyProviderPlayers",
                type: "int",
                nullable: true,
                oldClrType: typeof(int));

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
