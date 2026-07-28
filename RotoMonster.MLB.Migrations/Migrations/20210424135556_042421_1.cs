using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace RotoMonster.MLB.Migrations.Migrations
{
    public partial class _042421_1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PlayerGameStateTypes",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(nullable: true),
                    Descrition = table.Column<string>(nullable: true),
                    Icon = table.Column<string>(nullable: true),
                    TextColor = table.Column<string>(nullable: true),
                    BackgroundColor = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerGameStateTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PlayerGameStates",
                columns: table => new
                {
                    GameId = table.Column<int>(nullable: false),
                    PlayerId = table.Column<int>(nullable: false),
                    TeamId = table.Column<int>(nullable: false),
                    PositionId = table.Column<int>(nullable: true),
                    PlayerGameStateTypeId = table.Column<int>(nullable: false),
                    DateAdded = table.Column<DateTime>(nullable: false),
                    Details = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerGameStates", x => new { x.GameId, x.PlayerId });
                    table.ForeignKey(
                        name: "FK_PlayerGameStates_Games_GameId",
                        column: x => x.GameId,
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlayerGameStates_PlayerGameStateTypes_PlayerGameStateTypeId",
                        column: x => x.PlayerGameStateTypeId,
                        principalTable: "PlayerGameStateTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlayerGameStates_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlayerGameStates_Positions_PositionId",
                        column: x => x.PositionId,
                        principalTable: "Positions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PlayerGameStates_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlayerGameStates_PlayerGameStateTypeId",
                table: "PlayerGameStates",
                column: "PlayerGameStateTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerGameStates_PlayerId",
                table: "PlayerGameStates",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerGameStates_PositionId",
                table: "PlayerGameStates",
                column: "PositionId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerGameStates_TeamId",
                table: "PlayerGameStates",
                column: "TeamId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlayerGameStates");

            migrationBuilder.DropTable(
                name: "PlayerGameStateTypes");
        }
    }
}
