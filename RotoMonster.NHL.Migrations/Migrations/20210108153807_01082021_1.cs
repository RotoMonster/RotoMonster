using Microsoft.EntityFrameworkCore.Migrations;

namespace RotoMonster.NHL.Migrations.Migrations
{
    public partial class _01082021_1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NHLGoalieGames",
                columns: table => new
                {
                    PlayerId = table.Column<int>(nullable: false),
                    GameId = table.Column<int>(nullable: false),
                    TeamId = table.Column<int>(nullable: false),
                    Started = table.Column<byte>(nullable: false),
                    Shifts = table.Column<byte>(nullable: false),
                    Credit = table.Column<string>(nullable: true),
                    PowerPlayTimeOnIce = table.Column<double>(nullable: false),
                    PowerPlayShotsAgainst = table.Column<byte>(nullable: false),
                    PowerPlayGoalsAgainst = table.Column<byte>(nullable: false),
                    PowerPlaySaves = table.Column<byte>(nullable: false),
                    ShorthandedTimeOnIce = table.Column<double>(nullable: false),
                    ShorthandedShotsAgainst = table.Column<byte>(nullable: false),
                    ShorthandedGoalsAgainst = table.Column<byte>(nullable: false),
                    ShorthandedPlaySaves = table.Column<byte>(nullable: false),
                    EvenstrengthTimeOnIce = table.Column<double>(nullable: false),
                    EvenstrengthShotsAgainst = table.Column<byte>(nullable: false),
                    EvenstrengthGoalsAgainst = table.Column<byte>(nullable: false),
                    EvenstrengthPlaySaves = table.Column<byte>(nullable: false),
                    PenaltyShotsAgainst = table.Column<byte>(nullable: false),
                    PenaltyGoalsAgainst = table.Column<byte>(nullable: false),
                    PenaltySaves = table.Column<byte>(nullable: false),
                    ShootoutShotsAgainst = table.Column<byte>(nullable: false),
                    ShootoutGoalsAgainst = table.Column<byte>(nullable: false),
                    ShootoutSaves = table.Column<byte>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NHLGoalieGames", x => new { x.PlayerId, x.GameId });
                });

            migrationBuilder.CreateTable(
                name: "NHLSkaterGames",
                columns: table => new
                {
                    PlayerId = table.Column<int>(nullable: false),
                    GameId = table.Column<int>(nullable: false),
                    TeamId = table.Column<int>(nullable: false),
                    Started = table.Column<byte>(nullable: false),
                    PowerPlayTimeOnIce = table.Column<double>(nullable: false),
                    PowerPlayShots = table.Column<byte>(nullable: false),
                    PowerPlayGoals = table.Column<byte>(nullable: false),
                    PowerPlayMissedShots = table.Column<byte>(nullable: false),
                    PowerPlayAssists = table.Column<byte>(nullable: false),
                    PowerPlayFaceoffsWon = table.Column<byte>(nullable: false),
                    PowerPlayFaceoffsLost = table.Column<byte>(nullable: false),
                    ShorthandedTimeOnIce = table.Column<double>(nullable: false),
                    ShorthandedShots = table.Column<byte>(nullable: false),
                    ShorthandedGoals = table.Column<byte>(nullable: false),
                    ShorthandedMissedShots = table.Column<byte>(nullable: false),
                    ShorthandedAssists = table.Column<byte>(nullable: false),
                    ShorthandedFaceoffsWon = table.Column<byte>(nullable: false),
                    ShorthandedFaceoffsLost = table.Column<byte>(nullable: false),
                    EvenstrengthTimeOnIce = table.Column<double>(nullable: false),
                    EvenstrengthShots = table.Column<byte>(nullable: false),
                    EvenstrengthGoals = table.Column<byte>(nullable: false),
                    EvenstrengthMissedShots = table.Column<byte>(nullable: false),
                    EvenstrengthAssists = table.Column<byte>(nullable: false),
                    EvenstrengthFaceoffsWon = table.Column<byte>(nullable: false),
                    EvenstrengthFaceoffsLost = table.Column<byte>(nullable: false),
                    PenaltyShots = table.Column<byte>(nullable: false),
                    PenaltyGoals = table.Column<byte>(nullable: false),
                    PenaltyMissedShots = table.Column<byte>(nullable: false),
                    PenaltyAssists = table.Column<byte>(nullable: false),
                    ShootoutShots = table.Column<byte>(nullable: false),
                    ShootoutGoals = table.Column<byte>(nullable: false),
                    ShootoutMissedShots = table.Column<byte>(nullable: false),
                    Penalties = table.Column<byte>(nullable: false),
                    PenaltyMinutes = table.Column<double>(nullable: false),
                    BlockedAttempts = table.Column<byte>(nullable: false),
                    Hits = table.Column<byte>(nullable: false),
                    Giveaways = table.Column<byte>(nullable: false),
                    Takeaways = table.Column<byte>(nullable: false),
                    BlockedShots = table.Column<byte>(nullable: false),
                    PlusMinus = table.Column<byte>(nullable: false),
                    OvertimeGoals = table.Column<byte>(nullable: false),
                    OvertimeAssists = table.Column<byte>(nullable: false),
                    OvertimeShots = table.Column<byte>(nullable: false),
                    PenaltiesMajor = table.Column<byte>(nullable: false),
                    PenaltiesMinor = table.Column<byte>(nullable: false),
                    PenaltiesMisconduct = table.Column<byte>(nullable: false),
                    EmptynetGoals = table.Column<byte>(nullable: false),
                    Shifts = table.Column<byte>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NHLSkaterGames", x => new { x.PlayerId, x.GameId });
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NHLGoalieGames");

            migrationBuilder.DropTable(
                name: "NHLSkaterGames");
        }
    }
}
