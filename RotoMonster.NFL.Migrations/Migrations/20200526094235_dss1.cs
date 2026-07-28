using Microsoft.EntityFrameworkCore.Migrations;

namespace RotoMonster.NFL.Migrations.Migrations
{
    public partial class dss1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DefaultScoringSystem",
                table: "Sports",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "DefaultPointsPerStat",
                table: "Categories",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DefaultScoringSystem",
                table: "Sports");

            migrationBuilder.DropColumn(
                name: "DefaultPointsPerStat",
                table: "Categories");
        }
    }
}
