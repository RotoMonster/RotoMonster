using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RotoMonster.Data.Migrations
{
    public partial class _220125_2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "UserOptionTypes",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsEnabled",
                table: "UserOptionTypes",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "UserOptionTypes");

            migrationBuilder.DropColumn(
                name: "IsEnabled",
                table: "UserOptionTypes");
        }
    }
}
