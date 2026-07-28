using Microsoft.EntityFrameworkCore.Migrations;

namespace RotoMonster.NFL.Migrations.Migrations
{
    public partial class _090320_1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DraftPlayerTypes",
                columns: table => new
                {
                    DraftId = table.Column<int>(nullable: false),
                    PlayerTypeId = table.Column<int>(nullable: false),
                    CategoriesCode = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DraftPlayerTypes", x => new { x.DraftId, x.PlayerTypeId });
                    table.ForeignKey(
                        name: "FK_DraftPlayerTypes_Drafts_DraftId",
                        column: x => x.DraftId,
                        principalTable: "Drafts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DraftPlayerTypes_PlayerTypes_PlayerTypeId",
                        column: x => x.PlayerTypeId,
                        principalTable: "PlayerTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DraftPlayerTypes_PlayerTypeId",
                table: "DraftPlayerTypes",
                column: "PlayerTypeId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DraftPlayerTypes");
        }
    }
}
