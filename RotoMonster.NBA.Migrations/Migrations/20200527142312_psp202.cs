using Microsoft.EntityFrameworkCore.Migrations;

namespace RotoMonster.Data.Migrations
{
    public partial class psp202 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DefaultScoringSystem",
                table: "Sports",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FantasyProviderId",
                table: "PositionSources",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "DefaultPointsPerStat",
                table: "Categories",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "NFLOffensiveGames",
                columns: table => new
                {
                    PlayerId = table.Column<int>(nullable: false),
                    GameId = table.Column<int>(nullable: false),
                    TeamId = table.Column<int>(nullable: false),
                    PassAttempts = table.Column<byte>(nullable: true),
                    PassCompletions = table.Column<byte>(nullable: true),
                    PassYards = table.Column<int>(nullable: true),
                    PassTD = table.Column<byte>(nullable: true),
                    PassInt = table.Column<byte>(nullable: true),
                    PassSacks = table.Column<byte>(nullable: true),
                    PassSackYards = table.Column<int>(nullable: true),
                    RushAttempts = table.Column<byte>(nullable: true),
                    RushYards = table.Column<int>(nullable: true),
                    RushTD = table.Column<byte>(nullable: true),
                    RushFumbles = table.Column<byte>(nullable: true),
                    RecTargets = table.Column<byte>(nullable: true),
                    RecReceptions = table.Column<byte>(nullable: true),
                    RecYards = table.Column<int>(nullable: true),
                    RecTD = table.Column<byte>(nullable: true),
                    Fumbles = table.Column<byte>(nullable: true),
                    FumblesLost = table.Column<byte>(nullable: true),
                    RushRedzoneAttempted = table.Column<byte>(nullable: true),
                    RushYardsLost = table.Column<int>(nullable: true),
                    RushLost = table.Column<byte>(nullable: true),
                    RushBrokenTackles = table.Column<byte>(nullable: true),
                    RushYardsAfterContact = table.Column<int>(nullable: true),
                    RushKneelDowns = table.Column<byte>(nullable: true),
                    RecYardsAfterCatch = table.Column<int>(nullable: true),
                    RecRedzoneTargets = table.Column<byte>(nullable: true),
                    RecAirYards = table.Column<int>(nullable: true),
                    RecBrokenTackles = table.Column<byte>(nullable: true),
                    RecDroppedPasses = table.Column<byte>(nullable: true),
                    RecCatchablePasses = table.Column<byte>(nullable: true),
                    RecYardsAafterContact = table.Column<int>(nullable: true),
                    PassRating = table.Column<double>(nullable: true),
                    PassAirYards = table.Column<int>(nullable: true),
                    RassRedzoneAttempts = table.Column<byte>(nullable: true),
                    PassThrowAways = table.Column<byte>(nullable: true),
                    PassPoorThrows = table.Column<byte>(nullable: true),
                    PassDefendedPasses = table.Column<byte>(nullable: true),
                    PassDroppedPasses = table.Column<byte>(nullable: true),
                    PassSpikes = table.Column<byte>(nullable: true),
                    PassBlitzes = table.Column<byte>(nullable: true),
                    PassHurries = table.Column<byte>(nullable: true),
                    PassKnockdowns = table.Column<byte>(nullable: true),
                    PassPocketTime = table.Column<double>(nullable: true),
                    ReturnReturns = table.Column<byte>(nullable: true),
                    ReturnYards = table.Column<int>(nullable: true),
                    ReturnTD = table.Column<byte>(nullable: true),
                    ReturnFaircatches = table.Column<byte>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NFLOffensiveGames", x => new { x.PlayerId, x.GameId });
                });

            migrationBuilder.CreateIndex(
                name: "IX_PositionSourcePositions_PositionId",
                table: "PositionSourcePositions",
                column: "PositionId");

            migrationBuilder.AddForeignKey(
                name: "FK_PositionSourcePositions_Positions_PositionId",
                table: "PositionSourcePositions",
                column: "PositionId",
                principalTable: "Positions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PositionSourcePositions_PositionSources_PositionSourceId",
                table: "PositionSourcePositions",
                column: "PositionSourceId",
                principalTable: "PositionSources",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PositionSourcePositions_Positions_PositionId",
                table: "PositionSourcePositions");

            migrationBuilder.DropForeignKey(
                name: "FK_PositionSourcePositions_PositionSources_PositionSourceId",
                table: "PositionSourcePositions");

            migrationBuilder.DropTable(
                name: "NFLOffensiveGames");

            migrationBuilder.DropIndex(
                name: "IX_PositionSourcePositions_PositionId",
                table: "PositionSourcePositions");

            migrationBuilder.DropColumn(
                name: "DefaultScoringSystem",
                table: "Sports");

            migrationBuilder.DropColumn(
                name: "FantasyProviderId",
                table: "PositionSources");

            migrationBuilder.DropColumn(
                name: "DefaultPointsPerStat",
                table: "Categories");
        }
    }
}
