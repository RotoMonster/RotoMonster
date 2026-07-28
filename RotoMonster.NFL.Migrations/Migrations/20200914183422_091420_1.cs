using Microsoft.EntityFrameworkCore.Migrations;

namespace RotoMonster.NFL.Migrations.Migrations
{
    public partial class _091420_1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<byte>(
                name: "XpReturned",
                table: "NFLDefenseGames",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<byte>(
                name: "Touchdowns",
                table: "NFLDefenseGames",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<byte>(
                name: "Safeties",
                table: "NFLDefenseGames",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<byte>(
                name: "Sacks",
                table: "NFLDefenseGames",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<short>(
                name: "RushYards",
                table: "NFLDefenseGames",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<byte>(
                name: "RushTouchdowns",
                table: "NFLDefenseGames",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<byte>(
                name: "RushAttempts",
                table: "NFLDefenseGames",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<short>(
                name: "ReceivingAirYards",
                table: "NFLDefenseGames",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<byte>(
                name: "Points7to13",
                table: "NFLDefenseGames",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<byte>(
                name: "Points35",
                table: "NFLDefenseGames",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<byte>(
                name: "Points2to10",
                table: "NFLDefenseGames",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<byte>(
                name: "Points28to34",
                table: "NFLDefenseGames",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<byte>(
                name: "Points21to27",
                table: "NFLDefenseGames",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<byte>(
                name: "Points1to6",
                table: "NFLDefenseGames",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<byte>(
                name: "Points14to20",
                table: "NFLDefenseGames",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<byte>(
                name: "Points11to20",
                table: "NFLDefenseGames",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<byte>(
                name: "Points0",
                table: "NFLDefenseGames",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<byte>(
                name: "Points",
                table: "NFLDefenseGames",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<short>(
                name: "PassYards",
                table: "NFLDefenseGames",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<byte>(
                name: "PassTouchdowns",
                table: "NFLDefenseGames",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<byte>(
                name: "PassSacks",
                table: "NFLDefenseGames",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<byte>(
                name: "PassCompletion",
                table: "NFLDefenseGames",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<byte>(
                name: "PassAttempts",
                table: "NFLDefenseGames",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<byte>(
                name: "Interceptions",
                table: "NFLDefenseGames",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<byte>(
                name: "FumbleRecoveries",
                table: "NFLDefenseGames",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<byte>(
                name: "BlockedKicks",
                table: "NFLDefenseGames",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "XpReturned",
                table: "NFLDefenseGames",
                type: "int",
                nullable: false,
                oldClrType: typeof(byte));

            migrationBuilder.AlterColumn<int>(
                name: "Touchdowns",
                table: "NFLDefenseGames",
                type: "int",
                nullable: false,
                oldClrType: typeof(byte));

            migrationBuilder.AlterColumn<int>(
                name: "Safeties",
                table: "NFLDefenseGames",
                type: "int",
                nullable: false,
                oldClrType: typeof(byte));

            migrationBuilder.AlterColumn<int>(
                name: "Sacks",
                table: "NFLDefenseGames",
                type: "int",
                nullable: false,
                oldClrType: typeof(byte));

            migrationBuilder.AlterColumn<int>(
                name: "RushYards",
                table: "NFLDefenseGames",
                type: "int",
                nullable: false,
                oldClrType: typeof(short));

            migrationBuilder.AlterColumn<int>(
                name: "RushTouchdowns",
                table: "NFLDefenseGames",
                type: "int",
                nullable: false,
                oldClrType: typeof(byte));

            migrationBuilder.AlterColumn<int>(
                name: "RushAttempts",
                table: "NFLDefenseGames",
                type: "int",
                nullable: false,
                oldClrType: typeof(byte));

            migrationBuilder.AlterColumn<int>(
                name: "ReceivingAirYards",
                table: "NFLDefenseGames",
                type: "int",
                nullable: false,
                oldClrType: typeof(short));

            migrationBuilder.AlterColumn<int>(
                name: "Points7to13",
                table: "NFLDefenseGames",
                type: "int",
                nullable: false,
                oldClrType: typeof(byte));

            migrationBuilder.AlterColumn<int>(
                name: "Points35",
                table: "NFLDefenseGames",
                type: "int",
                nullable: false,
                oldClrType: typeof(byte));

            migrationBuilder.AlterColumn<int>(
                name: "Points2to10",
                table: "NFLDefenseGames",
                type: "int",
                nullable: false,
                oldClrType: typeof(byte));

            migrationBuilder.AlterColumn<int>(
                name: "Points28to34",
                table: "NFLDefenseGames",
                type: "int",
                nullable: false,
                oldClrType: typeof(byte));

            migrationBuilder.AlterColumn<int>(
                name: "Points21to27",
                table: "NFLDefenseGames",
                type: "int",
                nullable: false,
                oldClrType: typeof(byte));

            migrationBuilder.AlterColumn<int>(
                name: "Points1to6",
                table: "NFLDefenseGames",
                type: "int",
                nullable: false,
                oldClrType: typeof(byte));

            migrationBuilder.AlterColumn<int>(
                name: "Points14to20",
                table: "NFLDefenseGames",
                type: "int",
                nullable: false,
                oldClrType: typeof(byte));

            migrationBuilder.AlterColumn<int>(
                name: "Points11to20",
                table: "NFLDefenseGames",
                type: "int",
                nullable: false,
                oldClrType: typeof(byte));

            migrationBuilder.AlterColumn<int>(
                name: "Points0",
                table: "NFLDefenseGames",
                type: "int",
                nullable: false,
                oldClrType: typeof(byte));

            migrationBuilder.AlterColumn<int>(
                name: "Points",
                table: "NFLDefenseGames",
                type: "int",
                nullable: false,
                oldClrType: typeof(byte));

            migrationBuilder.AlterColumn<int>(
                name: "PassYards",
                table: "NFLDefenseGames",
                type: "int",
                nullable: false,
                oldClrType: typeof(short));

            migrationBuilder.AlterColumn<int>(
                name: "PassTouchdowns",
                table: "NFLDefenseGames",
                type: "int",
                nullable: false,
                oldClrType: typeof(byte));

            migrationBuilder.AlterColumn<int>(
                name: "PassSacks",
                table: "NFLDefenseGames",
                type: "int",
                nullable: false,
                oldClrType: typeof(byte));

            migrationBuilder.AlterColumn<int>(
                name: "PassCompletion",
                table: "NFLDefenseGames",
                type: "int",
                nullable: false,
                oldClrType: typeof(byte));

            migrationBuilder.AlterColumn<int>(
                name: "PassAttempts",
                table: "NFLDefenseGames",
                type: "int",
                nullable: false,
                oldClrType: typeof(byte));

            migrationBuilder.AlterColumn<int>(
                name: "Interceptions",
                table: "NFLDefenseGames",
                type: "int",
                nullable: false,
                oldClrType: typeof(byte));

            migrationBuilder.AlterColumn<int>(
                name: "FumbleRecoveries",
                table: "NFLDefenseGames",
                type: "int",
                nullable: false,
                oldClrType: typeof(byte));

            migrationBuilder.AlterColumn<int>(
                name: "BlockedKicks",
                table: "NFLDefenseGames",
                type: "int",
                nullable: false,
                oldClrType: typeof(byte));
        }
    }
}
