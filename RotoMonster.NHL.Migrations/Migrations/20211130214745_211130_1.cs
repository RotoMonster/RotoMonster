using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RotoMonster.NHL.Migrations.Migrations
{
    public partial class _211130_1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsScoringAlertCategory",
                table: "Categories",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "GameScoringAlerts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GameId = table.Column<int>(type: "int", nullable: false),
                    PlayerId = table.Column<int>(type: "int", nullable: false),
                    TeamId = table.Column<int>(type: "int", nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    ScoringDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameScoringAlerts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GameScoringAlerts_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GameScoringAlerts_Games_GameId",
                        column: x => x.GameId,
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GameScoringAlerts_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GameScoringAlerts_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GameScoringAlerts_CategoryId",
                table: "GameScoringAlerts",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_GameScoringAlerts_GameId",
                table: "GameScoringAlerts",
                column: "GameId");

            migrationBuilder.CreateIndex(
                name: "IX_GameScoringAlerts_PlayerId",
                table: "GameScoringAlerts",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_GameScoringAlerts_TeamId",
                table: "GameScoringAlerts",
                column: "TeamId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GameScoringAlerts");

            migrationBuilder.DropColumn(
                name: "IsScoringAlertCategory",
                table: "Categories");
        }
    }
}
