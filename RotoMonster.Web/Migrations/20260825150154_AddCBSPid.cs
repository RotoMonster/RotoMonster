using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RotoMonster.Migrations
{
    /// <inheritdoc />
    public partial class AddCBSPid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CBSPid",
                table: "UserAuths",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CBSPid",
                table: "UserAuths");
        }
    }
}
