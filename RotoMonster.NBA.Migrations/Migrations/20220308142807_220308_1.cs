using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RotoMonster.Data.Migrations
{
    public partial class _220308_1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "EstimatedReturnDate",
                table: "PlayerStatuses",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "CategoryValue",
                table: "PlayerGamePositionCategories",
                type: "real",
                nullable: false,
                defaultValue: 0f);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EstimatedReturnDate",
                table: "PlayerStatuses");

            migrationBuilder.DropColumn(
                name: "CategoryValue",
                table: "PlayerGamePositionCategories");
        }
    }
}
