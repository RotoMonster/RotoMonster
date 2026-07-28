using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace RotoMonster.Migrations
{
    public partial class ua : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserAuths",
                columns: table => new
                {
                    UserId = table.Column<string>(maxLength: 260, nullable: false),
                    Provider = table.Column<string>(maxLength: 10, nullable: false),
                    DateAdded = table.Column<DateTime>(nullable: false),
                    LastUsed = table.Column<DateTime>(nullable: false),
                    HasBeenUsed = table.Column<bool>(nullable: false),
                    YahooAccessToken = table.Column<string>(maxLength: 200, nullable: true),
                    YahooRefreshToken = table.Column<string>(maxLength: 200, nullable: true),
                    ESPNswid = table.Column<string>(maxLength: 200, nullable: true),
                    ESPNs2 = table.Column<string>(maxLength: 200, nullable: true),
                    ESPNInfo = table.Column<string>(maxLength: 400, nullable: true),
                    CBSUsername = table.Column<string>(maxLength: 100, nullable: true),
                    CBSPassword = table.Column<string>(maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAuths", x => new { x.UserId, x.Provider });
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserAuths");
        }
    }
}
