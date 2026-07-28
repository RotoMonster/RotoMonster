using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RotoMonster.Data.Migrations
{
    public partial class _220125_5 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "DefaultValueBool",
                table: "UserOptionTypes",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<byte>(
                name: "DefaultValueByte",
                table: "UserOptionTypes",
                type: "tinyint",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "DefaultValueDouble",
                table: "UserOptionTypes",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DefaultValueInt",
                table: "UserOptionTypes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<short>(
                name: "DefaultValueShort",
                table: "UserOptionTypes",
                type: "smallint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DefaultValueString",
                table: "UserOptionTypes",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DefaultValueBool",
                table: "UserOptionTypes");

            migrationBuilder.DropColumn(
                name: "DefaultValueByte",
                table: "UserOptionTypes");

            migrationBuilder.DropColumn(
                name: "DefaultValueDouble",
                table: "UserOptionTypes");

            migrationBuilder.DropColumn(
                name: "DefaultValueInt",
                table: "UserOptionTypes");

            migrationBuilder.DropColumn(
                name: "DefaultValueShort",
                table: "UserOptionTypes");

            migrationBuilder.DropColumn(
                name: "DefaultValueString",
                table: "UserOptionTypes");
        }
    }
}
