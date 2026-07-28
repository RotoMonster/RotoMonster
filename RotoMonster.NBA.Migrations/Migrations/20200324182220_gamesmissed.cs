using Microsoft.EntityFrameworkCore.Migrations;

namespace RotoMonster.Data.Migrations
{
    public partial class gamesmissed : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlayerGames");

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

            migrationBuilder.CreateIndex(
                name: "IX_PlayedGamesMissed_PlayerId",
                table: "PlayedGamesMissed",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayedGamesMissed_TeamId",
                table: "PlayedGamesMissed",
                column: "TeamId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlayedGamesMissed");

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
        }
    }
}
