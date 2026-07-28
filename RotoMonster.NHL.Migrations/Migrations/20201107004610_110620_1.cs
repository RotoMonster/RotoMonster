using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace RotoMonster.NHL.Migrations.Migrations
{
    public partial class _110620_1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FantasyProviderPlayers_FantasyProviders_FantasyProviderId",
                table: "FantasyProviderPlayers");

            migrationBuilder.DropForeignKey(
                name: "FK_FantasyProviderPlayers_Players_PlayerId",
                table: "FantasyProviderPlayers");

            migrationBuilder.DropForeignKey(
                name: "FK_Games_Seasons_SeasonId",
                table: "Games");

            migrationBuilder.DropForeignKey(
                name: "FK_PerValues_Categories_CategoryId",
                table: "PerValues");

            migrationBuilder.DropTable(
                name: "PlayerGames");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FantasyProviderPlayers",
                table: "FantasyProviderPlayers");

            migrationBuilder.DropIndex(
                name: "IX_FantasyProviderPlayers_FantasyProviderId",
                table: "FantasyProviderPlayers");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "FantasyProviderPlayers");

            migrationBuilder.DropColumn(
                name: "IsCalculated",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "IsStat",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "PerDisplayFormat",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "Property",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "TotalDisplayFormat",
                table: "Categories");

            migrationBuilder.AddColumn<string>(
                name: "SportRadarId",
                table: "Teams",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Salary",
                table: "SeasonPlayers",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActualPosition",
                table: "Positions",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDefault",
                table: "PlayerTypes",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDisabled",
                table: "PlayerTypes",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Bats",
                table: "Players",
                maxLength: 1,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Throws",
                table: "Players",
                maxLength: 1,
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "CategoryId",
                table: "PerValues",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "ColumnTitle",
                table: "PerValues",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDefault",
                table: "PerValues",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "SkillCategoryValue",
                table: "PerValues",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "SeasonId",
                table: "Games",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsFinished",
                table: "Games",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DisplayOrder",
                table: "FantasyProviders",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsDefault",
                table: "FantasyProviders",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LeagueURL",
                table: "FantasyProviders",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "PlayerId",
                table: "FantasyProviderPlayers",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "FantasyProviderId",
                table: "FantasyProviderPlayers",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DisplayOrder",
                table: "Divisions",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "CBSId",
                table: "Categories",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "DefaultPointsPerStat",
                table: "Categories",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ESPNId",
                table: "Categories",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FanTraxGroup",
                table: "Categories",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FanTraxId",
                table: "Categories",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDefaultDisplayCategory",
                table: "Categories",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDisplayCategory",
                table: "Categories",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "OtherAbbreviations",
                table: "Categories",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SourceField",
                table: "Categories",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "YahooId",
                table: "Categories",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_FantasyProviderPlayers",
                table: "FantasyProviderPlayers",
                columns: new[] { "FantasyProviderId", "PlayerId" });

            migrationBuilder.CreateTable(
                name: "ActiveRosterSpots",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(nullable: true),
                    YahooTitle = table.Column<string>(nullable: true),
                    FanTraxTitle = table.Column<string>(nullable: true),
                    DefaultNumberOf = table.Column<int>(nullable: false),
                    DisplayOrder = table.Column<int>(nullable: false),
                    FilterDisplayOrder = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActiveRosterSpots", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CategoryPerValues",
                columns: table => new
                {
                    CategoryId = table.Column<int>(nullable: false),
                    PerValueId = table.Column<int>(nullable: false),
                    DisplayFormat = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoryPerValues", x => new { x.CategoryId, x.PerValueId });
                    table.ForeignKey(
                        name: "FK_CategoryPerValues_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CategoryPerValues_PerValues_PerValueId",
                        column: x => x.PerValueId,
                        principalTable: "PerValues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DisplayCategories",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryId = table.Column<int>(nullable: false),
                    DisplayOrder = table.Column<int>(nullable: false),
                    IsBeforeStats = table.Column<bool>(nullable: false),
                    IsAfterStats = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DisplayCategories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DisplayCategories_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Drafts",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SeasonId = table.Column<int>(nullable: false),
                    FantasyProviderId = table.Column<int>(nullable: false),
                    DateAdded = table.Column<DateTime>(nullable: false),
                    DraftDate = table.Column<DateTime>(nullable: false),
                    Title = table.Column<string>(nullable: true),
                    ProviderLeagueId = table.Column<string>(nullable: true),
                    LeagueSize = table.Column<int>(nullable: false),
                    NumberOfTeams = table.Column<int>(nullable: false),
                    IsProLeague = table.Column<bool>(nullable: false),
                    IsAuction = table.Column<bool>(nullable: false),
                    IsMoney = table.Column<bool>(nullable: false),
                    LeagueType = table.Column<string>(nullable: true),
                    IsMock = table.Column<bool>(nullable: false),
                    IsFinished = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Drafts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Drafts_FantasyProviders_FantasyProviderId",
                        column: x => x.FantasyProviderId,
                        principalTable: "FantasyProviders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Drafts_Seasons_SeasonId",
                        column: x => x.SeasonId,
                        principalTable: "Seasons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExtraAnalysisLeagues",
                columns: table => new
                {
                    FantasyProviderId = table.Column<int>(nullable: false),
                    ProviderId = table.Column<string>(nullable: false),
                    Title = table.Column<string>(nullable: true),
                    EntryFee = table.Column<int>(nullable: true),
                    NumberOfTeams = table.Column<int>(nullable: true),
                    DraftDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExtraAnalysisLeagues", x => new { x.FantasyProviderId, x.ProviderId });
                    table.ForeignKey(
                        name: "FK_ExtraAnalysisLeagues_FantasyProviders_FantasyProviderId",
                        column: x => x.FantasyProviderId,
                        principalTable: "FantasyProviders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MLBHitterGames",
                columns: table => new
                {
                    PlayerId = table.Column<int>(nullable: false),
                    GameId = table.Column<int>(nullable: false),
                    TeamId = table.Column<int>(nullable: false),
                    AB = table.Column<byte>(nullable: false),
                    R = table.Column<byte>(nullable: false),
                    H = table.Column<byte>(nullable: false),
                    RBI = table.Column<byte>(nullable: false),
                    BB = table.Column<byte>(nullable: false),
                    K = table.Column<byte>(nullable: false),
                    LOB = table.Column<byte>(nullable: false),
                    Singles = table.Column<byte>(nullable: false),
                    Doubles = table.Column<byte>(nullable: false),
                    Triples = table.Column<byte>(nullable: false),
                    HR = table.Column<byte>(nullable: false),
                    SB = table.Column<byte>(nullable: false),
                    CS = table.Column<byte>(nullable: false),
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
                    table.ForeignKey(
                        name: "FK_MLBHitterGames_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MLBPitcherGames",
                columns: table => new
                {
                    PlayerId = table.Column<int>(nullable: false),
                    GameId = table.Column<int>(nullable: false),
                    TeamId = table.Column<int>(nullable: false),
                    Innings = table.Column<double>(nullable: false),
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
                    GamesStarted = table.Column<byte>(nullable: false),
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
                    table.ForeignKey(
                        name: "FK_MLBPitcherGames_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NFLDefenseGames",
                columns: table => new
                {
                    PlayerId = table.Column<int>(nullable: false),
                    GameId = table.Column<int>(nullable: false),
                    TeamId = table.Column<int>(nullable: false),
                    Sacks = table.Column<byte>(nullable: true),
                    Interceptions = table.Column<byte>(nullable: true),
                    FumbleRecoveries = table.Column<byte>(nullable: true),
                    Touchdowns = table.Column<byte>(nullable: true),
                    Safeties = table.Column<byte>(nullable: true),
                    BlockedKicks = table.Column<byte>(nullable: true),
                    XpReturned = table.Column<byte>(nullable: true),
                    Points = table.Column<byte>(nullable: true),
                    PassAttempts = table.Column<byte>(nullable: true),
                    PassCompletion = table.Column<byte>(nullable: true),
                    PassYards = table.Column<short>(nullable: true),
                    PassTouchdowns = table.Column<byte>(nullable: true),
                    RushAttempts = table.Column<byte>(nullable: true),
                    RushYards = table.Column<short>(nullable: true),
                    RushTouchdowns = table.Column<byte>(nullable: true),
                    ReceivingAirYards = table.Column<short>(nullable: true),
                    PassSacks = table.Column<byte>(nullable: true),
                    Minutes = table.Column<double>(nullable: true),
                    Points0 = table.Column<byte>(nullable: true),
                    Points1to6 = table.Column<byte>(nullable: true),
                    Points7to13 = table.Column<byte>(nullable: true),
                    Points14to20 = table.Column<byte>(nullable: true),
                    Points21to27 = table.Column<byte>(nullable: true),
                    Points28to34 = table.Column<byte>(nullable: true),
                    Points35 = table.Column<byte>(nullable: true),
                    Points2to10 = table.Column<byte>(nullable: true),
                    Points11to20 = table.Column<byte>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NFLDefenseGames", x => new { x.PlayerId, x.GameId });
                    table.ForeignKey(
                        name: "FK_NFLDefenseGames_Games_GameId",
                        column: x => x.GameId,
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NFLDefenseGames_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NFLGames",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GameId = table.Column<int>(nullable: false),
                    OverUnder = table.Column<double>(nullable: false),
                    HomeSpread = table.Column<double>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NFLGames", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NFLGames_Games_GameId",
                        column: x => x.GameId,
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NFLKickerGames",
                columns: table => new
                {
                    PlayerId = table.Column<int>(nullable: false),
                    GameId = table.Column<int>(nullable: false),
                    TeamId = table.Column<int>(nullable: false),
                    FieldGoals = table.Column<byte>(nullable: true),
                    FieldGoalsMade = table.Column<byte>(nullable: true),
                    FieldGoals0to19 = table.Column<byte>(nullable: true),
                    FieldGoals20to29 = table.Column<byte>(nullable: true),
                    FieldGoals30to39 = table.Column<byte>(nullable: true),
                    FieldGoals40to49 = table.Column<byte>(nullable: true),
                    FieldGoals50 = table.Column<byte>(nullable: true),
                    FieldGoalsBlocked = table.Column<byte>(nullable: true),
                    FieldGoalsYards = table.Column<int>(nullable: true),
                    FieldGoalsLongest = table.Column<byte>(nullable: true),
                    ExtraPointsAttempts = table.Column<byte>(nullable: true),
                    ExtraPointsBlocked = table.Column<byte>(nullable: true),
                    ExtraPointsMade = table.Column<byte>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NFLKickerGames", x => new { x.PlayerId, x.GameId });
                    table.ForeignKey(
                        name: "FK_NFLKickerGames_Games_GameId",
                        column: x => x.GameId,
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NFLKickerGames_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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
                    table.ForeignKey(
                        name: "FK_NFLOffensiveGames_Games_GameId",
                        column: x => x.GameId,
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NFLOffensiveGames_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OwnershipPlayers",
                columns: table => new
                {
                    GameDate = table.Column<DateTime>(nullable: false),
                    PlayerId = table.Column<int>(nullable: false),
                    CategoriesCode = table.Column<string>(nullable: false),
                    LeagueSize = table.Column<int>(nullable: false),
                    LeagueCount = table.Column<int>(nullable: false),
                    OwnCount = table.Column<int>(nullable: false),
                    ActiveCount = table.Column<int>(nullable: false),
                    IRCount = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OwnershipPlayers", x => new { x.GameDate, x.PlayerId, x.CategoriesCode });
                    table.ForeignKey(
                        name: "FK_OwnershipPlayers_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlayedGamesMissed",
                columns: table => new
                {
                    GameId = table.Column<int>(nullable: false),
                    PlayerId = table.Column<int>(nullable: false),
                    TeamId = table.Column<int>(nullable: false),
                    IsActive = table.Column<bool>(nullable: true),
                    Started = table.Column<bool>(nullable: true),
                    Played = table.Column<bool>(nullable: true),
                    NotPlayingReason = table.Column<string>(maxLength: 100, nullable: true),
                    NotPlayingDescription = table.Column<string>(maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayedGamesMissed", x => new { x.GameId, x.PlayerId });
                    table.ForeignKey(
                        name: "FK_PlayedGamesMissed_Games_GameId",
                        column: x => x.GameId,
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlayedGamesMissed_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlayedGamesMissed_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlayerInjuries",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlayerId = table.Column<int>(nullable: false),
                    TeamId = table.Column<int>(nullable: true),
                    DownloadDate = table.Column<DateTime>(nullable: false),
                    ProviderInjuryId = table.Column<string>(nullable: true),
                    PlayerStatus = table.Column<string>(nullable: true),
                    InjuryStatus = table.Column<string>(nullable: true),
                    Comment = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    StartDate = table.Column<DateTime>(nullable: true),
                    UpdateDate = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerInjuries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PlayerStatusTagTypes",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(nullable: true),
                    Icon = table.Column<string>(nullable: true),
                    IsDefault = table.Column<bool>(nullable: true),
                    DisplayOrder = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerStatusTagTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PlayerStatusTypes",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(nullable: true),
                    BackgroundColor = table.Column<string>(nullable: true),
                    TextColor = table.Column<string>(nullable: true),
                    TextFormat = table.Column<string>(nullable: true),
                    AutoClear = table.Column<bool>(nullable: true),
                    UsesDate = table.Column<bool>(nullable: true),
                    ShowInDaily = table.Column<bool>(nullable: true),
                    AllowFilter = table.Column<bool>(nullable: true),
                    AppliesToNextGame = table.Column<bool>(nullable: true),
                    IsInGame = table.Column<bool>(nullable: true),
                    IsUndetermined = table.Column<bool>(nullable: true),
                    ShowOnPlayerProfile = table.Column<bool>(nullable: true),
                    EndOfGameMissedPlayerStatusTypeId = table.Column<int>(nullable: true),
                    EndOfGamePlayedPlayerStatusTypeId = table.Column<int>(nullable: true),
                    TweetTemplate = table.Column<string>(nullable: true),
                    UpdateTemplate = table.Column<string>(nullable: true),
                    PlayType = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerStatusTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PositionSourcePlayers",
                columns: table => new
                {
                    SeasonId = table.Column<int>(nullable: false),
                    PositionSourceId = table.Column<int>(nullable: false),
                    PlayerId = table.Column<int>(nullable: false),
                    PositionId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PositionSourcePlayers", x => new { x.SeasonId, x.PositionSourceId, x.PlayerId, x.PositionId });
                    table.ForeignKey(
                        name: "FK_PositionSourcePlayers_Positions_PositionId",
                        column: x => x.PositionId,
                        principalTable: "Positions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PositionSources",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProviderId = table.Column<int>(nullable: false),
                    FantasyProviderId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PositionSources", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PositionSources_FantasyProviders_FantasyProviderId",
                        column: x => x.FantasyProviderId,
                        principalTable: "FantasyProviders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Sports",
                columns: table => new
                {
                    Title = table.Column<string>(maxLength: 3, nullable: true),
                    SportType = table.Column<string>(maxLength: 20, nullable: true),
                    DivisionTitle = table.Column<string>(maxLength: 20, nullable: true),
                    UsesCategories = table.Column<bool>(nullable: false),
                    UsesPointsPerStat = table.Column<bool>(nullable: false),
                    DefaultScoringSystem = table.Column<string>(nullable: true),
                    StartDayOfWeek = table.Column<int>(nullable: false),
                    MenuColor = table.Column<string>(nullable: true),
                    LogoColor = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "TeamAliases",
                columns: table => new
                {
                    TeamId = table.Column<int>(nullable: false),
                    Alias = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeamAliases", x => new { x.TeamId, x.Alias });
                    table.ForeignKey(
                        name: "FK_TeamAliases_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserDisplayCategories",
                columns: table => new
                {
                    UserId = table.Column<string>(maxLength: 260, nullable: false),
                    CategoryId = table.Column<int>(nullable: false),
                    DisplayOrder = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserDisplayCategories", x => new { x.UserId, x.CategoryId });
                    table.ForeignKey(
                        name: "FK_UserDisplayCategories_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserLeagues",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SeasonId = table.Column<int>(nullable: false),
                    UserId = table.Column<string>(maxLength: 450, nullable: false),
                    Title = table.Column<string>(maxLength: 250, nullable: false),
                    DisplayTitle = table.Column<string>(maxLength: 250, nullable: false),
                    FantasyProviderId = table.Column<int>(nullable: false),
                    ProviderLeagueId = table.Column<string>(maxLength: 100, nullable: true),
                    MyProviderTeamId = table.Column<string>(maxLength: 100, nullable: true),
                    TrackLeague = table.Column<bool>(nullable: false),
                    MyTeamTitle = table.Column<string>(maxLength: 250, nullable: true),
                    ScoringSystem = table.Column<string>(maxLength: 10, nullable: false),
                    LeagueType = table.Column<string>(maxLength: 10, nullable: false),
                    LineupFrequency = table.Column<string>(maxLength: 10, nullable: false),
                    NumberOfTeams = table.Column<int>(nullable: false),
                    PlayersPerTeam = table.Column<int>(nullable: false),
                    IRSpots = table.Column<int>(nullable: false),
                    StartWeekday = table.Column<int>(nullable: false),
                    QualityGamesLimit = table.Column<int>(nullable: false),
                    SameDayTransactions = table.Column<bool>(nullable: false),
                    IsAuction = table.Column<bool>(nullable: false),
                    IsMoney = table.Column<bool>(nullable: false),
                    IsProLeague = table.Column<bool>(nullable: false),
                    DraftDate = table.Column<DateTime>(nullable: true),
                    AutoEndDate = table.Column<bool>(nullable: false),
                    GameLimit = table.Column<int>(nullable: false),
                    AutoUpdate = table.Column<bool>(nullable: false),
                    ContinuousWaivers = table.Column<bool>(nullable: false),
                    HasDrafted = table.Column<bool>(nullable: false),
                    WaiverType = table.Column<string>(nullable: true),
                    WaiverRule = table.Column<string>(nullable: true),
                    DisplayOrder = table.Column<int>(nullable: false),
                    LastSelectedDate = table.Column<DateTime>(nullable: true),
                    UpdatedDate = table.Column<DateTime>(nullable: true),
                    CreatedDate = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserLeagues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserLeagues_FantasyProviders_FantasyProviderId",
                        column: x => x.FantasyProviderId,
                        principalTable: "FantasyProviders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserLeagues_Seasons_SeasonId",
                        column: x => x.SeasonId,
                        principalTable: "Seasons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ActiveRosterSpotPositions",
                columns: table => new
                {
                    ActiveRosterSpotId = table.Column<int>(nullable: false),
                    PositionId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActiveRosterSpotPositions", x => new { x.ActiveRosterSpotId, x.PositionId });
                    table.ForeignKey(
                        name: "FK_ActiveRosterSpotPositions_ActiveRosterSpots_ActiveRosterSpotId",
                        column: x => x.ActiveRosterSpotId,
                        principalTable: "ActiveRosterSpots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ActiveRosterSpotPositions_Positions_PositionId",
                        column: x => x.PositionId,
                        principalTable: "Positions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DraftPlayers",
                columns: table => new
                {
                    DraftId = table.Column<int>(nullable: false),
                    PlayerId = table.Column<int>(nullable: false),
                    DraftOrder = table.Column<int>(nullable: false),
                    Price = table.Column<int>(nullable: true),
                    ProviderTeamId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DraftPlayers", x => new { x.DraftId, x.PlayerId });
                    table.ForeignKey(
                        name: "FK_DraftPlayers_Drafts_DraftId",
                        column: x => x.DraftId,
                        principalTable: "Drafts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DraftPlayers_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DraftPlayerTypes",
                columns: table => new
                {
                    DraftId = table.Column<int>(nullable: false),
                    PlayerTypeId = table.Column<int>(nullable: false),
                    CategoriesCode = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DraftPlayerTypes", x => new { x.DraftId, x.PlayerTypeId });
                    table.ForeignKey(
                        name: "FK_DraftPlayerTypes_Drafts_DraftId",
                        column: x => x.DraftId,
                        principalTable: "Drafts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DraftPlayerTypes_PlayerTypes_PlayerTypeId",
                        column: x => x.PlayerTypeId,
                        principalTable: "PlayerTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlayerStatuses",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlayerId = table.Column<int>(nullable: false),
                    GameId = table.Column<int>(nullable: true),
                    IsActive = table.Column<bool>(nullable: false),
                    PlayerStatusTypeId = table.Column<int>(nullable: false),
                    PlayerStatusTagTypeId = table.Column<int>(nullable: true),
                    OwningUserId = table.Column<string>(nullable: true),
                    DateAdded = table.Column<DateTime>(nullable: false),
                    DateDeactivated = table.Column<DateTime>(nullable: true),
                    Comment = table.Column<string>(nullable: true),
                    Subject = table.Column<string>(nullable: true),
                    Source = table.Column<string>(nullable: true),
                    SourceUrl = table.Column<string>(nullable: true),
                    DateDeleted = table.Column<DateTime>(nullable: true),
                    DeletedByUserId = table.Column<string>(nullable: true),
                    GamePercent = table.Column<short>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerStatuses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlayerStatuses_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlayerStatuses_PlayerStatusTagTypes_PlayerStatusTagTypeId",
                        column: x => x.PlayerStatusTagTypeId,
                        principalTable: "PlayerStatusTagTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PlayerStatuses_PlayerStatusTypes_PlayerStatusTypeId",
                        column: x => x.PlayerStatusTypeId,
                        principalTable: "PlayerStatusTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PositionSourcePositions",
                columns: table => new
                {
                    PositionSourceId = table.Column<int>(nullable: false),
                    PositionId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PositionSourcePositions", x => new { x.PositionSourceId, x.PositionId });
                    table.ForeignKey(
                        name: "FK_PositionSourcePositions_Positions_PositionId",
                        column: x => x.PositionId,
                        principalTable: "Positions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PositionSourcePositions_PositionSources_PositionSourceId",
                        column: x => x.PositionSourceId,
                        principalTable: "PositionSources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserLeagueActiveRosterSpots",
                columns: table => new
                {
                    UserLeagueId = table.Column<int>(nullable: false),
                    ActiveRosterSpotId = table.Column<int>(nullable: false),
                    NumberOfPlayers = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserLeagueActiveRosterSpots", x => new { x.UserLeagueId, x.ActiveRosterSpotId });
                    table.ForeignKey(
                        name: "FK_UserLeagueActiveRosterSpots_ActiveRosterSpots_ActiveRosterSpotId",
                        column: x => x.ActiveRosterSpotId,
                        principalTable: "ActiveRosterSpots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserLeagueActiveRosterSpots_UserLeagues_UserLeagueId",
                        column: x => x.UserLeagueId,
                        principalTable: "UserLeagues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserLeagueCategories",
                columns: table => new
                {
                    UserLeagueId = table.Column<int>(nullable: false),
                    CategoryId = table.Column<int>(nullable: false),
                    IsActive = table.Column<bool>(nullable: false),
                    Weight = table.Column<double>(nullable: false),
                    PointsPerStat = table.Column<double>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserLeagueCategories", x => new { x.UserLeagueId, x.CategoryId });
                    table.ForeignKey(
                        name: "FK_UserLeagueCategories_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserLeagueCategories_UserLeagues_UserLeagueId",
                        column: x => x.UserLeagueId,
                        principalTable: "UserLeagues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserLeagueImportErrors",
                columns: table => new
                {
                    UserLeagueId = table.Column<int>(nullable: false),
                    Error = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserLeagueImportErrors", x => new { x.UserLeagueId, x.Error });
                    table.ForeignKey(
                        name: "FK_UserLeagueImportErrors_UserLeagues_UserLeagueId",
                        column: x => x.UserLeagueId,
                        principalTable: "UserLeagues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserLeagueMissingPlayers",
                columns: table => new
                {
                    UserLeagueId = table.Column<int>(nullable: false),
                    ProviderId = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserLeagueMissingPlayers", x => new { x.UserLeagueId, x.ProviderId });
                    table.ForeignKey(
                        name: "FK_UserLeagueMissingPlayers_UserLeagues_UserLeagueId",
                        column: x => x.UserLeagueId,
                        principalTable: "UserLeagues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserLeaguePlayerTypes",
                columns: table => new
                {
                    UserLeagueId = table.Column<int>(nullable: false),
                    PlayerTypeId = table.Column<int>(nullable: false),
                    CategoriesCode = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserLeaguePlayerTypes", x => new { x.UserLeagueId, x.PlayerTypeId });
                    table.ForeignKey(
                        name: "FK_UserLeaguePlayerTypes_PlayerTypes_PlayerTypeId",
                        column: x => x.PlayerTypeId,
                        principalTable: "PlayerTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserLeaguePlayerTypes_UserLeagues_UserLeagueId",
                        column: x => x.UserLeagueId,
                        principalTable: "UserLeagues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserLeagueTeams",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserLeagueId = table.Column<int>(nullable: false),
                    TeamNumber = table.Column<int>(nullable: false),
                    Title = table.Column<string>(nullable: true),
                    ProviderId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserLeagueTeams", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserLeagueTeams_UserLeagues_UserLeagueId",
                        column: x => x.UserLeagueId,
                        principalTable: "UserLeagues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserLeagueWaiverPlayers",
                columns: table => new
                {
                    UserLeagueId = table.Column<int>(nullable: false),
                    PlayerId = table.Column<int>(nullable: false),
                    AddedDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserLeagueWaiverPlayers", x => new { x.UserLeagueId, x.PlayerId });
                    table.ForeignKey(
                        name: "FK_UserLeagueWaiverPlayers_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserLeagueWaiverPlayers_UserLeagues_UserLeagueId",
                        column: x => x.UserLeagueId,
                        principalTable: "UserLeagues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserLeagueTeamPlayers",
                columns: table => new
                {
                    UserLeagueTeamId = table.Column<long>(nullable: false),
                    PlayerId = table.Column<int>(nullable: false),
                    IsActive = table.Column<bool>(nullable: false),
                    IsIR = table.Column<bool>(nullable: false),
                    PickNumber = table.Column<int>(nullable: false),
                    AuctionPrice = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserLeagueTeamPlayers", x => new { x.UserLeagueTeamId, x.PlayerId });
                    table.ForeignKey(
                        name: "FK_UserLeagueTeamPlayers_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserLeagueTeamPlayers_UserLeagueTeams_UserLeagueTeamId",
                        column: x => x.UserLeagueTeamId,
                        principalTable: "UserLeagueTeams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SeasonPlayers_TeamId",
                table: "SeasonPlayers",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_ActiveRosterSpotPositions_PositionId",
                table: "ActiveRosterSpotPositions",
                column: "PositionId");

            migrationBuilder.CreateIndex(
                name: "IX_CategoryPerValues_PerValueId",
                table: "CategoryPerValues",
                column: "PerValueId");

            migrationBuilder.CreateIndex(
                name: "IX_DisplayCategories_CategoryId",
                table: "DisplayCategories",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_DraftPlayers_PlayerId",
                table: "DraftPlayers",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_DraftPlayerTypes_PlayerTypeId",
                table: "DraftPlayerTypes",
                column: "PlayerTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Drafts_FantasyProviderId",
                table: "Drafts",
                column: "FantasyProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_Drafts_SeasonId",
                table: "Drafts",
                column: "SeasonId");

            migrationBuilder.CreateIndex(
                name: "IX_MLBHitterGames_GameId",
                table: "MLBHitterGames",
                column: "GameId");

            migrationBuilder.CreateIndex(
                name: "IX_MLBHitterGames_TeamId",
                table: "MLBHitterGames",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_MLBPitcherGames_GameId",
                table: "MLBPitcherGames",
                column: "GameId");

            migrationBuilder.CreateIndex(
                name: "IX_MLBPitcherGames_TeamId",
                table: "MLBPitcherGames",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_NFLDefenseGames_GameId",
                table: "NFLDefenseGames",
                column: "GameId");

            migrationBuilder.CreateIndex(
                name: "IX_NFLGames_GameId",
                table: "NFLGames",
                column: "GameId");

            migrationBuilder.CreateIndex(
                name: "IX_NFLKickerGames_GameId",
                table: "NFLKickerGames",
                column: "GameId");

            migrationBuilder.CreateIndex(
                name: "IX_NFLOffensiveGames_GameId",
                table: "NFLOffensiveGames",
                column: "GameId");

            migrationBuilder.CreateIndex(
                name: "IX_OwnershipPlayers_PlayerId",
                table: "OwnershipPlayers",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayedGamesMissed_PlayerId",
                table: "PlayedGamesMissed",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayedGamesMissed_TeamId",
                table: "PlayedGamesMissed",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerStatuses_PlayerId",
                table: "PlayerStatuses",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerStatuses_PlayerStatusTagTypeId",
                table: "PlayerStatuses",
                column: "PlayerStatusTagTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerStatuses_PlayerStatusTypeId",
                table: "PlayerStatuses",
                column: "PlayerStatusTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_PositionSourcePlayers_PositionId",
                table: "PositionSourcePlayers",
                column: "PositionId");

            migrationBuilder.CreateIndex(
                name: "IX_PositionSourcePositions_PositionId",
                table: "PositionSourcePositions",
                column: "PositionId");

            migrationBuilder.CreateIndex(
                name: "IX_PositionSources_FantasyProviderId",
                table: "PositionSources",
                column: "FantasyProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_UserDisplayCategories_CategoryId",
                table: "UserDisplayCategories",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_UserLeagueActiveRosterSpots_ActiveRosterSpotId",
                table: "UserLeagueActiveRosterSpots",
                column: "ActiveRosterSpotId");

            migrationBuilder.CreateIndex(
                name: "IX_UserLeagueCategories_CategoryId",
                table: "UserLeagueCategories",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_UserLeaguePlayerTypes_PlayerTypeId",
                table: "UserLeaguePlayerTypes",
                column: "PlayerTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_UserLeagues_FantasyProviderId",
                table: "UserLeagues",
                column: "FantasyProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_UserLeagues_SeasonId",
                table: "UserLeagues",
                column: "SeasonId");

            migrationBuilder.CreateIndex(
                name: "IX_UserLeagueTeamPlayers_PlayerId",
                table: "UserLeagueTeamPlayers",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_UserLeagueTeams_UserLeagueId",
                table: "UserLeagueTeams",
                column: "UserLeagueId");

            migrationBuilder.CreateIndex(
                name: "IX_UserLeagueWaiverPlayers_PlayerId",
                table: "UserLeagueWaiverPlayers",
                column: "PlayerId");

            migrationBuilder.AddForeignKey(
                name: "FK_FantasyProviderPlayers_FantasyProviders_FantasyProviderId",
                table: "FantasyProviderPlayers",
                column: "FantasyProviderId",
                principalTable: "FantasyProviders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FantasyProviderPlayers_Players_PlayerId",
                table: "FantasyProviderPlayers",
                column: "PlayerId",
                principalTable: "Players",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Games_Seasons_SeasonId",
                table: "Games",
                column: "SeasonId",
                principalTable: "Seasons",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PerValues_Categories_CategoryId",
                table: "PerValues",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SeasonPlayers_Teams_TeamId",
                table: "SeasonPlayers",
                column: "TeamId",
                principalTable: "Teams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FantasyProviderPlayers_FantasyProviders_FantasyProviderId",
                table: "FantasyProviderPlayers");

            migrationBuilder.DropForeignKey(
                name: "FK_FantasyProviderPlayers_Players_PlayerId",
                table: "FantasyProviderPlayers");

            migrationBuilder.DropForeignKey(
                name: "FK_Games_Seasons_SeasonId",
                table: "Games");

            migrationBuilder.DropForeignKey(
                name: "FK_PerValues_Categories_CategoryId",
                table: "PerValues");

            migrationBuilder.DropForeignKey(
                name: "FK_SeasonPlayers_Teams_TeamId",
                table: "SeasonPlayers");

            migrationBuilder.DropTable(
                name: "ActiveRosterSpotPositions");

            migrationBuilder.DropTable(
                name: "CategoryPerValues");

            migrationBuilder.DropTable(
                name: "DisplayCategories");

            migrationBuilder.DropTable(
                name: "DraftPlayers");

            migrationBuilder.DropTable(
                name: "DraftPlayerTypes");

            migrationBuilder.DropTable(
                name: "ExtraAnalysisLeagues");

            migrationBuilder.DropTable(
                name: "MLBHitterGames");

            migrationBuilder.DropTable(
                name: "MLBPitcherGames");

            migrationBuilder.DropTable(
                name: "NFLDefenseGames");

            migrationBuilder.DropTable(
                name: "NFLGames");

            migrationBuilder.DropTable(
                name: "NFLKickerGames");

            migrationBuilder.DropTable(
                name: "NFLOffensiveGames");

            migrationBuilder.DropTable(
                name: "OwnershipPlayers");

            migrationBuilder.DropTable(
                name: "PlayedGamesMissed");

            migrationBuilder.DropTable(
                name: "PlayerInjuries");

            migrationBuilder.DropTable(
                name: "PlayerStatuses");

            migrationBuilder.DropTable(
                name: "PositionSourcePlayers");

            migrationBuilder.DropTable(
                name: "PositionSourcePositions");

            migrationBuilder.DropTable(
                name: "Sports");

            migrationBuilder.DropTable(
                name: "TeamAliases");

            migrationBuilder.DropTable(
                name: "UserDisplayCategories");

            migrationBuilder.DropTable(
                name: "UserLeagueActiveRosterSpots");

            migrationBuilder.DropTable(
                name: "UserLeagueCategories");

            migrationBuilder.DropTable(
                name: "UserLeagueImportErrors");

            migrationBuilder.DropTable(
                name: "UserLeagueMissingPlayers");

            migrationBuilder.DropTable(
                name: "UserLeaguePlayerTypes");

            migrationBuilder.DropTable(
                name: "UserLeagueTeamPlayers");

            migrationBuilder.DropTable(
                name: "UserLeagueWaiverPlayers");

            migrationBuilder.DropTable(
                name: "Drafts");

            migrationBuilder.DropTable(
                name: "PlayerStatusTagTypes");

            migrationBuilder.DropTable(
                name: "PlayerStatusTypes");

            migrationBuilder.DropTable(
                name: "PositionSources");

            migrationBuilder.DropTable(
                name: "ActiveRosterSpots");

            migrationBuilder.DropTable(
                name: "UserLeagueTeams");

            migrationBuilder.DropTable(
                name: "UserLeagues");

            migrationBuilder.DropIndex(
                name: "IX_SeasonPlayers_TeamId",
                table: "SeasonPlayers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FantasyProviderPlayers",
                table: "FantasyProviderPlayers");

            migrationBuilder.DropColumn(
                name: "SportRadarId",
                table: "Teams");

            migrationBuilder.DropColumn(
                name: "Salary",
                table: "SeasonPlayers");

            migrationBuilder.DropColumn(
                name: "IsActualPosition",
                table: "Positions");

            migrationBuilder.DropColumn(
                name: "IsDefault",
                table: "PlayerTypes");

            migrationBuilder.DropColumn(
                name: "IsDisabled",
                table: "PlayerTypes");

            migrationBuilder.DropColumn(
                name: "Bats",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "Throws",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "ColumnTitle",
                table: "PerValues");

            migrationBuilder.DropColumn(
                name: "IsDefault",
                table: "PerValues");

            migrationBuilder.DropColumn(
                name: "SkillCategoryValue",
                table: "PerValues");

            migrationBuilder.DropColumn(
                name: "DisplayOrder",
                table: "FantasyProviders");

            migrationBuilder.DropColumn(
                name: "IsDefault",
                table: "FantasyProviders");

            migrationBuilder.DropColumn(
                name: "LeagueURL",
                table: "FantasyProviders");

            migrationBuilder.DropColumn(
                name: "DisplayOrder",
                table: "Divisions");

            migrationBuilder.DropColumn(
                name: "CBSId",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "DefaultPointsPerStat",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "ESPNId",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "FanTraxGroup",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "FanTraxId",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "IsDefaultDisplayCategory",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "IsDisplayCategory",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "OtherAbbreviations",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "SourceField",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "YahooId",
                table: "Categories");

            migrationBuilder.AlterColumn<int>(
                name: "CategoryId",
                table: "PerValues",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "SeasonId",
                table: "Games",
                type: "int",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AlterColumn<bool>(
                name: "IsFinished",
                table: "Games",
                type: "bit",
                nullable: true,
                oldClrType: typeof(bool));

            migrationBuilder.AlterColumn<int>(
                name: "PlayerId",
                table: "FantasyProviderPlayers",
                type: "int",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AlterColumn<int>(
                name: "FantasyProviderId",
                table: "FantasyProviderPlayers",
                type: "int",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "FantasyProviderPlayers",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<bool>(
                name: "IsCalculated",
                table: "Categories",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsStat",
                table: "Categories",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PerDisplayFormat",
                table: "Categories",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Property",
                table: "Categories",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TotalDisplayFormat",
                table: "Categories",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_FantasyProviderPlayers",
                table: "FantasyProviderPlayers",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "PlayerGames",
                columns: table => new
                {
                    GameId = table.Column<int>(type: "int", nullable: false),
                    PlayerId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: true),
                    NotPlayingDescription = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    NotPlayingReason = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Played = table.Column<bool>(type: "bit", nullable: true),
                    Started = table.Column<bool>(type: "bit", nullable: true),
                    TeamId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerGames", x => new { x.GameId, x.PlayerId });
                    table.ForeignKey(
                        name: "FK_PlayerGames_Games_GameId",
                        column: x => x.GameId,
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlayerGames_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlayerGames_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FantasyProviderPlayers_FantasyProviderId",
                table: "FantasyProviderPlayers",
                column: "FantasyProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerGames_PlayerId",
                table: "PlayerGames",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerGames_TeamId",
                table: "PlayerGames",
                column: "TeamId");

            migrationBuilder.AddForeignKey(
                name: "FK_FantasyProviderPlayers_FantasyProviders_FantasyProviderId",
                table: "FantasyProviderPlayers",
                column: "FantasyProviderId",
                principalTable: "FantasyProviders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FantasyProviderPlayers_Players_PlayerId",
                table: "FantasyProviderPlayers",
                column: "PlayerId",
                principalTable: "Players",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Games_Seasons_SeasonId",
                table: "Games",
                column: "SeasonId",
                principalTable: "Seasons",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PerValues_Categories_CategoryId",
                table: "PerValues",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
