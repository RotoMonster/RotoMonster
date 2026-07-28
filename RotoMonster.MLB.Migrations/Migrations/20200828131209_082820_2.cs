using Microsoft.EntityFrameworkCore.Migrations;

namespace RotoMonster.MLB.Migrations.Migrations
{
    public partial class _082820_2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserLeaguePlayerTypes",
                columns: table => new
                {
                    UserLeagueId = table.Column<int>(nullable: false),
                    PlayerTypeId = table.Column<int>(nullable: false),
                    CategoriesCode = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserLeaguePlayerTypes", x => new { x.UserLeagueId, x.PlayerTypeId });
                    table.ForeignKey(
                        name: "FK_UserLeaguePlayerTypes_PlayerTypes_PlayerTypeId",
                        column: x => x.PlayerTypeId,
                        principalTable: "PlayerTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserLeaguePlayerTypes_UserLeagues_UserLeagueId",
                        column: x => x.UserLeagueId,
                        principalTable: "UserLeagues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserLeaguePlayerTypes_PlayerTypeId",
                table: "UserLeaguePlayerTypes",
                column: "PlayerTypeId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserLeaguePlayerTypes");
        }
    }
}
