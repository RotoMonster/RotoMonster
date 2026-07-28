using Microsoft.EntityFrameworkCore.Migrations;

namespace RotoMonster.NFL.Migrations.Migrations
{
    public partial class _100120_1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {


            migrationBuilder.CreateTable(
                name: "PlayerStatusTypes",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(nullable: true),
                    BackgroundColor = table.Column<string>(nullable: true),
                    TextColor = table.Column<string>(nullable: true),
                    TextFormat = table.Column<string>(nullable: true),
                    AutoClear = table.Column<bool>(nullable: true),
                    UsesDate = table.Column<bool>(nullable: true),
                    ShowInDaily = table.Column<bool>(nullable: true),
                    AllowFilter = table.Column<bool>(nullable: true),
                    AppliesToNextGame = table.Column<bool>(nullable: true),
                    IsInGame = table.Column<bool>(nullable: true),
                    IsUndetermined = table.Column<bool>(nullable: true),
                    ShowOnPlayerProfile = table.Column<bool>(nullable: true),
                    EndOfGameMissedPlayerStatusTypeId = table.Column<int>(nullable: false),
                    EndOfGamePlayedPlayerStatusTypeId = table.Column<int>(nullable: false),
                    TweetTemplate = table.Column<string>(nullable: true),
                    UpdateTemplate = table.Column<string>(nullable: true),
                    PlayType = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerStatusTypes", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
