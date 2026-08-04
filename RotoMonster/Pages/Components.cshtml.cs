using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RotoMonster.Core;
using RotoMonster.Data;
using System.Collections.Generic;

namespace RotoMonster.Pages
{
    /// <summary>
    /// Preview page for library components. Dummy data only - nothing here
    /// touches the database beyond what RMPageModel already does.
    /// </summary>
    public class ComponentsModel : RMPageModel
    {
        public List<RotoMonsterUI.ScoringCategory> DemoCategories { get; set; }
            = new List<RotoMonsterUI.ScoringCategory>();

        public List<RotoMonsterUI.LeagueSettingsCategory> DemoLeagueCategories { get; set; }
            = new List<RotoMonsterUI.LeagueSettingsCategory>();

        public List<RotoMonsterUI.DisplayPlayerInput> DemoSearchPlayers { get; set; }
            = new List<RotoMonsterUI.DisplayPlayerInput>();

        public RotoMonsterUI.LineupCardInput DemoLineupCard { get; set; }

        public List<MonsterBarItem> DemoMonsterBarItems { get; set; }
            = new List<MonsterBarItem>();

        public ComponentsModel(IConfiguration config, IRMData db, IRMSharedData sharedDb,
            UserManager<ApplicationUser> userManager, IHttpContextAccessor contextAccessor,
            ILogger<PageModel> logger)
            : base(config, db, sharedDb, userManager, contextAccessor, logger)
        {
        }

