using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RotoMonster.NHL.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class AddGameReprocessCount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ReprocessCount",
                table: "Games",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReprocessCount",
                table: "Games");
        }
    }
}
