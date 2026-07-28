using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace RotoMonster.MLB.Migrations.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Divisions",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Divisions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FantasyProviders",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(nullable: true),
                    Url = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FantasyProviders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Players",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirstName = table.Column<string>(maxLength: 80, nullable: false),
                    LastName = table.Column<string>(maxLength: 80, nullable: false),
                    Birthdate = table.Column<DateTime>(nullable: false),
                    Height = table.Column<int>(nullable: false),
                    Weight = table.Column<int>(nullable: false),
                    RookieYear = table.Column<int>(nullable: true),
                    PickNumber = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Players", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PlayerTypes",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(nullable: true),
                    DisplayOrder = table.Column<int>(nullable: false),
                    SingularTitle = table.Column<string>(nullable: true),
                    PluralTitle = table.Column<string>(nullable: true),
                    DefaultPerTeam = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Seasons",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Year = table.Column<int>(nullable: true),
                    Title = table.Column<string>(nullable: true),
                    Abbreviation = table.Column<string>(nullable: true),
                    StartDate = table.Column<DateTime>(nullable: false),
                    EndDate = table.Column<DateTime>(nullable: false),
                    IsRegularSeason = table.Column<bool>(nullable: true),
                    YahooId = table.Column<string>(nullable: true),
                    IsEnabled = table.Column<bool>(nullable: false),
                    DisplayOrder = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Seasons", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Teams",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Teams", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FantasyProviderPlayers",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FantasyProviderId = table.Column<int>(nullable: true),
                    PlayerId = table.Column<int>(nullable: true),
                    ProviderId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FantasyProviderPlayers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FantasyProviderPlayers_FantasyProviders_FantasyProviderId",
                        column: x => x.FantasyProviderId,
                        principalTable: "FantasyProviders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FantasyProviderPlayers_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PlayerAliases",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Alias = table.Column<string>(nullable: true),
                    PlayerId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerAliases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlayerAliases_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlayerTypeId = table.Column<int>(nullable: true),
                    Title = table.Column<string>(nullable: true),
                    Abbreviation = table.Column<string>(nullable: true),
                    Property = table.Column<string>(nullable: true),
                    IsPositive = table.Column<bool>(nullable: true),
                    IsDefault = table.Column<bool>(nullable: true),
                    UseAsValue = table.Column<bool>(nullable: true),
                    TotalDisplayFormat = table.Column<string>(nullable: true),
                    PerDisplayFormat = table.Column<string>(nullable: true),
                    ExcludeFromEase = table.Column<bool>(nullable: true),
                    IsDisabled = table.Column<bool>(nullable: true),
                    IsStat = table.Column<bool>(nullable: false),
                    IsCalculated = table.Column<bool>(nullable: false),
                    DisplayOrder = table.Column<int>(nullable: false),
                    WeightCategoryId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Categories_PlayerTypes_PlayerTypeId",
                        column: x => x.PlayerTypeId,
                        principalTable: "PlayerTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Categories_Categories_WeightCategoryId",
                        column: x => x.WeightCategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Positions",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlayerTypeId = table.Column<int>(nullable: true),
                    Title = table.Column<string>(nullable: true),
                    Abbreviation = table.Column<string>(nullable: true),
                    IsDefault = table.Column<bool>(nullable: false),
                    ColorCode = table.Column<string>(nullable: true),
                    DisplayOrder = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Positions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Positions_PlayerTypes_PlayerTypeId",
                        column: x => x.PlayerTypeId,
                        principalTable: "PlayerTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SeasonDivisions",
                columns: table => new
                {
                    DivisionId = table.Column<int>(nullable: false),
                    SeasonId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SeasonDivisions", x => new { x.SeasonId, x.DivisionId });
                    table.ForeignKey(
                        name: "FK_SeasonDivisions_Divisions_DivisionId",
                        column: x => x.DivisionId,
                        principalTable: "Divisions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SeasonDivisions_Seasons_SeasonId",
                        column: x => x.SeasonId,
                        principalTable: "Seasons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SeasonPlayers",
                columns: table => new
                {
                    PlayerId = table.Column<int>(nullable: false),
                    SeasonId = table.Column<int>(nullable: false),
                    PlayerTypeId = table.Column<int>(nullable: false),
                    TeamId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SeasonPlayers", x => new { x.SeasonId, x.PlayerId, x.PlayerTypeId });
                    table.ForeignKey(
                        name: "FK_SeasonPlayers_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SeasonPlayers_PlayerTypes_PlayerTypeId",
                        column: x => x.PlayerTypeId,
                        principalTable: "PlayerTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SeasonPlayers_Seasons_SeasonId",
                        column: x => x.SeasonId,
                        principalTable: "Seasons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Games",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SeasonId = table.Column<int>(nullable: true),
                    Number = table.Column<int>(nullable: false),
                    HomeTeamId = table.Column<int>(nullable: true),
                    AwayTeamId = table.Column<int>(nullable: true),
                    GameDate = table.Column<DateTime>(nullable: false),
                    GameTime = table.Column<DateTime>(nullable: false),
                    IsFinished = table.Column<bool>(nullable: true),
                    SportRadarId = table.Column<string>(maxLength: 80, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Games", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Games_Teams_AwayTeamId",
                        column: x => x.AwayTeamId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Games_Teams_HomeTeamId",
                        column: x => x.HomeTeamId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Games_Seasons_SeasonId",
                        column: x => x.SeasonId,
                        principalTable: "Seasons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SeasonTeams",
                columns: table => new
                {
                    TeamId = table.Column<int>(nullable: false),
                    SeasonId = table.Column<int>(nullable: false),
                    DivisionId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SeasonTeams", x => new { x.SeasonId, x.TeamId });
                    table.ForeignKey(
                        name: "FK_SeasonTeams_Divisions_DivisionId",
                        column: x => x.DivisionId,
                        principalTable: "Divisions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SeasonTeams_Seasons_SeasonId",
                        column: x => x.SeasonId,
                        principalTable: "Seasons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SeasonTeams_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PerValues",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(nullable: true),
                    PlayerTypeId = table.Column<int>(nullable: false),
                    CategoryId = table.Column<int>(nullable: false),
                    DisplayOrder = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PerValues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PerValues_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PerValues_PlayerTypes_PlayerTypeId",
                        column: x => x.PlayerTypeId,
                        principalTable: "PlayerTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlayerDefaultPositions",
                columns: table => new
                {
                    PlayerId = table.Column<int>(nullable: false),
                    PositionId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerDefaultPositions", x => new { x.PlayerId, x.PositionId });
                    table.ForeignKey(
                        name: "FK_PlayerDefaultPositions_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlayerDefaultPositions_Positions_PositionId",
                        column: x => x.PositionId,
                        principalTable: "Positions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NBAPlayerGames",
                columns: table => new
                {
                    PlayerId = table.Column<int>(nullable: false),
                    GameId = table.Column<int>(nullable: false),
                    Minutes = table.Column<double>(nullable: false),
                    FieldGoals = table.Column<byte>(nullable: false),
                    FieldGoalsAttempted = table.Column<byte>(nullable: false),
                    Threes = table.Column<byte>(nullable: false),
                    ThreesAttempted = table.Column<byte>(nullable: false),
                    FreeThrows = table.Column<byte>(nullable: false),
                    FreeThrowsAttempted = table.Column<byte>(nullable: false),
                    OffensiveRebounds = table.Column<byte>(nullable: false),
                    DefensiveRebounds = table.Column<byte>(nullable: false),
                    Assists = table.Column<byte>(nullable: false),
                    Steals = table.Column<byte>(nullable: false),
                    Blocks = table.Column<byte>(nullable: false),
                    Turnovers = table.Column<byte>(nullable: false),
                    Points = table.Column<byte>(nullable: false),
                    Fouls = table.Column<byte>(nullable: false),
                    Started = table.Column<byte>(nullable: false),
                    DoubleDoubles = table.Column<byte>(nullable: false),
                    TripleDoubles = table.Column<byte>(nullable: false),
                    Technicals = table.Column<byte>(nullable: false),
                    PlusMinus = table.Column<double>(nullable: false),
                    Usage = table.Column<double>(nullable: true),
                    Wins = table.Column<byte>(nullable: false),
                    FoulTrouble = table.Column<bool>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NBAPlayerGames", x => new { x.PlayerId, x.GameId });
                    table.ForeignKey(
                        name: "FK_NBAPlayerGames_Games_GameId",
                        column: x => x.GameId,
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NBAPlayerGames_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlayerGames",
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
                name: "IX_Categories_PlayerTypeId",
                table: "Categories",
                column: "PlayerTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_WeightCategoryId",
                table: "Categories",
                column: "WeightCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_FantasyProviderPlayers_FantasyProviderId",
                table: "FantasyProviderPlayers",
                column: "FantasyProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_FantasyProviderPlayers_PlayerId",
                table: "FantasyProviderPlayers",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_Games_AwayTeamId",
                table: "Games",
                column: "AwayTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_Games_HomeTeamId",
                table: "Games",
                column: "HomeTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_Games_SeasonId",
                table: "Games",
                column: "SeasonId");

            migrationBuilder.CreateIndex(
                name: "IX_NBAPlayerGames_GameId",
                table: "NBAPlayerGames",
                column: "GameId");

            migrationBuilder.CreateIndex(
                name: "IX_PerValues_CategoryId",
                table: "PerValues",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_PerValues_PlayerTypeId",
                table: "PerValues",
                column: "PlayerTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerAliases_PlayerId",
                table: "PlayerAliases",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerDefaultPositions_PositionId",
                table: "PlayerDefaultPositions",
                column: "PositionId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerGames_PlayerId",
                table: "PlayerGames",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerGames_TeamId",
                table: "PlayerGames",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_Positions_PlayerTypeId",
                table: "Positions",
                column: "PlayerTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_SeasonDivisions_DivisionId",
                table: "SeasonDivisions",
                column: "DivisionId");

            migrationBuilder.CreateIndex(
                name: "IX_SeasonPlayers_PlayerId",
                table: "SeasonPlayers",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_SeasonPlayers_PlayerTypeId",
                table: "SeasonPlayers",
                column: "PlayerTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_SeasonTeams_DivisionId",
                table: "SeasonTeams",
                column: "DivisionId");

            migrationBuilder.CreateIndex(
                name: "IX_SeasonTeams_TeamId",
                table: "SeasonTeams",
                column: "TeamId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FantasyProviderPlayers");

            migrationBuilder.DropTable(
                name: "NBAPlayerGames");

            migrationBuilder.DropTable(
                name: "PerValues");

            migrationBuilder.DropTable(
                name: "PlayerAliases");

            migrationBuilder.DropTable(
                name: "PlayerDefaultPositions");

            migrationBuilder.DropTable(
                name: "PlayerGames");

            migrationBuilder.DropTable(
                name: "SeasonDivisions");

            migrationBuilder.DropTable(
                name: "SeasonPlayers");

            migrationBuilder.DropTable(
                name: "SeasonTeams");

            migrationBuilder.DropTable(
                name: "FantasyProviders");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "Positions");

            migrationBuilder.DropTable(
                name: "Games");

            migrationBuilder.DropTable(
                name: "Players");

            migrationBuilder.DropTable(
                name: "Divisions");

            migrationBuilder.DropTable(
                name: "PlayerTypes");

            migrationBuilder.DropTable(
                name: "Teams");

            migrationBuilder.DropTable(
                name: "Seasons");
        }
    }
}
