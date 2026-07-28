using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RotoMonster.Data.Migrations
{
    public partial class _220722_1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_OwnershipPlayers",
                table: "OwnershipPlayers");

            migrationBuilder.DropColumn(
                name: "LineupFrequency",
                table: "OwnershipPlayers");

            migrationBuilder.AddColumn<bool>(
                name: "DailyTransactionsAllowed",
                table: "UserLeagues",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDefault",
                table: "UserLeagues",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "RostersUpdatedDate",
                table: "UserLeagues",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_OwnershipPlayers",
                table: "OwnershipPlayers",
                columns: new[] { "GameDate", "PlayerId", "CategoriesStringId" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_OwnershipPlayers",
                table: "OwnershipPlayers");

            migrationBuilder.DropColumn(
                name: "DailyTransactionsAllowed",
                table: "UserLeagues");

            migrationBuilder.DropColumn(
                name: "IsDefault",
                table: "UserLeagues");

            migrationBuilder.DropColumn(
                name: "RostersUpdatedDate",
                table: "UserLeagues");

            migrationBuilder.AddColumn<string>(
                name: "LineupFrequency",
                table: "OwnershipPlayers",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OwnershipPlayers",
                table: "OwnershipPlayers",
                columns: new[] { "GameDate", "PlayerId", "CategoriesStringId", "LineupFrequency" });
        }
    }
}