        public IActionResult OnGet()
        {
            InitGet("Components");

            DemoCategories = new List<RotoMonsterUI.ScoringCategory>
            {
                new RotoMonsterUI.ScoringCategory { Id = 1, Abbreviation = "R", ColorCSS = "#e66000" },
                new RotoMonsterUI.ScoringCategory { Id = 2, Abbreviation = "HR", ColorCSS = "#e66000" },
                new RotoMonsterUI.ScoringCategory { Id = 3, Abbreviation = "RBI", ColorCSS = "#e66000" },
                new RotoMonsterUI.ScoringCategory { Id = 4, Abbreviation = "SB", ColorCSS = "#e66000" },
                new RotoMonsterUI.ScoringCategory { Id = 5, Abbreviation = "AVG", ColorCSS = "#e66000" },
                new RotoMonsterUI.ScoringCategory { Id = 6, Abbreviation = "W", ColorCSS = "#2563eb" },
                new RotoMonsterUI.ScoringCategory { Id = 7, Abbreviation = "SV", ColorCSS = "#2563eb" },
                new RotoMonsterUI.ScoringCategory { Id = 8, Abbreviation = "K", ColorCSS = "#2563eb" },
                new RotoMonsterUI.ScoringCategory { Id = 9, Abbreviation = "ERA", ColorCSS = "#2563eb" },
                new RotoMonsterUI.ScoringCategory { Id = 10, Abbreviation = "WHIP", ColorCSS = "#2563eb" }
            };

            DemoLeagueCategories = new List<RotoMonsterUI.LeagueSettingsCategory>
            {
                new RotoMonsterUI.LeagueSettingsCategory { Abbreviation = "R" },
                new RotoMonsterUI.LeagueSettingsCategory { Abbreviation = "HR" },
                new RotoMonsterUI.LeagueSettingsCategory { Abbreviation = "RBI" },
                new RotoMonsterUI.LeagueSettingsCategory { Abbreviation = "SB", IsActive = false },
                new RotoMonsterUI.LeagueSettingsCategory { Abbreviation = "AVG" },
                new RotoMonsterUI.LeagueSettingsCategory { Abbreviation = "W" },
                new RotoMonsterUI.LeagueSettingsCategory { Abbreviation = "SV" },
                new RotoMonsterUI.LeagueSettingsCategory { Abbreviation = "K" },
                new RotoMonsterUI.LeagueSettingsCategory { Abbreviation = "ERA", IsActive = false },
                new RotoMonsterUI.LeagueSettingsCategory { Abbreviation = "WHIP" }
            };

            DemoSearchPlayers = new List<RotoMonsterUI.DisplayPlayerInput>
            {
                new RotoMonsterUI.DisplayPlayerInput { PlayerId = 1, PlayerName = "Corbin Carroll", TeamCode = "ARI" },
                new RotoMonsterUI.DisplayPlayerInput { PlayerId = 2, PlayerName = "Ketel Marte", TeamCode = "ARI" },
                new RotoMonsterUI.DisplayPlayerInput { PlayerId = 3, PlayerName = "Matt Olson", TeamCode = "ATL" },
                new RotoMonsterUI.DisplayPlayerInput { PlayerId = 4, PlayerName = "Ronald Acuna", TeamCode = "ATL" },
                new RotoMonsterUI.DisplayPlayerInput { PlayerId = 5, PlayerName = "Austin Riley", TeamCode = "ATL" },
                new RotoMonsterUI.DisplayPlayerInput { PlayerId = 6, PlayerName = "Vladimir Guerrero", TeamCode = "TOR" }
            };

            DemoLineupCard = new RotoMonsterUI.LineupCardInput
            {
                Id = "lineupdemo",
                Game = new RotoMonsterUI.GameInput
                {
                    GameId = 1,
                    Sport = RotoMonsterUI.GameSport.Baseball,
                    GameTimeUtc = System.DateTime.UtcNow.AddHours(3),
                    DisplayTimezone = System.TimeZoneInfo.Local
                },
                AwayTeam = new RotoMonsterUI.LineupCardTeamInput
                {
                    TeamCode = "ARI",
                    ProjectedRuns = 4.6f,
                    IsLineupConfirmed = true,
                    TeamColor = "A71930",
                    Players = new List<RotoMonsterUI.LineupPlayer>
                    {
                        new RotoMonsterUI.LineupPlayer { BattingOrder = 1, Position = "OF", Handedness = "L",
                            Player = new RotoMonsterUI.DisplayPlayerInput { PlayerId = 1, PlayerName = "Corbin Carroll", TeamCode = "ARI" } },
                        new RotoMonsterUI.LineupPlayer { BattingOrder = 2, Position = "2B", Handedness = "S",
                            Player = new RotoMonsterUI.DisplayPlayerInput { PlayerId = 2, PlayerName = "Ketel Marte", TeamCode = "ARI" } },
                        new RotoMonsterUI.LineupPlayer { BattingOrder = 3, Position = "C", Handedness = "R", IsOwned = true,
                            Player = new RotoMonsterUI.DisplayPlayerInput { PlayerId = 7, PlayerName = "Gabriel Moreno", TeamCode = "ARI" } }
                    }
                },
                HomeTeam = new RotoMonsterUI.LineupCardTeamInput
                {
                    TeamCode = "ATL",
                    ProjectedRuns = 5.1f,
                    IsLineupConfirmed = false,
                    IsHomeTeam = true,
                    TeamColor = "CE1141",
                    Players = new List<RotoMonsterUI.LineupPlayer>
                    {
                        new RotoMonsterUI.LineupPlayer { BattingOrder = 1, Position = "OF", Handedness = "R",
                            Player = new RotoMonsterUI.DisplayPlayerInput { PlayerId = 4, PlayerName = "Ronald Acuna", TeamCode = "ATL" } },
                        new RotoMonsterUI.LineupPlayer { BattingOrder = 2, Position = "1B", Handedness = "L", IsOwned = true,
                            Player = new RotoMonsterUI.DisplayPlayerInput { PlayerId = 3, PlayerName = "Matt Olson", TeamCode = "ATL" } },
                        new RotoMonsterUI.LineupPlayer { BattingOrder = 3, Position = "3B", Handedness = "R",
                            Player = new RotoMonsterUI.DisplayPlayerInput { PlayerId = 5, PlayerName = "Austin Riley", TeamCode = "ATL" } }
                    }
                }
            };

            // Same shape RMSqlData builds - title is the short id, description the label.
            DemoMonsterBarItems = new List<MonsterBarItem>
            {
                new MonsterBarItem { Title = "LS", Description = "Last Season" },
                new MonsterBarItem { Title = "S", Description = "Current Season" },
                new MonsterBarItem { Title = "2M", Description = "Past 2 Months" },
                new MonsterBarItem { Title = "3W", Description = "Past 3 Weeks" },
                new MonsterBarItem { Title = "W", Description = "Past Week" }
            };

            return Page();
        }
    }
}
