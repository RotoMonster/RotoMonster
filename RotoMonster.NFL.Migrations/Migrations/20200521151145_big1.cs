using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace RotoMonster.NFL.Migrations.Migrations
{
    public partial class big1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.DropTable(
                name: "PlayerGames");

            migrationBuilder.DropIndex(
                name: "IX_NBAPlayerGames_GameId",
                table: "NBAPlayerGames");

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

            migrationBuilder.AddColumn<int>(
                name: "Salary",
                table: "SeasonPlayers",
                nullable: true);

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

            migrationBuilder.AddColumn<int>(
                name: "DisplayOrder",
                table: "Divisions",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "CBSId",
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
                    DefaultNumberOf = table.Column<int>(nullable: false),
                    DisplayOrder = table.Column<int>(nullable: false)
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
                    FantasyProviderId = table.Column<int>(nullable: false),
                    DateAdded = table.Column<DateTime>(nullable: false),
                    DraftDate = table.Column<DateTime>(nullable: false),
                    Title = table.Column<string>(nullable: true),
                    ProviderLeagueId = table.Column<string>(nullable: true),
                    LeagueSize = table.Column<int>(nullable: false),
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
                });

            migrationBuilder.CreateTable(
                name: "MLBHitterGames",
                columns: table => new
                {
                    PlayerId = table.Column<int>(nullable: false),
                    GameId = table.Column<int>(nullable: false),
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
                });

            migrationBuilder.CreateTable(
                name: "MLBPitcherGames",
                columns: table => new
                {
                    PlayerId = table.Column<int>(nullable: false),
                    GameId = table.Column<int>(nullable: false),
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
                name: "PositionSourcePositions",
                columns: table => new
                {
                    PositionSourceId = table.Column<int>(nullable: false),
                    PositionId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PositionSourcePositions", x => new { x.PositionSourceId, x.PositionId });
                });

            migrationBuilder.CreateTable(
                name: "PositionSources",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProviderId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PositionSources", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Sports",
                columns: table => new
                {
                    Title = table.Column<string>(maxLength: 3, nullable: true),
                    DivisionTitle = table.Column<string>(maxLength: 20, nullable: true),
                    UsesCategories = table.Column<bool>(nullable: false),
                    UsesPointsPerStat = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "UserLeagues",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(maxLength: 450, nullable: false),
                    Title = table.Column<string>(maxLength: 250, nullable: false),
                    DisplayTitle = table.Column<string>(maxLength: 250, nullable: false),
                    FantasyProviderId = table.Column<int>(nullable: false),
                    ProviderLeagueId = table.Column<string>(maxLength: 100, nullable: false),
                    MyProviderTeamId = table.Column<string>(maxLength: 100, nullable: false),
                    TrackLeague = table.Column<bool>(nullable: false),
                    MyTeamTitle = table.Column<string>(maxLength: 250, nullable: false),
                    ScoringSystem = table.Column<string>(maxLength: 10, nullable: false),
                    LeagueType = table.Column<string>(maxLength: 10, nullable: false),
                    LineupFrequency = table.Column<string>(maxLength: 10, nullable: false),
                    NumberOfTeams = table.Column<int>(nullable: false),
                    PlayersPerTeam = table.Column<int>(nullable: false),
                    IRSpots = table.Column<int>(nullable: false),
                    StartWeekday = table.Column<int>(nullable: false),
                    QualityGamesLimit = table.Column<int>(nullable: false),
                    SameDayTransactions = table.Column<bool>(nullable: false),
                    IsMoney = table.Column<bool>(nullable: false),
                    DraftDate = table.Column<DateTime>(nullable: true),
                    AutoEndDate = table.Column<bool>(nullable: false),
                    GameLimit = table.Column<int>(nullable: false),
                    AutoUpdate = table.Column<bool>(nullable: false),
                    ContinuousWaivers = table.Column<bool>(nullable: false),
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
                name: "IX_MLBHitterGames_GameId",
                table: "MLBHitterGames",
                column: "GameId");

            migrationBuilder.CreateIndex(
                name: "IX_MLBPitcherGames_GameId",
                table: "MLBPitcherGames",
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
                name: "IX_UserLeagueTeamPlayers_PlayerId",
                table: "UserLeagueTeamPlayers",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_UserLeagueTeams_UserLeagueId",
                table: "UserLeagueTeams",
                column: "UserLeagueId");

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
                name: "MLBHitterGames");

            migrationBuilder.DropTable(
                name: "MLBPitcherGames");

            migrationBuilder.DropTable(
                name: "OwnershipPlayers");

            migrationBuilder.DropTable(
                name: "PlayedGamesMissed");

            migrationBuilder.DropTable(
                name: "PositionSourcePlayers");

            migrationBuilder.DropTable(
                name: "PositionSourcePositions");

            migrationBuilder.DropTable(
                name: "PositionSources");

            migrationBuilder.DropTable(
                name: "Sports");

            migrationBuilder.DropTable(
                name: "UserLeagueActiveRosterSpots");

            migrationBuilder.DropTable(
                name: "UserLeagueCategories");

            migrationBuilder.DropTable(
                name: "UserLeagueTeamPlayers");

            migrationBuilder.DropTable(
                name: "Drafts");

            migrationBuilder.DropTable(
                name: "ActiveRosterSpots");

            migrationBuilder.DropTable(
                name: "UserLeagueTeams");

            migrationBuilder.DropTable(
                name: "UserLeagues");

            migrationBuilder.DropIndex(
                name: "IX_SeasonPlayers_TeamId",
                table: "SeasonPlayers");

            migrationBuilder.DropColumn(
                name: "Salary",
                table: "SeasonPlayers");

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
                name: "DisplayOrder",
                table: "Divisions");

            migrationBuilder.DropColumn(
                name: "CBSId",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "ESPNId",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "FanTraxId",
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
                name: "IX_NBAPlayerGames_GameId",
                table: "NBAPlayerGames",
                column: "GameId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerGames_PlayerId",
                table: "PlayerGames",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerGames_TeamId",
                table: "PlayerGames",
                column: "TeamId");

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
                onDelete: ReferentialAction.Cascade);
        }
    }
}
