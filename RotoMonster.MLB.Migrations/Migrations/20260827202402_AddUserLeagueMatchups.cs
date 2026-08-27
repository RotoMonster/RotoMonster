using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RotoMonster.MLB.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class AddUserLeagueMatchups : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserLeagueMatchups",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserLeagueId = table.Column<int>(type: "int", nullable: false),
                    Period = table.Column<int>(type: "int", nullable: false),
                    AwayProviderTeamId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    HomeProviderTeamId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsPlayoff = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserLeagueMatchups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserLeagueMatchups_UserLeagues_UserLeagueId",
                        column: x => x.UserLeagueId,
                        principalTable: "UserLeagues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserLeagueMatchups_UserLeagueId",
                table: "UserLeagueMatchups",
                column: "UserLeagueId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserLeagueMatchups");
        }
    }
}
