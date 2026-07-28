using Microsoft.EntityFrameworkCore.Migrations;

namespace RotoMonster.NFL.Migrations.Migrations
{
    public partial class nflo4 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<byte>(
                name: "RushYardsLost",
                table: "NFLOffensiveGames",
                nullable: true,
                oldClrType: typeof(byte),
                oldType: "tinyint");

            migrationBuilder.AlterColumn<byte>(
                name: "RushYardsAfterContact",
                table: "NFLOffensiveGames",
                nullable: true,
                oldClrType: typeof(byte),
                oldType: "tinyint");

            migrationBuilder.AlterColumn<byte>(
                name: "RushYards",
                table: "NFLOffensiveGames",
                nullable: true,
                oldClrType: typeof(byte),
                oldType: "tinyint");

            migrationBuilder.AlterColumn<byte>(
                name: "RushTD",
                table: "NFLOffensiveGames",
                nullable: true,
                oldClrType: typeof(byte),
                oldType: "tinyint");

            migrationBuilder.AlterColumn<byte>(
                name: "RushRedzoneAttempted",
                table: "NFLOffensiveGames",
                nullable: true,
                oldClrType: typeof(byte),
                oldType: "tinyint");

            migrationBuilder.AlterColumn<byte>(
                name: "RushLost",
                table: "NFLOffensiveGames",
                nullable: true,
                oldClrType: typeof(byte),
                oldType: "tinyint");

            migrationBuilder.AlterColumn<byte>(
                name: "RushKneelDowns",
                table: "NFLOffensiveGames",
                nullable: true,
                oldClrType: typeof(byte),
                oldType: "tinyint");

            migrationBuilder.AlterColumn<byte>(
                name: "RushFumbles",
                table: "NFLOffensiveGames",
                nullable: true,
                oldClrType: typeof(byte),
                oldType: "tinyint");

            migrationBuilder.AlterColumn<byte>(
                name: "RushBrokenTackles",
                table: "NFLOffensiveGames",
                nullable: true,
                oldClrType: typeof(byte),
                oldType: "tinyint");

            migrationBuilder.AlterColumn<byte>(
                name: "RushAttempts",
                table: "NFLOffensiveGames",
                nullable: true,
                oldClrType: typeof(byte),
                oldType: "tinyint");

            migrationBuilder.AlterColumn<byte>(
                name: "ReturnYards",
                table: "NFLOffensiveGames",
                nullable: true,
                oldClrType: typeof(byte),
                oldType: "tinyint");

            migrationBuilder.AlterColumn<byte>(
                name: "ReturnTD",
                table: "NFLOffensiveGames",
                nullable: true,
                oldClrType: typeof(byte),
                oldType: "tinyint");

            migrationBuilder.AlterColumn<byte>(
                name: "ReturnReturns",
                table: "NFLOffensiveGames",
                nullable: true,
                oldClrType: typeof(byte),
                oldType: "tinyint");

            migrationBuilder.AlterColumn<byte>(
                name: "ReturnFaircatches",
                table: "NFLOffensiveGames",
                nullable: true,
                oldClrType: typeof(byte),
                oldType: "tinyint");

            migrationBuilder.AlterColumn<byte>(
                name: "RecYardsAfterCatch",
                table: "NFLOffensiveGames",
                nullable: true,
                oldClrType: typeof(byte),
                oldType: "tinyint");

            migrationBuilder.AlterColumn<byte>(
                name: "RecYardsAafterContact",
                table: "NFLOffensiveGames",
                nullable: true,
                oldClrType: typeof(byte),
                oldType: "tinyint");

