using Microsoft.EntityFrameworkCore.Migrations;

namespace RotoMonster.Data.Migrations
{
    public partial class fp100 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ProviderLeagueId",
                table: "UserLeagues",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "MyTeamTitle",
                table: "UserLeagues",
                maxLength: 250,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(250)",
                oldMaxLength: 250);

            migrationBuilder.AlterColumn<string>(
                name: "MyProviderTeamId",
                table: "UserLeagues",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<int>(
                name: "StartDayOfWeek",
                table: "Sports",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "LeagueURL",
                table: "FantasyProviders",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDisplayCategory",
                table: "Categories",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "UserDisplayCategories",
                columns: table => new
                {
                    UserId = table.Column<string>(maxLength: 260, nullable: false),
                    CategoryId = table.Column<int>(nullable: false),
                    DisplayOrder = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserDisplayCategories", x => new { x.UserId, x.CategoryId });
                    table.ForeignKey(
                        name: "FK_UserDisplayCategories_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserDisplayCategories_CategoryId",
                table: "UserDisplayCategories",
                column: "CategoryId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserDisplayCategories");

            migrationBuilder.DropColumn(
                name: "StartDayOfWeek",
                table: "Sports");

            migrationBuilder.DropColumn(
                name: "LeagueURL",
                table: "FantasyProviders");

            migrationBuilder.DropColumn(
                name: "IsDisplayCategory",
                table: "Categories");

            migrationBuilder.AlterColumn<string>(
                name: "ProviderLeagueId",
                table: "UserLeagues",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MyTeamTitle",
                table: "UserLeagues",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: false,
                oldClrType: typeof(string),
                oldMaxLength: 250,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MyProviderTeamId",
                table: "UserLeagues",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldMaxLength: 100,
                oldNullable: true);
        }
    }
}
