using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RotoMonster.Migrations
{
    /// <inheritdoc />
    public partial class AddSleeperAuth : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SleeperId",
                table: "UserAuths",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SleeperName",
                table: "UserAuths",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SleeperId",
                table: "UserAuths");

            migrationBuilder.DropColumn(
                name: "SleeperName",
                table: "UserAuths");
        }
    }
}
