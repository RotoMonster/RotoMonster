using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace RotoMonster.NFL.Migrations.Migrations
{
    public partial class _090120_3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ExtraAnalysisLeagues",
                columns: table => new
                {
                    FantasyProviderId = table.Column<int>(nullable: false),
                    ProviderId = table.Column<string>(nullable: false),
                    Title = table.Column<string>(nullable: true),
                    EntryFee = table.Column<int>(nullable: true),
                    NumberOfTeams = table.Column<int>(nullable: true),
                    DraftDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExtraAnalysisLeagues", x => new { x.FantasyProviderId, x.ProviderId });
                    table.ForeignKey(
                        name: "FK_ExtraAnalysisLeagues_FantasyProviders_FantasyProviderId",
                        column: x => x.FantasyProviderId,
                        principalTable: "FantasyProviders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExtraAnalysisLeagues");
        }
    }
}
