using Microsoft.EntityFrameworkCore.Migrations;

namespace RotoMonster.Data.Migrations
{
    public partial class ars1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ActiveRosterSpot",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(nullable: true),
                    DefaultNumberOf = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActiveRosterSpot", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ActiveRosterSpotPositions",
                columns: table => new
                {
                    ActiveRosterSpotId = table.Column<int>(nullable: false),
                    PositionId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActiveRosterSpotPositions", x => new { x.ActiveRosterSpotId, x.PositionId });
                    table.ForeignKey(
                        name: "FK_ActiveRosterSpotPositions_ActiveRosterSpot_ActiveRosterSpotId",
                        column: x => x.ActiveRosterSpotId,
                        principalTable: "ActiveRosterSpot",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ActiveRosterSpotPositions_Positions_PositionId",
                        column: x => x.PositionId,
                        principalTable: "Positions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ActiveRosterSpotPositions_PositionId",
                table: "ActiveRosterSpotPositions",
                column: "PositionId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActiveRosterSpotPositions");

            migrationBuilder.DropTable(
                name: "ActiveRosterSpot");
        }
    }
}
