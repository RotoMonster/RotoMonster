using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace RotoMonster.MLB.Migrations.Migrations
{
    public partial class _0627_2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProviderFileId",
                table: "PlayerInjuries");

            migrationBuilder.AddColumn<DateTime>(
                name: "DownloadDate",
                table: "PlayerInjuries",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DownloadDate",
                table: "PlayerInjuries");

            migrationBuilder.AddColumn<string>(
                name: "ProviderFileId",
                table: "PlayerInjuries",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
