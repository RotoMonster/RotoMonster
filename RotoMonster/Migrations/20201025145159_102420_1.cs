using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace RotoMonster.Migrations
{
    public partial class _102420_1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "YahooRequests",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DateAdded = table.Column<DateTime>(nullable: false),
                    UserId = table.Column<string>(nullable: true),
                    ProcessorId = table.Column<string>(nullable: true),
                    Results = table.Column<string>(nullable: true),
                    DateProcessed = table.Column<DateTime>(nullable: false),
                    ErrorMessage = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_YahooRequests", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "YahooRequests");
        }
    }
}
