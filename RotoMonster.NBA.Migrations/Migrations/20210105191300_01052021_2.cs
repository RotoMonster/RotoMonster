using Microsoft.EntityFrameworkCore.Migrations;

namespace RotoMonster.Data.Migrations
{
    public partial class _01052021_2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_OwnershipPlayers",
                table: "OwnershipPlayers");

            migrationBuilder.AlterColumn<string>(
                name: "LineupFrequency",
                table: "OwnershipPlayers",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_OwnershipPlayers",
                table: "OwnershipPlayers",
                columns: new[] { "GameDate", "PlayerId", "CategoriesCode", "LineupFrequency" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_OwnershipPlayers",
                table: "OwnershipPlayers");

            migrationBuilder.AlterColumn<string>(
                name: "LineupFrequency",
                table: "OwnershipPlayers",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string));

            migrationBuilder.AddPrimaryKey(
                name: "PK_OwnershipPlayers",
                table: "OwnershipPlayers",
                columns: new[] { "GameDate", "PlayerId", "CategoriesCode" });
        }
    }
}
