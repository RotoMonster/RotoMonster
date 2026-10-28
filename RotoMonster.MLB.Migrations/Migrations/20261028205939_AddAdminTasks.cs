using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RotoMonster.MLB.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class AddAdminTasks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AdminDuties",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    IsOnDuty = table.Column<bool>(type: "bit", nullable: false),
                    ReadyAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Auto = table.Column<bool>(type: "bit", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdminDuties", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AdminTasks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Text = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    UrlText = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsAdminOnly = table.Column<bool>(type: "bit", nullable: false),
                    Source = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DedupeKey = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DoneAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DoneBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ActionType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ActionLabel = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    ActionJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ActionRanAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ActionResult = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdminTasks", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AdminDuties_UserName",
                table: "AdminDuties",
                column: "UserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AdminTasks_DedupeKey",
                table: "AdminTasks",
                column: "DedupeKey");

            migrationBuilder.CreateIndex(
                name: "IX_AdminTasks_DoneAt",
                table: "AdminTasks",
                column: "DoneAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdminDuties");

            migrationBuilder.DropTable(
                name: "AdminTasks");
        }
    }
}
