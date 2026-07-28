using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace RotoMonster.Data.Migrations
{
    public partial class d2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Drafts",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FantasyProviderId = table.Column<int>(nullable: false),
                    DraftDate = table.Column<DateTime>(nullable: false),
                    Title = table.Column<string>(nullable: true),
                    ProviderLeagueId = table.Column<string>(nullable: true),
                    LeagueSize = table.Column<int>(nullable: false),
                    IsAuction = table.Column<bool>(nullable: false),
                    IsMoney = table.Column<bool>(nullable: false),
                    LeagueType = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Drafts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Drafts_FantasyProviders_FantasyProviderId",
                        column: x => x.FantasyProviderId,
                        principalTable: "FantasyProviders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DraftPlayers",
                columns: table => new
                {
                    DraftId = table.Column<int>(nullable: false),
                    PlayerId = table.Column<int>(nullable: false),
                    DraftOrder = table.Column<int>(nullable: false),
                    Price = table.Column<int>(nullable: true),
                    ProviderTeamId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DraftPlayers", x => new { x.DraftId, x.PlayerId });
                    table.ForeignKey(
                        name: "FK_DraftPlayers_Drafts_DraftId",
                        column: x => x.DraftId,
                        principalTable: "Drafts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DraftPlayers_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DraftPlayers_PlayerId",
                table: "DraftPlayers",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_Drafts_FantasyProviderId",
                table: "Drafts",
                column: "FantasyProviderId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DraftPlayers");

            migrationBuilder.DropTable(
                name: "Drafts");
        }
    }
}
