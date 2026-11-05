using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RotoMonster.Core;
using RotoMonster.Data;
using RotoMonsterUI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RotoMonster.Pages
{
    public class HelpModel : RMPageModel
    {
        public List<Helper> Helpers { get; set; }
        public string HelpHtml { get; set; }

        public HelpModel(IConfiguration config, IRMData db, IRMSharedData sharedDb, UserManager<ApplicationUser> userManager, IHttpContextAccessor contextAccessor, ILogger<PageModel> logger)
            : base(config, db, sharedDb, userManager, contextAccessor, logger)
        {
        }

        public IActionResult OnGet(int? from)
        {
            InitGet("Help");

            Helpers = (db.GetHelpers() ?? new List<Helper>()).Where(h => !h.IsDisabled).OrderBy(h => h.DisplayOrder).ToList();
            var sport = db.Sport.Title;

            var input = new HelpPageInput
            {
                Id = "rmHelp",
                SearchPlaceholder = "Search help, like \"free agents\" or \"Own%\"",
                ToolsGroupId = "shortcuts",
                ToolsGroupTitle = "Shortcuts",
                ToolsHintText = "Preset views that open a page already filtered for a common job.",
                FromToolFormat = "You came from {0}",
                FromToolNoteText = "Its shortcut is first below.",
                StuckTitle = "Still stuck?",
                StuckText = "Send us a message and we will get back to you.",
                StuckLinks = new List<HelpToolLink>
                {
                    new HelpToolLink { Text = "Contact support", Url = Url.Page("/Support/Index") }
                }
            };

            var fromHelper = from.HasValue ? Helpers.FirstOrDefault(h => h.Id == from.Value) : null;
            if (fromHelper != null)
                input.FromToolName = Clean(fromHelper.Title);

            foreach (var h in Helpers)
            {
                input.Tools.Add(new HelpTool
                {
                    Name = Clean(h.Title),
                    Url = HelperUrl(h, sport),
                    Description = Clean(string.IsNullOrWhiteSpace(h.OnPageDetails) ? h.Description : h.OnPageDetails),
                    SearchTerms = new List<string> { Clean(h.Description) },
                    Links = new List<HelpToolLink> { new HelpToolLink { Text = "Open", Url = HelperUrl(h, sport) } }
                });
            }

            input.Groups.Add(new HelpGroup
            {
                Id = "start",
                Title = "Getting started",
                Topics = new List<HelpTopic>
                {
                    new HelpTopic
                    {
                        Id = "add-leagues",
                        Title = "Add your leagues",
                        WhenText = "Do this first",
                        IsExpanded = true,
                        SearchTerms = new List<string> { "import", "yahoo", "espn", "fantrax", "connect" },
                        BodyHtml = "<p>Click <b>Add Leagues</b> on the home page to import your leagues from Yahoo, ESPN or FanTrax. Once a league is added, the site shows your roster, your free agents and MonsterBot suggestions for it.</p>"
                            + "<p><a href=\"" + Url.Page("/UserLeagues/Import") + "\">Add leagues</a></p>"
                    },
                    new HelpTopic
                    {
                        Id = "switch-league",
                        Title = "Switch between leagues",
                        SearchTerms = new List<string> { "dropdown", "selected league", "change league" },
                        BodyHtml = "<p>Use the league dropdown at the top of every page. Rankings, Scores and the other pages use the league you have selected for its scoring and rosters.</p>"
                    },
                    new HelpTopic
                    {
                        Id = "refresh-rosters",
                        Title = "Keep rosters up to date",
                        SearchTerms = new List<string> { "refresh", "sync", "update", "trade", "pickup" },
                        BodyHtml = "<p>Click <b>Refresh Rosters</b> at the top of the page to pull your latest rosters. Rosters also refresh on their own when you open a page and your league has not been updated in a few hours.</p>"
                    }
                }
            });

            input.Groups.Add(new HelpGroup
            {
                Id = "reading",
                Title = "Reading the numbers",
                Topics = new List<HelpTopic>
                {
                    new HelpTopic
                    {
                        Id = "value",
                        Title = "TotalV and GameV",
                        SearchTerms = new List<string> { "value", "total value", "per game", "rank" },
                        BodyHtml = "<div class=\"hp-deflist\">"
                            + "<div><b>TotalV</b><span>The value a player has produced across the whole date range. It rewards players who stay healthy and play often.</span></div>"
                            + "<div><b>GameV</b><span>The value a player produces per game. It shows who is most productive when they play.</span></div>"
                            + "</div>"
                    },
                    new HelpTopic
                    {
                        Id = "ownership",
                        Title = "Own% and Act%",
                        SearchTerms = new List<string> { "ownership", "active", "start", "rostered", "advanced ownership" },
                        BodyHtml = "<div class=\"hp-deflist\">"
                            + "<div><b>Own%</b><span>The percent of leagues where a player is rostered.</span></div>"
                            + "<div><b>Act% / Start%</b><span>The percent of leagues where a player is in the starting lineup.</span></div>"
                            + "</div>"
                            + "<p>A player with high Own% but low Act% is being held rather than played.</p>"
                    },
                    new HelpTopic
                    {
                        Id = "monsterbar",
                        Title = "Monster Bars",
                        SearchTerms = new List<string> { "monster bar", "bar", "colors" },
                        BodyHtml = "<p>Monster Bars show a player's value at a glance. There are two kinds: a per game bar and a total bar. Turn either one on or off in <a href=\"" + Url.Page("/DisplaySettings") + "\">Display Settings</a>.</p>"
                    },
                    new HelpTopic
                    {
                        Id = "injuries",
                        Title = "Injury statuses",
                        SearchTerms = new List<string> { "injury", "questionable", "out", "doubtful", "ir", "return" },
                        BodyHtml = "<p>Badges like <b>Q</b> (Questionable), <b>D</b> (Doubtful) and <b>OUT</b> show a player's current status. Hover a badge for the full note. The <a href=\"" + Url.Page("/Injuries") + "\">Injuries</a> page lists every current status with filters and expected return dates.</p>"
                    }
                }
            });

            input.Groups.Add(new HelpGroup
            {
                Id = "settings",
                Title = "Settings",
                Topics = new List<HelpTopic>
                {
                    new HelpTopic
                    {
                        Id = "columns",
                        Title = "Choose which columns show",
                        SearchTerms = new List<string> { "columns", "display", "stats", "hide" },
                        BodyHtml = "<p>Go to <a href=\"" + Url.Page("/DisplaySettings") + "\">Settings &gt; Display</a> to pick the stat and value columns in the tables, then click Save.</p>"
                    },
                    new HelpTopic
                    {
                        Id = "timezone",
                        Title = "Change the time zone",
                        SearchTerms = new List<string> { "timezone", "time", "game times" },
                        BodyHtml = "<p>Game times show in Eastern Time by default. Change it at the top of <a href=\"" + Url.Page("/DisplaySettings") + "\">Settings &gt; Display</a>.</p>"
                    },
                    new HelpTopic
                    {
                        Id = "dark-mode",
                        Title = "Light and dark mode",
                        SearchTerms = new List<string> { "dark", "light", "theme" },
                        BodyHtml = "<p>Click the sun or moon icon in the top menu to switch between light and dark mode.</p>"
                    }
                }
            });

            input.Groups.Add(new HelpGroup
            {
                Id = "account",
                Title = "Your account",
                Topics = new List<HelpTopic>
                {
                    new HelpTopic
                    {
                        Id = "invitation",
                        Title = "Creating an account",
                        SearchTerms = new List<string> { "register", "sign up", "invitation", "beta" },
                        BodyHtml = "<p>While we are in beta you need an Invitation ID to create an account. Basketball Monster members can find theirs on Basketball Monster under Settings &gt; User Settings.</p>"
                    },
                    new HelpTopic
                    {
                        Id = "support",
                        Title = "Contacting support",
                        SearchTerms = new List<string> { "help", "contact", "email", "bug", "question" },
                        BodyHtml = "<p>Click the envelope icon in the top menu to send us a message.</p>"
                    }
                }
            });

            HelpHtml = new HelpPage(input).Render();

            return Page();
        }

        private static string Clean(string s)
        {
            return (s ?? "").Replace("\r", " ").Replace("\n", " ").Trim();
        }

        private string HelperUrl(Helper h, string sport)
        {
            var url = h.GetPlayerTypeUrl(h.Url, null) ?? "";
            var prefix = "/" + (sport ?? "").Trim().ToLower();
            if (!Request.PathBase.HasValue && Request.Host.Host == "localhost" && url.StartsWith(prefix + "/", StringComparison.OrdinalIgnoreCase))
                url = Url.Content("~" + url.Substring(prefix.Length));
            return url;
        }
    }
}
