using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RotoMonster.Data.Migrations
{
    public partial class _220125_1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserOptionTypes",
                columns: table => new
                {
                    Id = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Abbreviation = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    DisplayOrder = table.Column<short>(type: "smallint", nullable: false),
                    DataType = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserOptionTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserOptions",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    UserOptionTypeId = table.Column<short>(type: "smallint", nullable: false),
                    ValueByte = table.Column<byte>(type: "tinyint", nullable: true),
                    ValueShort = table.Column<short>(type: "smallint", nullable: true),
                    ValueInt = table.Column<int>(type: "int", nullable: true),
                    ValueDouble = table.Column<double>(type: "float", nullable: true),
                    ValueString = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserOptions", x => new { x.UserId, x.UserOptionTypeId });
                    table.ForeignKey(
                        name: "FK_UserOptions_UserOptionTypes_UserOptionTypeId",
                        column: x => x.UserOptionTypeId,
                        principalTable: "UserOptionTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserOptions_UserOptionTypeId",
                table: "UserOptions",
                column: "UserOptionTypeId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserOptions");

            migrationBuilder.DropTable(
                name: "UserOptionTypes");
        }
    }
}
