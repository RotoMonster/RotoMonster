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
