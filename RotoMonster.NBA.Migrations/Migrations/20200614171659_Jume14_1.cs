using Microsoft.EntityFrameworkCore.Migrations;

namespace RotoMonster.Data.Migrations
{
    public partial class Jume14_1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                name: "FanTraxTitle",
                table: "ActiveRosterSpots",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "YahooTitle",
                table: "ActiveRosterSpots",
                nullable: true);

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

            migrationBuilder.CreateIndex(
                name: "IX_NFLOffensiveGames_GameId",
                table: "NFLOffensiveGames",
                column: "GameId");

            migrationBuilder.CreateIndex(
                name: "IX_NBAPlayerGames_GameId",
                table: "NBAPlayerGames",
                column: "GameId");

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
                name: "FK_NFLOffensiveGames_Games_GameId",
                table: "NFLOffensiveGames",
                column: "GameId",
                principalTable: "Games",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_NFLOffensiveGames_Players_PlayerId",
                table: "NFLOffensiveGames",
                column: "PlayerId",
                principalTable: "Players",
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
                name: "FK_NFLOffensiveGames_Games_GameId",
                table: "NFLOffensiveGames");

            migrationBuilder.DropForeignKey(
                name: "FK_NFLOffensiveGames_Players_PlayerId",
                table: "NFLOffensiveGames");

            migrationBuilder.DropTable(
                name: "NFLKickerGames");

            migrationBuilder.DropIndex(
                name: "IX_NFLOffensiveGames_GameId",
                table: "NFLOffensiveGames");

            migrationBuilder.DropIndex(
                name: "IX_NBAPlayerGames_GameId",
                table: "NBAPlayerGames");

            migrationBuilder.DropColumn(
                name: "IsDefault",
                table: "PlayerTypes");

            migrationBuilder.DropColumn(
                name: "IsDisabled",
                table: "PlayerTypes");

            migrationBuilder.DropColumn(
                name: "FanTraxTitle",
                table: "ActiveRosterSpots");

            migrationBuilder.DropColumn(
                name: "YahooTitle",
                table: "ActiveRosterSpots");
        }
    }
}
