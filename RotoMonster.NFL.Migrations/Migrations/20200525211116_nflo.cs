using Microsoft.EntityFrameworkCore.Migrations;

namespace RotoMonster.NFL.Migrations.Migrations
{
    public partial class nflo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NFLOffensiveGames",
                columns: table => new
                {
                    PlayerId = table.Column<int>(nullable: false),
                    GameId = table.Column<int>(nullable: false),
                    RushTD = table.Column<byte>(nullable: false),
                    RushFumbles = table.Column<byte>(nullable: false),
                    RecTargets = table.Column<byte>(nullable: false),
                    RecReceptions = table.Column<byte>(nullable: false),
                    RecYards = table.Column<int>(nullable: false),
                    RecTD = table.Column<byte>(nullable: false),
                    Fumbles = table.Column<byte>(nullable: false),
                    FumblesLost = table.Column<byte>(nullable: false),
                    RushRedzoneAttempted = table.Column<byte>(nullable: false),
                    RushYardsLost = table.Column<byte>(nullable: false),
                    RushLost = table.Column<byte>(nullable: false),
                    RushBrokenTackles = table.Column<byte>(nullable: false),
                    RushYardsAfterContact = table.Column<byte>(nullable: false),
                    RushKneelDowns = table.Column<byte>(nullable: false),
                    RecYardsAfterCatch = table.Column<byte>(nullable: false),
                    RecRedzoneTargets = table.Column<byte>(nullable: false),
                    RecAirYards = table.Column<byte>(nullable: false),
                    RecBrokenTackles = table.Column<byte>(nullable: false),
                    RecDroppedPasses = table.Column<byte>(nullable: false),
                    RecCatchablePasses = table.Column<byte>(nullable: false),
                    RecYardsAafterContact = table.Column<byte>(nullable: false),
                    PassRating = table.Column<double>(nullable: false),
                    PassAirYards = table.Column<byte>(nullable: false),
                    RassRedzoneAttempts = table.Column<byte>(nullable: false),
                    PassThrowAways = table.Column<byte>(nullable: false),
                    PassPoorThrows = table.Column<byte>(nullable: false),
                    PassDefendedPasses = table.Column<byte>(nullable: false),
                    PassDroppedPasses = table.Column<byte>(nullable: false),
                    PassSpikes = table.Column<byte>(nullable: false),
                    PassBlitzes = table.Column<byte>(nullable: false),
                    PassHurries = table.Column<byte>(nullable: false),
                    PassKnockdowns = table.Column<byte>(nullable: false),
                    PassPocketTime = table.Column<byte>(nullable: false),
                    ReturnReturns = table.Column<byte>(nullable: false),
                    ReturnYards = table.Column<byte>(nullable: false),
                    ReturnTD = table.Column<byte>(nullable: false),
                    ReturnFaircatches = table.Column<byte>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NFLOffensiveGames", x => new { x.PlayerId, x.GameId });
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NFLOffensiveGames");
        }
    }
}
