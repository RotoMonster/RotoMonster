using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace RotoMonster.NFL.Migrations.Migrations
{
    public partial class _100320_1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PlayerStatuses",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlayerId = table.Column<int>(nullable: false),
                    GameId = table.Column<int>(nullable: true),
                    PlayerStatusTypeId = table.Column<int>(nullable: false),
                    PlayerStatusTagTypeId = table.Column<int>(nullable: true),
                    OwningUserId = table.Column<string>(nullable: true),
                    DateAdded = table.Column<DateTime>(nullable: false),
                    Comment = table.Column<string>(nullable: true),
                    Subject = table.Column<string>(nullable: true),
                    Source = table.Column<string>(nullable: true),
                    SourceUrl = table.Column<string>(nullable: true),
                    DateDeleted = table.Column<DateTime>(nullable: true),
                    DeletedByUserId = table.Column<string>(nullable: true),
                    GamePercent = table.Column<short>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerStatuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PlayerStatusTagTypes",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(nullable: true),
                    Icon = table.Column<string>(nullable: true),
                    IsDefault = table.Column<bool>(nullable: true),
                    DisplayOrder = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerStatusTagTypes", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlayerStatuses");

            migrationBuilder.DropTable(
                name: "PlayerStatusTagTypes");
        }
    }
}
