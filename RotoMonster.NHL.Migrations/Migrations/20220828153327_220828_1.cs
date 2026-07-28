using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RotoMonster.NHL.Migrations.Migrations
{
    public partial class _220828_1 : Migration
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

            migrationBuilder.AddColumn<string>(
                name: "ESPNCode",
                table: "Sports",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ESPNYear",
                table: "Seasons",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<byte>(
                name: "FieldGoals0to39",
                table: "NFLKickerGames",
                type: "tinyint",
                nullable: true);

            migrationBuilder.AddColumn<byte>(
                name: "Points14to17",
                table: "NFLDefenseGames",
                type: "tinyint",
                nullable: true);

            migrationBuilder.AddColumn<byte>(
                name: "Points35to45",
                table: "NFLDefenseGames",
                type: "tinyint",
                nullable: true);

            migrationBuilder.AddColumn<byte>(
                name: "Points46",
                table: "NFLDefenseGames",
                type: "tinyint",
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

            migrationBuilder.DropColumn(
                name: "ESPNCode",
                table: "Sports");

            migrationBuilder.DropColumn(
                name: "ESPNYear",
                table: "Seasons");

            migrationBuilder.DropColumn(
                name: "FieldGoals0to39",
                table: "NFLKickerGames");

            migrationBuilder.DropColumn(
                name: "Points14to17",
                table: "NFLDefenseGames");

            migrationBuilder.DropColumn(
                name: "Points35to45",
                table: "NFLDefenseGames");

            migrationBuilder.DropColumn(
                name: "Points46",
                table: "NFLDefenseGames");

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
