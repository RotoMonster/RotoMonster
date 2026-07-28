using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace RotoMonster.MLB.Migrations.Migrations
{
    public partial class _0619_1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PerValues_Categories_CategoryId",
                table: "PerValues");

            migrationBuilder.DropTable(
                name: "PlayerGames");

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
                name: "DefaultScoringSystem",
                table: "Sports",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StartDayOfWeek",
                table: "Sports",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Salary",
                table: "SeasonPlayers",
                nullable: true);

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

            migrationBuilder.AddColumn<double>(
                name: "SkillCategoryValue",
                table: "PerValues",
                nullable: true);

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
                name: "FanTraxId",
                table: "Categories",
                nullable: true);

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
                    LeagueSize = table.Column<int>(nullable: false),
                    LeagueCount = table.Column<int>(nullable: false),
                    OwnCount = table.Column<int>(nullable: false),
                    ActiveCount = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OwnershipPlayers", x => new { x.GameDate, x.PlayerId });
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
                    DisplayOrder = table.Column<int>(nullable: false),
                    LastSelectedDate = table.Column<DateTime>(nullable: true)
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
                name: "IX_NBAPlayerGames_GameId",
                table: "NBAPlayerGames",
                column: "GameId");

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
                name: "IX_Drafts_FantasyProviderId",
                table: "Drafts",
                column: "FantasyProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_Drafts_SeasonId",
                table: "Drafts",
                column: "SeasonId");

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

            migrationBuilder.AddForeignKey(
                name: "FK_NBAPlayerGames_Games_GameId",
                table: "NBAPlayerGames",
                column: "GameId",
                principalTable: "Games",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_NBAPlayerGames_Players_PlayerId",
                table: "NBAPlayerGames",
                column: "PlayerId",
                principalTable: "Players",
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
                name: "FK_NBAPlayerGames_Games_GameId",
                table: "NBAPlayerGames");

            migrationBuilder.DropForeignKey(
                name: "FK_NBAPlayerGames_Players_PlayerId",
                table: "NBAPlayerGames");

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
                name: "NFLKickerGames");

            migrationBuilder.DropTable(
                name: "NFLOffensiveGames");

            migrationBuilder.DropTable(
                name: "OwnershipPlayers");

            migrationBuilder.DropTable(
                name: "PlayedGamesMissed");

            migrationBuilder.DropTable(
                name: "PositionSourcePlayers");

            migrationBuilder.DropTable(
                name: "PositionSourcePositions");

            migrationBuilder.DropTable(
                name: "UserDisplayCategories");

            migrationBuilder.DropTable(
                name: "UserLeagueActiveRosterSpots");

            migrationBuilder.DropTable(
                name: "UserLeagueCategories");

            migrationBuilder.DropTable(
                name: "UserLeagueTeamPlayers");

            migrationBuilder.DropTable(
                name: "Drafts");

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

            migrationBuilder.DropIndex(
                name: "IX_NBAPlayerGames_GameId",
                table: "NBAPlayerGames");

            migrationBuilder.DropColumn(
                name: "DefaultScoringSystem",
                table: "Sports");

            migrationBuilder.DropColumn(
                name: "StartDayOfWeek",
                table: "Sports");

            migrationBuilder.DropColumn(
                name: "Salary",
                table: "SeasonPlayers");

            migrationBuilder.DropColumn(
                name: "IsDefault",
                table: "PlayerTypes");

            migrationBuilder.DropColumn(
                name: "IsDisabled",
                table: "PlayerTypes");

            migrationBuilder.DropColumn(
                name: "ColumnTitle",
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
                name: "CBSId",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "DefaultPointsPerStat",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "ESPNId",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "FanTraxId",
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
                name: "IX_PlayerGames_PlayerId",
                table: "PlayerGames",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerGames_TeamId",
                table: "PlayerGames",
                column: "TeamId");

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