            migrationBuilder.AlterColumn<int>(
                name: "RecYards",
                table: "NFLOffensiveGames",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<byte>(
                name: "RecTargets",
                table: "NFLOffensiveGames",
                nullable: true,
                oldClrType: typeof(byte),
                oldType: "tinyint");

            migrationBuilder.AlterColumn<byte>(
                name: "RecTD",
                table: "NFLOffensiveGames",
                nullable: true,
                oldClrType: typeof(byte),
                oldType: "tinyint");

            migrationBuilder.AlterColumn<byte>(
                name: "RecRedzoneTargets",
                table: "NFLOffensiveGames",
                nullable: true,
                oldClrType: typeof(byte),
                oldType: "tinyint");

            migrationBuilder.AlterColumn<byte>(
                name: "RecReceptions",
                table: "NFLOffensiveGames",
                nullable: true,
                oldClrType: typeof(byte),
                oldType: "tinyint");

            migrationBuilder.AlterColumn<byte>(
                name: "RecDroppedPasses",
                table: "NFLOffensiveGames",
                nullable: true,
                oldClrType: typeof(byte),
                oldType: "tinyint");

            migrationBuilder.AlterColumn<byte>(
                name: "RecCatchablePasses",
                table: "NFLOffensiveGames",
                nullable: true,
                oldClrType: typeof(byte),
                oldType: "tinyint");

            migrationBuilder.AlterColumn<byte>(
                name: "RecBrokenTackles",
                table: "NFLOffensiveGames",
                nullable: true,
                oldClrType: typeof(byte),
                oldType: "tinyint");

            migrationBuilder.AlterColumn<byte>(
                name: "RecAirYards",
                table: "NFLOffensiveGames",
                nullable: true,
                oldClrType: typeof(byte),
                oldType: "tinyint");

            migrationBuilder.AlterColumn<byte>(
                name: "RassRedzoneAttempts",
                table: "NFLOffensiveGames",
                nullable: true,
                oldClrType: typeof(byte),
                oldType: "tinyint");

            migrationBuilder.AlterColumn<byte>(
                name: "PassYards",
                table: "NFLOffensiveGames",
                nullable: true,
                oldClrType: typeof(byte),
                oldType: "tinyint");

            migrationBuilder.AlterColumn<byte>(
                name: "PassThrowAways",
                table: "NFLOffensiveGames",
                nullable: true,
                oldClrType: typeof(byte),
                oldType: "tinyint");

            migrationBuilder.AlterColumn<byte>(
                name: "PassTD",
                table: "NFLOffensiveGames",
                nullable: true,
                oldClrType: typeof(byte),
                oldType: "tinyint");

            migrationBuilder.AlterColumn<byte>(
                name: "PassSpikes",
                table: "NFLOffensiveGames",
                nullable: true,
                oldClrType: typeof(byte),
                oldType: "tinyint");

            migrationBuilder.AlterColumn<byte>(
                name: "PassSacks",
                table: "NFLOffensiveGames",
                nullable: true,
                oldClrType: typeof(byte),
                oldType: "tinyint");

            migrationBuilder.AlterColumn<byte>(
                name: "PassSackYards",
                table: "NFLOffensiveGames",
                nullable: true,
                oldClrType: typeof(byte),
                oldType: "tinyint");

            migrationBuilder.AlterColumn<double>(
                name: "PassRating",
                table: "NFLOffensiveGames",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AlterColumn<byte>(
                name: "PassPoorThrows",
                table: "NFLOffensiveGames",
                nullable: true,
                oldClrType: typeof(byte),
                oldType: "tinyint");

            migrationBuilder.AlterColumn<byte>(
                name: "PassPocketTime",
                table: "NFLOffensiveGames",
                nullable: true,
                oldClrType: typeof(byte),
                oldType: "tinyint");

            migrationBuilder.AlterColumn<byte>(
                name: "PassKnockdowns",
                table: "NFLOffensiveGames",
                nullable: true,
                oldClrType: typeof(byte),
                oldType: "tinyint");

            migrationBuilder.AlterColumn<byte>(
                name: "PassInt",
                table: "NFLOffensiveGames",
                nullable: true,
                oldClrType: typeof(byte),
                oldType: "tinyint");

            migrationBuilder.AlterColumn<byte>(
                name: "PassHurries",
                table: "NFLOffensiveGames",
                nullable: true,
                oldClrType: typeof(byte),
                oldType: "tinyint");

            migrationBuilder.AlterColumn<byte>(
                name: "PassDroppedPasses",
                table: "NFLOffensiveGames",
                nullable: true,
                oldClrType: typeof(byte),
                oldType: "tinyint");

            migrationBuilder.AlterColumn<byte>(
                name: "PassDefendedPasses",
                table: "NFLOffensiveGames",
                nullable: true,
                oldClrType: typeof(byte),
                oldType: "tinyint");

            migrationBuilder.AlterColumn<byte>(
                name: "PassCompletions",
                table: "NFLOffensiveGames",
                nullable: true,
                oldClrType: typeof(byte),
                oldType: "tinyint");

            migrationBuilder.AlterColumn<byte>(
                name: "PassBlitzes",
                table: "NFLOffensiveGames",
                nullable: true,
                oldClrType: typeof(byte),
                oldType: "tinyint");

            migrationBuilder.AlterColumn<byte>(
                name: "PassAttempts",
                table: "NFLOffensiveGames",
                nullable: true,
                oldClrType: typeof(byte),
                oldType: "tinyint");

            migrationBuilder.AlterColumn<byte>(
                name: "PassAirYards",
                table: "NFLOffensiveGames",
                nullable: true,
                oldClrType: typeof(byte),
                oldType: "tinyint");

            migrationBuilder.AlterColumn<byte>(
                name: "FumblesLost",
                table: "NFLOffensiveGames",
                nullable: true,
                oldClrType: typeof(byte),
                oldType: "tinyint");

            migrationBuilder.AlterColumn<byte>(
                name: "Fumbles",
                table: "NFLOffensiveGames",
                nullable: true,
                oldClrType: typeof(byte),
                oldType: "tinyint");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<byte>(
                name: "RushYardsLost",
                table: "NFLOffensiveGames",
                type: "tinyint",
                nullable: false,
                oldClrType: typeof(byte),
                oldNullable: true);

            migrationBuilder.AlterColumn<byte>(
                name: "RushYardsAfterContact",
                table: "NFLOffensiveGames",
                type: "tinyint",
                nullable: false,
                oldClrType: typeof(byte),
                oldNullable: true);

            migrationBuilder.AlterColumn<byte>(
                name: "RushYards",
                table: "NFLOffensiveGames",
                type: "tinyint",
                nullable: false,
                oldClrType: typeof(byte),
                oldNullable: true);

            migrationBuilder.AlterColumn<byte>(
                name: "RushTD",
                table: "NFLOffensiveGames",
                type: "tinyint",
                nullable: false,
                oldClrType: typeof(byte),
                oldNullable: true);

            migrationBuilder.AlterColumn<byte>(
                name: "RushRedzoneAttempted",
                table: "NFLOffensiveGames",
                type: "tinyint",
                nullable: false,
                oldClrType: typeof(byte),
                oldNullable: true);

            migrationBuilder.AlterColumn<byte>(
                name: "RushLost",
                table: "NFLOffensiveGames",
                type: "tinyint",
                nullable: false,
                oldClrType: typeof(byte),
                oldNullable: true);

            migrationBuilder.AlterColumn<byte>(
                name: "RushKneelDowns",
                table: "NFLOffensiveGames",
                type: "tinyint",
                nullable: false,
                oldClrType: typeof(byte),
                oldNullable: true);

            migrationBuilder.AlterColumn<byte>(
                name: "RushFumbles",
                table: "NFLOffensiveGames",
                type: "tinyint",
                nullable: false,
                oldClrType: typeof(byte),
                oldNullable: true);

            migrationBuilder.AlterColumn<byte>(
                name: "RushBrokenTackles",
                table: "NFLOffensiveGames",
                type: "tinyint",
                nullable: false,
                oldClrType: typeof(byte),
                oldNullable: true);

            migrationBuilder.AlterColumn<byte>(
                name: "RushAttempts",
                table: "NFLOffensiveGames",
                type: "tinyint",
                nullable: false,
                oldClrType: typeof(byte),
                oldNullable: true);

            migrationBuilder.AlterColumn<byte>(
                name: "ReturnYards",
                table: "NFLOffensiveGames",
                type: "tinyint",
                nullable: false,
                oldClrType: typeof(byte),
                oldNullable: true);

            migrationBuilder.AlterColumn<byte>(
                name: "ReturnTD",
                table: "NFLOffensiveGames",
                type: "tinyint",
                nullable: false,
                oldClrType: typeof(byte),
                oldNullable: true);

            migrationBuilder.AlterColumn<byte>(
                name: "ReturnReturns",
                table: "NFLOffensiveGames",
                type: "tinyint",
                nullable: false,
                oldClrType: typeof(byte),
                oldNullable: true);

            migrationBuilder.AlterColumn<byte>(
                name: "ReturnFaircatches",
                table: "NFLOffensiveGames",
                type: "tinyint",
                nullable: false,
                oldClrType: typeof(byte),
                oldNullable: true);

            migrationBuilder.AlterColumn<byte>(
                name: "RecYardsAfterCatch",
                table: "NFLOffensiveGames",
                type: "tinyint",
                nullable: false,
                oldClrType: typeof(byte),
                oldNullable: true);

            migrationBuilder.AlterColumn<byte>(
                name: "RecYardsAafterContact",
                table: "NFLOffensiveGames",
                type: "tinyint",
                nullable: false,
                oldClrType: typeof(byte),
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "RecYards",
                table: "NFLOffensiveGames",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AlterColumn<byte>(
                name: "RecTargets",
                table: "NFLOffensiveGames",
                type: "tinyint",
                nullable: false,
                oldClrType: typeof(byte),
                oldNullable: true);

            migrationBuilder.AlterColumn<byte>(
                name: "RecTD",
                table: "NFLOffensiveGames",
                type: "tinyint",
                nullable: false,
                oldClrType: typeof(byte),
                oldNullable: true);

            migrationBuilder.AlterColumn<byte>(
                name: "RecRedzoneTargets",
                table: "NFLOffensiveGames",
                type: "tinyint",
                nullable: false,
                oldClrType: typeof(byte),
                oldNullable: true);

            migrationBuilder.AlterColumn<byte>(
                name: "RecReceptions",
                table: "NFLOffensiveGames",
                type: "tinyint",
                nullable: false,
                oldClrType: typeof(byte),
                oldNullable: true);

            migrationBuilder.AlterColumn<byte>(
                name: "RecDroppedPasses",
                table: "NFLOffensiveGames",
                type: "tinyint",
                nullable: false,
                oldClrType: typeof(byte),
                oldNullable: true);

            migrationBuilder.AlterColumn<byte>(
                name: "RecCatchablePasses",
                table: "NFLOffensiveGames",
                type: "tinyint",
                nullable: false,
                oldClrType: typeof(byte),
                oldNullable: true);

            migrationBuilder.AlterColumn<byte>(
                name: "RecBrokenTackles",
                table: "NFLOffensiveGames",
                type: "tinyint",
                nullable: false,
                oldClrType: typeof(byte),
                oldNullable: true);

            migrationBuilder.AlterColumn<byte>(
                name: "RecAirYards",
                table: "NFLOffensiveGames",
                type: "tinyint",
                nullable: false,
                oldClrType: typeof(byte),
                oldNullable: true);

            migrationBuilder.AlterColumn<byte>(
                name: "RassRedzoneAttempts",
                table: "NFLOffensiveGames",
                type: "tinyint",
                nullable: false,
                oldClrType: typeof(byte),
                oldNullable: true);

            migrationBuilder.AlterColumn<byte>(
                name: "PassYards",
                table: "NFLOffensiveGames",
                type: "tinyint",
                nullable: false,
                oldClrType: typeof(byte),
                oldNullable: true);

            migrationBuilder.AlterColumn<byte>(
                name: "PassThrowAways",
                table: "NFLOffensiveGames",
                type: "tinyint",
                nullable: false,
                oldClrType: typeof(byte),
                oldNullable: true);

            migrationBuilder.AlterColumn<byte>(
                name: "PassTD",
                table: "NFLOffensiveGames",
                type: "tinyint",
                nullable: false,
                oldClrType: typeof(byte),
                oldNullable: true);

            migrationBuilder.AlterColumn<byte>(
                name: "PassSpikes",
                table: "NFLOffensiveGames",
                type: "tinyint",
                nullable: false,
                oldClrType: typeof(byte),
                oldNullable: true);

            migrationBuilder.AlterColumn<byte>(
                name: "PassSacks",
                table: "NFLOffensiveGames",
                type: "tinyint",
                nullable: false,
                oldClrType: typeof(byte),
                oldNullable: true);

            migrationBuilder.AlterColumn<byte>(
                name: "PassSackYards",
                table: "NFLOffensiveGames",
                type: "tinyint",
                nullable: false,
                oldClrType: typeof(byte),
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "PassRating",
                table: "NFLOffensiveGames",
                type: "float",
                nullable: false,
                oldClrType: typeof(double),
                oldNullable: true);

            migrationBuilder.AlterColumn<byte>(
                name: "PassPoorThrows",
                table: "NFLOffensiveGames",
                type: "tinyint",
                nullable: false,
                oldClrType: typeof(byte),
                oldNullable: true);

            migrationBuilder.AlterColumn<byte>(
                name: "PassPocketTime",
                table: "NFLOffensiveGames",
                type: "tinyint",
                nullable: false,
                oldClrType: typeof(byte),
                oldNullable: true);

            migrationBuilder.AlterColumn<byte>(
                name: "PassKnockdowns",
                table: "NFLOffensiveGames",
                type: "tinyint",
                nullable: false,
                oldClrType: typeof(byte),
                oldNullable: true);

            migrationBuilder.AlterColumn<byte>(
                name: "PassInt",
                table: "NFLOffensiveGames",
                type: "tinyint",
                nullable: false,
                oldClrType: typeof(byte),
                oldNullable: true);

            migrationBuilder.AlterColumn<byte>(
                name: "PassHurries",
                table: "NFLOffensiveGames",
                type: "tinyint",
                nullable: false,
                oldClrType: typeof(byte),
                oldNullable: true);

            migrationBuilder.AlterColumn<byte>(
                name: "PassDroppedPasses",
                table: "NFLOffensiveGames",
                type: "tinyint",
                nullable: false,
                oldClrType: typeof(byte),
                oldNullable: true);

            migrationBuilder.AlterColumn<byte>(
                name: "PassDefendedPasses",
                table: "NFLOffensiveGames",
                type: "tinyint",
                nullable: false,
                oldClrType: typeof(byte),
                oldNullable: true);

            migrationBuilder.AlterColumn<byte>(
                name: "PassCompletions",
                table: "NFLOffensiveGames",
                type: "tinyint",
                nullable: false,
                oldClrType: typeof(byte),
                oldNullable: true);

            migrationBuilder.AlterColumn<byte>(
                name: "PassBlitzes",
                table: "NFLOffensiveGames",
                type: "tinyint",
                nullable: false,
                oldClrType: typeof(byte),
                oldNullable: true);

            migrationBuilder.AlterColumn<byte>(
                name: "PassAttempts",
                table: "NFLOffensiveGames",
                type: "tinyint",
                nullable: false,
                oldClrType: typeof(byte),
                oldNullable: true);

            migrationBuilder.AlterColumn<byte>(
                name: "PassAirYards",
                table: "NFLOffensiveGames",
                type: "tinyint",
                nullable: false,
                oldClrType: typeof(byte),
                oldNullable: true);

            migrationBuilder.AlterColumn<byte>(
                name: "FumblesLost",
                table: "NFLOffensiveGames",
                type: "tinyint",
                nullable: false,
                oldClrType: typeof(byte),
                oldNullable: true);

            migrationBuilder.AlterColumn<byte>(
                name: "Fumbles",
                table: "NFLOffensiveGames",
                type: "tinyint",
                nullable: false,
                oldClrType: typeof(byte),
                oldNullable: true);
        }
    }
}
