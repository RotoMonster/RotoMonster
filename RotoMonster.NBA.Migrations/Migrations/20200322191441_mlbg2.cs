using Microsoft.EntityFrameworkCore.Migrations;

namespace RotoMonster.Data.Migrations
{
    public partial class mlbg2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MLBHitterGames",
                columns: table => new
                {
                    PlayerId = table.Column<int>(nullable: false),
                    GameId = table.Column<int>(nullable: false),
                    AB = table.Column<byte>(nullable: false),
                    R = table.Column<byte>(nullable: false),
                    Hits = table.Column<byte>(nullable: false),
                    RBI = table.Column<byte>(nullable: false),
                    BB = table.Column<byte>(nullable: false),
                    K = table.Column<byte>(nullable: false),
                    LOB = table.Column<byte>(nullable: false),
                    Singles = table.Column<byte>(nullable: false),
                    Doubles = table.Column<byte>(nullable: false),
                    Triples = table.Column<byte>(nullable: false),
                    HR = table.Column<byte>(nullable: false),
                    SB = table.Column<byte>(nullable: false),
                    SBCaught = table.Column<byte>(nullable: false),
                    SacFlies = table.Column<byte>(nullable: false),
                    SacBunts = table.Column<byte>(nullable: false),
                    HBP = table.Column<byte>(nullable: false),
                    RBITwoOut = table.Column<byte>(nullable: false),
                    GrandSlams = table.Column<byte>(nullable: false),
                    GIDP = table.Column<byte>(nullable: false),
                    Errors = table.Column<byte>(nullable: false),
                    PastBalls = table.Column<byte>(nullable: false),
                    Starts = table.Column<byte>(nullable: false),
                    PA = table.Column<byte>(nullable: false),
                    BattingOrder = table.Column<byte>(nullable: false),
                    Assists = table.Column<byte>(nullable: false),
                    FullInnings = table.Column<byte>(nullable: false),
                    ThirdInnings = table.Column<byte>(nullable: false),
                    Putouts = table.Column<byte>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MLBHitterGames", x => new { x.PlayerId, x.GameId });
                    table.ForeignKey(
                        name: "FK_MLBHitterGames_Games_GameId",
                        column: x => x.GameId,
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MLBHitterGames_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MLBPitcherGames",
                columns: table => new
                {
                    PlayerId = table.Column<int>(nullable: false),
                    GameId = table.Column<int>(nullable: false),
                    FullInnings = table.Column<byte>(nullable: false),
                    ThirdInnings = table.Column<byte>(nullable: false),
                    HitsAllowed = table.Column<byte>(nullable: false),
                    RunsAgainst = table.Column<byte>(nullable: false),
                    RunsEarned = table.Column<byte>(nullable: false),
                    BB = table.Column<byte>(nullable: false),
                    BBI = table.Column<byte>(nullable: false),
                    K = table.Column<byte>(nullable: false),
                    HR = table.Column<byte>(nullable: false),
                    Pitches = table.Column<byte>(nullable: false),
                    Strikes = table.Column<byte>(nullable: false),
                    OutsGroundBalls = table.Column<byte>(nullable: false),
                    OutsFlyBalls = table.Column<byte>(nullable: false),
                    Outs = table.Column<byte>(nullable: false),
                    HBP = table.Column<byte>(nullable: false),
                    WildPitches = table.Column<byte>(nullable: false),
                    W = table.Column<byte>(nullable: false),
                    L = table.Column<byte>(nullable: false),
                    S = table.Column<byte>(nullable: false),
                    Holds = table.Column<byte>(nullable: false),
                    Balks = table.Column<byte>(nullable: false),
                    Shutouts = table.Column<byte>(nullable: false),
                    CG = table.Column<byte>(nullable: false),
                    BS = table.Column<byte>(nullable: false),
                    Singles = table.Column<byte>(nullable: false),
                    Doubles = table.Column<byte>(nullable: false),
                    Triples = table.Column<byte>(nullable: false),
                    SacFlies = table.Column<byte>(nullable: false),
                    SacBunts = table.Column<byte>(nullable: false),
                    PickOffs = table.Column<byte>(nullable: false),
                    InheritedRunners = table.Column<byte>(nullable: false),
                    InheritedRunnersScored = table.Column<byte>(nullable: false),
                    GamesFinished = table.Column<byte>(nullable: false),
                    BoxscoreOrder = table.Column<byte>(nullable: false),
                    QS = table.Column<byte>(nullable: false),
                    AtBatsAgainst = table.Column<byte>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MLBPitcherGames", x => new { x.PlayerId, x.GameId });
                    table.ForeignKey(
                        name: "FK_MLBPitcherGames_Games_GameId",
                        column: x => x.GameId,
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MLBPitcherGames_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MLBHitterGames_GameId",
                table: "MLBHitterGames",
                column: "GameId");

            migrationBuilder.CreateIndex(
                name: "IX_MLBPitcherGames_GameId",
                table: "MLBPitcherGames",
                column: "GameId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MLBHitterGames");

            migrationBuilder.DropTable(
                name: "MLBPitcherGames");
        }
    }
}
