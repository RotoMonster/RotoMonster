using Microsoft.EntityFrameworkCore.Migrations;

namespace RotoMonster.Data.Migrations
{
    public partial class ul3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserLeagueTeams",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserLeagueId = table.Column<int>(nullable: false),
                    TeamNumber = table.Column<int>(nullable: false),
                    Title = table.Column<string>(nullable: true),
                    ProviderId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserLeagueTeams", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserLeagueTeams_UserLeagues_UserLeagueId",
                        column: x => x.UserLeagueId,
                        principalTable: "UserLeagues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserLeagueTeams_UserLeagueId",
                table: "UserLeagueTeams",
                column: "UserLeagueId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserLeagueTeams");
        }
    }
}
