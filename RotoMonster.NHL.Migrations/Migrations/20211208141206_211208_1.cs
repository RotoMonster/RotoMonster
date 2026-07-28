using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RotoMonster.NHL.Migrations.Migrations
{
    public partial class _211208_1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PlayerGamePositionCategories",
                columns: table => new
                {
                    GameId = table.Column<int>(type: "int", nullable: false),
                    PlayerId = table.Column<int>(type: "int", nullable: false),
                    TeamId = table.Column<int>(type: "int", nullable: false),
                    PositionId = table.Column<int>(type: "int", nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    Percent = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerGamePositionCategories", x => new { x.PlayerId, x.GameId, x.TeamId, x.PositionId, x.CategoryId });
                    table.ForeignKey(
                        name: "FK_PlayerGamePositionCategories_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlayerGamePositionCategories_Games_GameId",
                        column: x => x.GameId,
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlayerGamePositionCategories_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlayerGamePositionCategories_Positions_PositionId",
                        column: x => x.PositionId,
                        principalTable: "Positions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlayerGamePositionCategories_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlayerGamePositionCategories_CategoryId",
                table: "PlayerGamePositionCategories",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerGamePositionCategories_GameId",
                table: "PlayerGamePositionCategories",
                column: "GameId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerGamePositionCategories_PositionId",
                table: "PlayerGamePositionCategories",
                column: "PositionId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerGamePositionCategories_TeamId",
                table: "PlayerGamePositionCategories",
                column: "TeamId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlayerGamePositionCategories");
        }
    }
}
