using Microsoft.EntityFrameworkCore.Migrations;

namespace RotoMonster.NFL.Migrations.Migrations
{
    public partial class _090820_1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NFLGames",
                columns: table => new
                {
                    GameId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OverUnder = table.Column<double>(nullable: false),
                    HomeSpread = table.Column<double>(nullable: false),
                    GameId1 = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NFLGames", x => x.GameId);
                    table.ForeignKey(
                        name: "FK_NFLGames_Games_GameId1",
                        column: x => x.GameId1,
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NFLGames_GameId1",
                table: "NFLGames",
                column: "GameId1");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NFLGames");
        }
    }
}
