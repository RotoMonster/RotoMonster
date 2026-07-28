using Microsoft.EntityFrameworkCore.Migrations;

namespace RotoMonster.MLB.Migrations.Migrations
{
    public partial class _082820_1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
                name: "IsFinished",
                table: "Games",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
                name: "IsFinished",
                table: "Games",
                type: "bit",
                nullable: true,
                oldClrType: typeof(bool));
        }
    }
}
