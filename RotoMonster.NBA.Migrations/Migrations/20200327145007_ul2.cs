using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace RotoMonster.Data.Migrations
{
    public partial class ul2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserLeagues",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(maxLength: 450, nullable: false),
                    Title = table.Column<string>(maxLength: 250, nullable: false),
                    FantasyProviderId = table.Column<int>(nullable: false),
                    ProviderLeagueId = table.Column<string>(maxLength: 100, nullable: false),
                    TrackLeague = table.Column<bool>(nullable: false),
                    MyTeamTitle = table.Column<string>(maxLength: 250, nullable: false),
                    ScoringSystem = table.Column<string>(maxLength: 1, nullable: false),
                    LeagueType = table.Column<string>(maxLength: 1, nullable: false),
                    LineupFrequency = table.Column<string>(maxLength: 1, nullable: false),
                    NumberOfTeams = table.Column<int>(nullable: false),
                    BenchSize = table.Column<int>(nullable: true),
                    IRSpots = table.Column<int>(nullable: true),
                    StartWeekday = table.Column<int>(nullable: false),
                    QualityGamesLimit = table.Column<int>(nullable: true),
                    SameDayTransactions = table.Column<bool>(nullable: false),
                    IsMoney = table.Column<bool>(nullable: true),
                    DraftDate = table.Column<DateTime>(nullable: true),
                    AutoEndDate = table.Column<bool>(nullable: true),
                    GameLimit = table.Column<int>(nullable: true),
                    ColorStats = table.Column<bool>(nullable: true),
                    AutoUpdate = table.Column<bool>(nullable: true),
                    ContinuousWaivers = table.Column<bool>(nullable: true),
                    DisplayOrder = table.Column<int>(nullable: false)
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

            migrationBuilder.CreateIndex(
                name: "IX_UserLeagues_FantasyProviderId",
                table: "UserLeagues",
                column: "FantasyProviderId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserLeagues");
        }
    }
}
