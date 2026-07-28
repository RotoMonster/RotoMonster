using Microsoft.EntityFrameworkCore.Migrations;

namespace RotoMonster.Migrations
{
    public partial class ua4 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_UserAuths",
                table: "UserAuths");

            migrationBuilder.DropColumn(
                name: "Provider",
                table: "UserAuths");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserAuths",
                table: "UserAuths",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_UserAuths",
                table: "UserAuths");

            migrationBuilder.AddColumn<string>(
                name: "Provider",
                table: "UserAuths",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserAuths",
                table: "UserAuths",
                columns: new[] { "UserId", "Provider" });
        }
    }
}
