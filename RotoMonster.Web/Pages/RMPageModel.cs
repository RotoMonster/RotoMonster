using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RotoMonster.Core;
using RotoMonster.Core.Libs;
using RotoMonster.Models.Shared;
using RotoMonster.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RotoMonster.Pages
{
    public class RMPageModel : PageModel
    {
        protected readonly IConfiguration config;
        protected readonly IRMData db;
        protected readonly IRMSharedData sharedDb;
        protected readonly UserManager<ApplicationUser> userManager;
        protected readonly IHttpContextAccessor contextAccessor;
        protected readonly ILogger logger;

        public string UserId { get; set; }
        public bool IsLoggedIn { get; }
        public YahooLib YahooLib { get; }
        public FanTraxLib FanTraxLib { get; }
        public List<UserLeague> SelectedUserLeagues { get; set; }

        // Hoisted from the individual pages so _Layout can render one league
        // dropdown for the whole site. Pages still assign it exactly as before.
        [BindProperty]
        public int SelectedUserLeagueId { get; set; }
        public Helper SelectedHelper { get; set; } = null;

        public RMPageModel(
            IConfiguration config,
            IRMData db,
            IRMSharedData sharedDb,
            UserManager<ApplicationUser> userManager,
            IHttpContextAccessor contextAccessor,
            ILogger<PageModel> logger)
        {
            this.config = config;
            this.db = db;
            this.sharedDb = sharedDb;
            this.userManager = userManager;
            this.contextAccessor = contextAccessor;
            this.logger = logger;
            YahooLib = new YahooLib(config, config["YahooClientId"], config["YahooClientSecret"], db.GetDefaultSeason().YahooId, logger);
            FanTraxLib = new FanTraxLib(config, logger);
            UserId = userManager.GetUserId(contextAccessor.HttpContext.User);
            IsLoggedIn = (UserId != null);
            SelectedUserLeagues = db.GetTrackedUserLeagues(UserId);

            //if (UserId != null)
            //{
            //    UserAuth userAuth = sharedDb.GetUserAuth(UserId);
            //    if (userAuth.MustRefreshYahoo)
            //    {
            //        string outAccessToken = "";
            //        string outRefreshToken = "";
            //        if (YahooLib.RefreshAccessToken(userAuth.YahooRefreshToken, ref outAccessToken, ref outRefreshToken))
            //        {
            //            sharedDb.AddYahooUserAuth(userAuth.UserId, outAccessToken, outRefreshToken);
            //        }
            //    }
            //    YahooLib.GetYahooGameKeysXml(userAuth, "nhl");
            //}

        }

        protected void InitGet(string pageTitle, string testuser = null)
        {
            if (User.IsInRole("Admin"))
            {
                if (testuser != null && testuser.Length > 0)
                {
                    var foundUser = (from u in sharedDb.GetUsers() where u.NormalizedUserName == testuser.ToUpper().Trim() select u).FirstOrDefault();
                    if (foundUser != null)
                    {
                        UserId = foundUser.Id;
                        SelectedUserLeagues = db.GetTrackedUserLeagues(UserId);
                    }
                }
            }

            ViewData["Helpers"] = db.GetHelpers();

            if (pageTitle.Length > 0)
            {
                ViewData["pagetitle"] = pageTitle + " - RotoMonster" + db.Sport.Title;

                // The bare name, for the header block. "pagetitle" above is the
                // browser tab text and is too long to use as a heading.
                ViewData["PageHeading"] = pageTitle;
            }

            if (User.IsInRole("Admin"))
            {
                ViewData["AdminErrors"] = db.GetLogItems("Error");
            }

            var defaultSeason = db.GetDefaultSeason();
            ViewData["SeasonState"] = defaultSeason.State;

            // Feeds the header progress bar. Season.State is already formatted
            // text, so the percentage is derived from the real dates instead.
            var seasonToday = DateTime.UtcNow.Date;
            if (seasonToday < defaultSeason.StartDate.Date)
            {
                ViewData["SeasonDaysUntil"] = (int)(defaultSeason.StartDate.Date - seasonToday).TotalDays;
                ViewData["SeasonPercent"] = 0d;
            }
            else
            {
                var seasonDays = (defaultSeason.EndDate.Date - defaultSeason.StartDate.Date).TotalDays;
                var seasonElapsed = (seasonToday - defaultSeason.StartDate.Date).TotalDays;
                ViewData["SeasonPercent"] = seasonDays > 0
                    ? System.Math.Min(100d, seasonElapsed / seasonDays * 100d)
                    : 100d;
            }
        }

        public IRMData NewDb
        {
            get
            {
                var builder = new DbContextOptionsBuilder<RMDBContext>();
                builder.UseSqlServer(config.GetConnectionString("RotoMonsterDb").Replace("{sport}", db.Sport.Title));
                var db1 = new RMDBContext(builder.Options);
                return new RMSqlData(db1, null, null);
            }
        }

        public IRMSharedData NewSharedDb
        {
            get
            {
                var builderS = new DbContextOptionsBuilder<RMSharedDbContext>();
                builderS.UseSqlServer(config.GetConnectionString("RotoMonsterSharedDb"));
                var db2 = new RMSharedDbContext(builderS.Options);
                return new RMSharedSqlData(db2, null, null);
            }
        }

        private int AddMessageToList(string listId, string msg)
        {
            if (TempData == null) return 0;

            var list = new List<string>();

            var existing = TempData[listId];
            if (existing is IEnumerable<string> current)
                list.AddRange(current);

            list.Add(msg);

            TempData[listId] = list;
            return list.Count;
        }

        public int AddMessage(string msg)
        {
            return AddMessageToList("messages", msg);
        }

        public int AddErrorMessage(string msg)
        {
            return AddMessageToList("errormessages", msg);
        }

        public int AddWarningMessage(string msg)
        {
            return AddMessageToList("warningmessages", msg);
        }

        public UserLeague RefreshRosters(UserLeague userLeague)
        {
            return null;

            if (userLeague.ProviderLeagueId == null || userLeague.ProviderLeagueId.Length == 0)
                return userLeague;

            UserLeague outUserLeague = userLeague;

            IRMData tmpDB = NewDb;
            IRMSharedData tmpSharedDB = NewSharedDb;

            if (userLeague != null)
            {
                UserAuth userAuth = tmpSharedDB.GetUserAuth(UserId);
                if (userAuth == null)
                {
                    AddErrorMessage("There was no authorization associated with your account.  Click Add Leagues and add an autorization.");
                    return userLeague;
                }

                if (userLeague.FantasyProviderId == 1)
                {
                    try
                    {
                        var missingPlayers = new List<UserLeagueMissingPlayer>();
                        List<UserLeagueTeam> teams = tmpSharedDB.GetUserLeagueTeams(userAuth, tmpDB.GetDefaultSeason().YahooId, userLeague, tmpDB.GetFantasyProviderPlayers(tmpDB.GetFantasyProvider("yahoo")), missingPlayers, logger);
                        if (teams.Count == 0)
                        {
                            AddErrorMessage("There were no teams imported for " + userLeague.DisplayTitle + ". Please try again.");
                        }
                        else
                        {
                            tmpDB.UpdateUserLeagueTeams(userLeague.Id, teams, missingPlayers, null);
                            AddMessage(userLeague.DisplayTitle + " rosters updated");
                            //var draft = tmpDB.GetDraft(userLeague.FantasyProvider, userLeague.ProviderLeagueId);
                            //if (draft == null)
                            //    tmpSharedDB.ImportDraft(userAuth, tmpDB.GetDefaultSeason(), userLeague.ProviderLeagueId, tmpDB.GetFantasyProviderPlayers(tmpDB.GetFantasyProvider("yahoo")), logger);
                        }
                        outUserLeague = tmpDB.GetUserLeague(userLeague.Id);
                    }
                    catch (Exception ex)
                    {
                        AddErrorMessage("Unable to import rosters.  Make sure your league has drafted. [" + ex.Message + "]");
                    }
                }

                else if (userLeague.FantasyProviderId == 4)
                {
                    FanTraxLib fantrax = new FanTraxLib(config, logger);
                    var missingPlayers = new List<UserLeagueMissingPlayer>();
                    List<UserLeagueTeam> teams = fantrax.GetUserLeagueTeams(userAuth, tmpDB.Sport.Title, userLeague, tmpDB.GetFantasyProviderPlayers(tmpDB.GetFantasyProvider("fantrax")), missingPlayers);
                    tmpDB.UpdateUserLeagueTeams(userLeague.Id, teams, missingPlayers, userLeague.UserLeagueWaiverPlayers);
                    outUserLeague = tmpDB.GetUserLeague(userLeague.Id);
                    AddMessage(userLeague.DisplayTitle + " rosters updated");
                }

                else if (userLeague.FantasyProviderId == 2)
                {
                    ESPNLib espn = new ESPNLib(config, logger);
                    var missingPlayers = new List<UserLeagueMissingPlayer>();
                    List<UserLeagueTeam> teams = espn.GetUserLeagueTeams(userAuth, tmpDB.Sport, tmpDB.GetDefaultSeason(), userLeague, tmpDB.GetFantasyProviderPlayers(tmpDB.GetFantasyProvider("espn")), db.GetPlayers(), missingPlayers);
                    tmpDB.UpdateUserLeagueTeams(userLeague.Id, teams, missingPlayers, userLeague.UserLeagueWaiverPlayers);
                    outUserLeague = tmpDB.GetUserLeague(userLeague.Id);
                    AddMessage(userLeague.DisplayTitle + " rosters updated");
                }
            }

            return outUserLeague;
        }

        public UserAuth RefreshYahoo(UserAuth userAuth)
        {
            if (userAuth.MustRefreshYahoo)
            {
                string outAccessToken = "";
                string outRefreshToken = "";
                if (YahooLib.RefreshAccessToken(userAuth.YahooRefreshToken, ref outAccessToken, ref outRefreshToken))
                {
                    return sharedDb.AddYahooUserAuth(userAuth.UserId, outAccessToken, outRefreshToken);
                }
            }

            return userAuth;
        }

        public IActionResult OnGetClearCache()
        {
            db.ClearCache();
            return RedirectToPage("Index");
        }

    }
}
