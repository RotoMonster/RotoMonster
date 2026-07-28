using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace RotoMonster.MLB.Migrations.Migrations
{
    public partial class _071120_1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "UserLeagues",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "UserLeagues");
        }
    }
}
