using Microsoft.EntityFrameworkCore.Migrations;

namespace RotoMonster.Data.Migrations
{
    public partial class pgg3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Games",
                table: "NBAPlayerGames");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte>(
                name: "Games",
                table: "NBAPlayerGames",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);
        }
    }
}
