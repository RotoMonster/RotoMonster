using Microsoft.EntityFrameworkCore.Migrations;

namespace RotoMonster.Data.Migrations
{
    public partial class ps : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PositionSourcePlayers",
                columns: table => new
                {
                    PositionSourceId = table.Column<int>(nullable: false),
                    PlayerId = table.Column<int>(nullable: false),
                    PositionId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PositionSourcePlayers", x => new { x.PositionSourceId, x.PlayerId, x.PositionId });
                });

            migrationBuilder.CreateTable(
                name: "PositionSourcePositions",
                columns: table => new
                {
                    PositionSourceId = table.Column<int>(nullable: false),
                    PositionId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PositionSourcePositions", x => new { x.PositionSourceId, x.PositionId });
                });

            migrationBuilder.CreateTable(
                name: "PositionSources",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProviderId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PositionSources", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PositionSourcePlayers");

            migrationBuilder.DropTable(
                name: "PositionSourcePositions");

            migrationBuilder.DropTable(
                name: "PositionSources");
        }
    }
}
