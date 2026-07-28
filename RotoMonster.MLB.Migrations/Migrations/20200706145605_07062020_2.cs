using Microsoft.EntityFrameworkCore.Migrations;

namespace RotoMonster.MLB.Migrations.Migrations
{
    public partial class _07062020_2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Outs",
                table: "MLBPitcherGames");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte>(
                name: "Outs",
                table: "MLBPitcherGames",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);
        }
    }
}
