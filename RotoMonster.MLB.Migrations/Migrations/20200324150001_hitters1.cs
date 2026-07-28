using Microsoft.EntityFrameworkCore.Migrations;

namespace RotoMonster.MLB.Migrations.Migrations
{
    public partial class hitters1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FullInnings",
                table: "MLBPitcherGames");

            migrationBuilder.DropColumn(
                name: "ThirdInnings",
                table: "MLBPitcherGames");

            migrationBuilder.DropColumn(
                name: "Hits",
                table: "MLBHitterGames");

            migrationBuilder.DropColumn(
                name: "SBCaught",
                table: "MLBHitterGames");

            migrationBuilder.AlterColumn<double>(
                name: "Innings",
                table: "MLBPitcherGames",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float",
                oldNullable: true);

            migrationBuilder.AddColumn<byte>(
                name: "CS",
                table: "MLBHitterGames",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<byte>(
                name: "H",
                table: "MLBHitterGames",
                nullable: false,
                defaultValue: (byte)0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CS",
                table: "MLBHitterGames");

            migrationBuilder.DropColumn(
                name: "H",
                table: "MLBHitterGames");

            migrationBuilder.AlterColumn<double>(
                name: "Innings",
                table: "MLBPitcherGames",
                type: "float",
                nullable: true,
                oldClrType: typeof(double));

            migrationBuilder.AddColumn<byte>(
                name: "FullInnings",
                table: "MLBPitcherGames",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<byte>(
                name: "ThirdInnings",
                table: "MLBPitcherGames",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<byte>(
                name: "Hits",
                table: "MLBHitterGames",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<byte>(
                name: "SBCaught",
                table: "MLBHitterGames",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);
        }
    }
}
