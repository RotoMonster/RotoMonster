using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RotoMonster.NFL.Migrations.Migrations
{
    public partial class _220614_1 : Migration
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

            migrationBuilder.AddColumn<DateTime>(
                name: "RostersUpdatedDate",
                table: "UserLeagues",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LineupCount",
                table: "Positions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "EstimatedReturnDate",
                table: "PlayerStatuses",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EstimatedReturnDate",
                table: "PlayerInjuries",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "CategoryValue",
                table: "PlayerGamePositionCategories",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<bool>(
                name: "PerValuesSameAsTotal",
                table: "Categories",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddPrimaryKey(
                name: "PK_OwnershipPlayers",
                table: "OwnershipPlayers",
                columns: new[] { "GameDate", "PlayerId", "CategoriesStringId" });

            migrationBuilder.CreateTable(
                name: "Helpers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OnPageDetails = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDisabled = table.Column<bool>(type: "bit", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Helpers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserOptionTypes",
                columns: table => new
                {
                    Id = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Abbreviation = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    OptionGroup = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    DisplayOrder = table.Column<short>(type: "smallint", nullable: false),
                    DataType = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: true),
                    DefaultValueBool = table.Column<bool>(type: "bit", nullable: true),
                    DefaultValueByte = table.Column<byte>(type: "tinyint", nullable: true),
                    DefaultValueShort = table.Column<short>(type: "smallint", nullable: true),
                    DefaultValueInt = table.Column<int>(type: "int", nullable: true),
                    DefaultValueDouble = table.Column<double>(type: "float", nullable: true),
                    DefaultValueString = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserOptionTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserOptions",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    UserOptionTypeId = table.Column<short>(type: "smallint", nullable: false),
                    ValueBool = table.Column<bool>(type: "bit", nullable: true),
                    ValueByte = table.Column<byte>(type: "tinyint", nullable: true),
                    ValueShort = table.Column<short>(type: "smallint", nullable: true),
                    ValueInt = table.Column<int>(type: "int", nullable: true),
                    ValueDouble = table.Column<double>(type: "float", nullable: true),
                    ValueString = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserOptions", x => new { x.UserId, x.UserOptionTypeId });
                    table.ForeignKey(
                        name: "FK_UserOptions_UserOptionTypes_UserOptionTypeId",
                        column: x => x.UserOptionTypeId,
                        principalTable: "UserOptionTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserOptions_UserOptionTypeId",
                table: "UserOptions",
                column: "UserOptionTypeId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Helpers");

            migrationBuilder.DropTable(
                name: "UserOptions");

            migrationBuilder.DropTable(
                name: "UserOptionTypes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OwnershipPlayers",
                table: "OwnershipPlayers");

            migrationBuilder.DropColumn(
                name: "DailyTransactionsAllowed",
                table: "UserLeagues");

            migrationBuilder.DropColumn(
                name: "RostersUpdatedDate",
                table: "UserLeagues");

            migrationBuilder.DropColumn(
                name: "LineupCount",
                table: "Positions");

            migrationBuilder.DropColumn(
                name: "EstimatedReturnDate",
                table: "PlayerStatuses");

            migrationBuilder.DropColumn(
                name: "EstimatedReturnDate",
                table: "PlayerInjuries");

            migrationBuilder.DropColumn(
                name: "CategoryValue",
                table: "PlayerGamePositionCategories");

            migrationBuilder.DropColumn(
                name: "PerValuesSameAsTotal",
                table: "Categories");

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
