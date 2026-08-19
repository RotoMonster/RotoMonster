using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RotoMonster.MLB.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class TutorialTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Tutorials",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TutorialKey = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Purpose = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsDisabled = table.Column<bool>(type: "bit", nullable: false),
                    ModifiedUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tutorials", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TutorialSections",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TutorialId = table.Column<int>(type: "int", nullable: false),
                    Heading = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Body = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsDisabled = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TutorialSections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TutorialSections_Tutorials_TutorialId",
                        column: x => x.TutorialId,
                        principalTable: "Tutorials",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TutorialSteps",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TutorialId = table.Column<int>(type: "int", nullable: false),
                    TargetSelector = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Body = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Placement = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsDisabled = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TutorialSteps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TutorialSteps_Tutorials_TutorialId",
                        column: x => x.TutorialId,
                        principalTable: "Tutorials",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tutorials_TutorialKey",
                table: "Tutorials",
                column: "TutorialKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TutorialSections_TutorialId",
                table: "TutorialSections",
                column: "TutorialId");

            migrationBuilder.CreateIndex(
                name: "IX_TutorialSteps_TutorialId",
                table: "TutorialSteps",
                column: "TutorialId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TutorialSections");

            migrationBuilder.DropTable(
                name: "TutorialSteps");

            migrationBuilder.DropTable(
                name: "Tutorials");
        }
    }
}
