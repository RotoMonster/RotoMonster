using Microsoft.EntityFrameworkCore.Migrations;

namespace RotoMonster.NHL.Migrations.Migrations
{
    public partial class _093021_1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_OwnershipPlayers",
                table: "OwnershipPlayers");

            migrationBuilder.DropColumn(
                name: "CategoriesCode",
                table: "UserLeaguePlayerTypes");

            migrationBuilder.DropColumn(
                name: "CategoriesCode",
                table: "OwnershipPlayers");

            migrationBuilder.DropColumn(
                name: "CategoriesCode",
                table: "DraftPlayerTypes");

            migrationBuilder.AddColumn<int>(
                name: "CategoriesStringId",
                table: "UserLeaguePlayerTypes",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsBench",
                table: "PlayerGameStateTypes",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsStarter",
                table: "PlayerGameStateTypes",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ShowLockAfterStart",
                table: "PlayerGameStateTypes",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "CategoriesStringId",
                table: "OwnershipPlayers",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CategoriesStringId",
                table: "DraftPlayerTypes",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_OwnershipPlayers",
                table: "OwnershipPlayers",
                columns: new[] { "GameDate", "PlayerId", "CategoriesStringId", "LineupFrequency" });

            migrationBuilder.CreateTable(
                name: "CategoriesStrings",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoriesStrings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserLeaguePlayerTypes_CategoriesStringId",
                table: "UserLeaguePlayerTypes",
                column: "CategoriesStringId");

            migrationBuilder.CreateIndex(
                name: "IX_OwnershipPlayers_CategoriesStringId",
                table: "OwnershipPlayers",
                column: "CategoriesStringId");

            migrationBuilder.CreateIndex(
                name: "IX_DraftPlayerTypes_CategoriesStringId",
                table: "DraftPlayerTypes",
                column: "CategoriesStringId");

            migrationBuilder.AddForeignKey(
                name: "FK_DraftPlayerTypes_CategoriesStrings_CategoriesStringId",
                table: "DraftPlayerTypes",
                column: "CategoriesStringId",
                principalTable: "CategoriesStrings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OwnershipPlayers_CategoriesStrings_CategoriesStringId",
                table: "OwnershipPlayers",
                column: "CategoriesStringId",
                principalTable: "CategoriesStrings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserLeaguePlayerTypes_CategoriesStrings_CategoriesStringId",
                table: "UserLeaguePlayerTypes",
                column: "CategoriesStringId",
                principalTable: "CategoriesStrings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DraftPlayerTypes_CategoriesStrings_CategoriesStringId",
                table: "DraftPlayerTypes");

            migrationBuilder.DropForeignKey(
                name: "FK_OwnershipPlayers_CategoriesStrings_CategoriesStringId",
                table: "OwnershipPlayers");

            migrationBuilder.DropForeignKey(
                name: "FK_UserLeaguePlayerTypes_CategoriesStrings_CategoriesStringId",
                table: "UserLeaguePlayerTypes");

            migrationBuilder.DropTable(
                name: "CategoriesStrings");

            migrationBuilder.DropIndex(
                name: "IX_UserLeaguePlayerTypes_CategoriesStringId",
                table: "UserLeaguePlayerTypes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OwnershipPlayers",
                table: "OwnershipPlayers");

            migrationBuilder.DropIndex(
                name: "IX_OwnershipPlayers_CategoriesStringId",
                table: "OwnershipPlayers");

            migrationBuilder.DropIndex(
                name: "IX_DraftPlayerTypes_CategoriesStringId",
                table: "DraftPlayerTypes");

            migrationBuilder.DropColumn(
                name: "CategoriesStringId",
                table: "UserLeaguePlayerTypes");

            migrationBuilder.DropColumn(
                name: "IsBench",
                table: "PlayerGameStateTypes");

            migrationBuilder.DropColumn(
                name: "IsStarter",
                table: "PlayerGameStateTypes");

            migrationBuilder.DropColumn(
                name: "ShowLockAfterStart",
                table: "PlayerGameStateTypes");

            migrationBuilder.DropColumn(
                name: "CategoriesStringId",
                table: "OwnershipPlayers");

            migrationBuilder.DropColumn(
                name: "CategoriesStringId",
                table: "DraftPlayerTypes");

            migrationBuilder.AddColumn<string>(
                name: "CategoriesCode",
                table: "UserLeaguePlayerTypes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CategoriesCode",
                table: "OwnershipPlayers",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CategoriesCode",
                table: "DraftPlayerTypes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_OwnershipPlayers",
                table: "OwnershipPlayers",
                columns: new[] { "GameDate", "PlayerId", "CategoriesCode", "LineupFrequency" });
        }
    }
}
