using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace RotoMonster.Data.Migrations
{
    public partial class own1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.CreateIndex(
                name: "IX_OwnershipPlayers_PlayerId",
                table: "OwnershipPlayers",
                column: "PlayerId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OwnershipPlayers");
        }
    }
}
