using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RotoMonster.MLB.Migrations.Migrations
{
    public partial class _220609_1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "DailyTransactionsAllowed",
                table: "UserLeagues",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RostersUpdatedDate",
                table: "UserLeagues",
                type: "datetime2",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DailyTransactionsAllowed",
                table: "UserLeagues");

            migrationBuilder.DropColumn(
                name: "RostersUpdatedDate",
                table: "UserLeagues");
        }
    }
}
