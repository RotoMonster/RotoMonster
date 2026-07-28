using Microsoft.EntityFrameworkCore.Migrations;

namespace RotoMonster.MLB.Migrations.Migrations
{
    public partial class _0627_3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "PlayerInjuries");

            migrationBuilder.AddColumn<string>(
                name: "InjuryStatus",
                table: "PlayerInjuries",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PlayerStatus",
                table: "PlayerInjuries",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InjuryStatus",
                table: "PlayerInjuries");

            migrationBuilder.DropColumn(
                name: "PlayerStatus",
                table: "PlayerInjuries");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "PlayerInjuries",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
