using Microsoft.EntityFrameworkCore.Migrations;

namespace RotoMonster.NFL.Migrations.Migrations
{
    public partial class pkg1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "FieldGoalsYards",
                table: "NFLKickerGames",
                nullable: true,
                oldClrType: typeof(byte),
                oldType: "tinyint",
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<byte>(
                name: "FieldGoalsYards",
                table: "NFLKickerGames",
                type: "tinyint",
                nullable: true,
                oldClrType: typeof(int),
                oldNullable: true);
        }
    }
}
