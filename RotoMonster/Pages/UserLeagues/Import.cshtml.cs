using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RotoMonster.Core;
using RotoMonster.Core.Libs;
using RotoMonster.Core.PartialViewModels;
using RotoMonster.Data;

namespace RotoMonster.Pages.UserLeagues
{
    [Authorize]
    public class ImportModel : RMPageModel
    {
        [BindProperty]
        public string YahooCode { get; set; }
        [BindProperty]
        public string ESPNUsername { get; set; }
        [BindProperty]
        public string ESPNPassword { get; set; }
        [BindProperty]
        public string FanTraxEmail { get; set; }
        [BindProperty]
        public string FanTraxLeagueId { get; set; }

        [BindProperty]
        public string ESPNSWID { get; set; }
        [BindProperty]
        public string ESPNS2 { get; set; }

        public UserAuth UserAuth { get; set; }
        public Sport Sport { get; set; }
        public bool IsYahooConnected { get; set; }
        public string YahooUrl { get; set; }
        public bool IsESPNConnected { get; set; }
        public bool IsFanTraxConnected { get; set; }
        public UserLeagueTableModel YahooUserLeagueTableModel { get; set; }
        public UserLeagueTableModel ESPNUserLeagueTableModel { get; set; }
        public UserLeagueTableModel FanTraxUserLeagueTableModel { get; set; }

        private YahooLib yahoo = null;
        private FanTraxLib fanTrax = null;
        private ESPNLib espn = null;

        public ImportModel(IRMData db, IRMSharedData sharedDb, IConfiguration config, UserManager<ApplicationUser> userManager, IHttpContextAccessor contextAccessor, ILogger<PageModel> logger)
            : base(config, db, sharedDb, userManager, contextAccessor, logger)
        {
            yahoo = new YahooLib(config, config["YahooClientId"], config["YahooClientSecret"], db.GetDefaultSeason().YahooId, logger);
            fanTrax = new FanTraxLib(config, logger);
            espn = new ESPNLib(config, logger);

            UserAuth = sharedDb.GetUserAuth(UserId);
            Sport = db.Sport;

            YahooUrl = yahoo.GetAuthorizationURL();

            IsYahooConnected = yahoo.IsConnected(UserAuth);
            IsESPNConnected = espn.IsConnected(UserAuth);
            IsFanTraxConnected = fanTrax.IsConnected(UserAuth);
        }

        public void OnGet()
        {
        }

        public IActionResult OnPostYahooDisconnect()
        {
            sharedDb.ClearYahooAuth(userManager.GetUserId(User));

            return RedirectToPage("./Import");
        }

        public IActionResult OnPostYahoo()
        {
            if (YahooCode != null && YahooCode.Length > 0)
            {
                string outAccessToken = "";
                string outRefreshToken = "";
                if (yahoo.GetAccessToken(YahooCode, ref outAccessToken, ref outRefreshToken))
                {
                    var newAuth = sharedDb.AddYahooUserAuth(userManager.GetUserId(User), outAccessToken, outRefreshToken);
                    AddMessage("You have successfully authorized with Yahoo!");
                    return RedirectToPage("./Import");
                }
            }

            AddErrorMessage("An error occurred authorizing Yahoo!");
            return RedirectToPage("./Import");
        }

        public async Task<IActionResult> OnPostCustomLeagueAsync()
        {
            //var providerPlayers = db.GetFantasyProviderPlayers(db.GetFantasyProvider("yahoo"));
            //if (UserAuth.MustRefreshYahoo)
            //{
            //    string outAccessToken = "";
            //    string outRefreshToken = "";
            //    if (yahoo.RefreshAccessToken(UserAuth.YahooRefreshToken, ref outAccessToken, ref outRefreshToken))
            //    {
            //        UserAuth = sharedDb.AddYahooUserAuth(UserAuth.UserId, outAccessToken, outRefreshToken);
            //    }
            //}
            //league = sharedDb.ImportUserLeague(UserAuth, db.GetDefaultSeason(), id, db.GetActiveRosterSpots(), db.GetCategories(), logger);
            //try
            //{
            //    var missingPlayers = new List<UserLeagueMissingPlayer>();
            //    league.UserLeagueTeams = sharedDb.GetUserLeagueTeams(UserAuth, db.GetDefaultSeason().YahooId, league, providerPlayers, missingPlayers, logger);
            //}
            //catch
            //{
            //}

            var newUserLeague = await db.GetNewCustomUserLeagueAsync();
            newUserLeague.UserId = UserId;
            if (newUserLeague != null)
                await db.AddUserLeagueAsync(newUserLeague);
            //Draft draft = sharedDb.ImportDraft(UserAuth, league, providerPlayers, db.GetDefaultSeason().YahooId, logger);
            //db.AddDraft(draft);
            //AddMessage("You have imported the Yahoo! league " + league.Title);

            return RedirectToPage("./Index");
        }

        public IActionResult OnGetYahooList()
        {
            if (IsYahooConnected)
            {
                try
                {
                    if (UserAuth.MustRefreshYahoo)
                    {
                        string outAccessToken = "";
                        string outRefreshToken = "";
                        if (yahoo.RefreshAccessToken(UserAuth.YahooRefreshToken, ref outAccessToken, ref outRefreshToken))
                        {
                            sharedDb.AddYahooUserAuth(UserAuth.UserId, outAccessToken, outRefreshToken);
                        }
                    }
                    YahooUserLeagueTableModel = new UserLeagueTableModel();
                    string xml = sharedDb.GetLeaguesXml(UserAuth, db.GetDefaultSeason().YahooId);
                    YahooUserLeagueTableModel.ProviderUserLeagues = sharedDb.GetLeagues(xml);
                    YahooUserLeagueTableModel.CurrentUserLeagues = SelectedUserLeagues;
                    YahooUserLeagueTableModel.FantasyProvider = db.GetFantasyProvider("yahoo");
                    YahooUserLeagueTableModel.ShowMyTeam = false;
                }
                catch (Exception ex)
                {
                    AddErrorMessage("An error occurred reading your Yahoo! leagues [" + ex.Message + "]");
                }
            }

            return Page();
        }

        public IActionResult OnGetESPNList()
        {
            if (IsESPNConnected)
            {
                try
                {
                    ESPNUserLeagueTableModel = new UserLeagueTableModel();
                    ESPNUserLeagueTableModel.ProviderUserLeagues = espn.GetLeagues(UserAuth.ESPNswid, db.Sport);
                    ESPNUserLeagueTableModel.CurrentUserLeagues = SelectedUserLeagues;
                    ESPNUserLeagueTableModel.FantasyProvider = db.GetFantasyProvider("espn");
                    ESPNUserLeagueTableModel.ShowMyTeam = false;
                }
                catch (Exception ex)
                {
                    AddErrorMessage("An error occurred reading your ESPN leagues [" + ex.Message + "]");
                }
            }

            return Page();
        }

        public IActionResult OnPostESPN()
        {
            if (ESPNSWID != null && ESPNS2 != null && ESPNSWID.Length > 0)
            {
                var newAuth = sharedDb.AddESPNUserAuth(userManager.GetUserId(User), ESPNSWID.Trim(), ESPNS2.Trim());

                AddMessage("You have successfully authorized with ESPN.");
                return RedirectToPage("./Import");
            }

            //if (ESPNSWID != null && ESPNS2 != null && ESPNSWID.Length > 0)
            //{
            //    ESPNLib espn = new ESPNLib();
            //    string swid = "";
            //    string s2 = "";
            //    if (espn.LoginESPN(ESPNSWID, ESPNSWID, ref swid, ref s2))
            //    {
            //        var newAuth = sharedDb.AddESPNUserAuth(userManager.GetUserId(User), swid, s2);

            //        AddMessage("You have successfully authorized with ESPN.");
            //        return RedirectToPage("./Import");
            //    }
            //}

            AddErrorMessage("An error occurred authorizing ESPN.");
            return RedirectToPage("./Import");
        }

        public IActionResult OnPostESPNDisconnect()
        {
            sharedDb.ClearESPNAuth(userManager.GetUserId(User));
            return RedirectToPage("./Import");
        }

        public IActionResult OnPostFanTrax()
        {
            if (FanTraxEmail != null && FanTraxEmail.Length > 0)
            {
                string email = FanTraxEmail.Trim();
                FanTraxLib lib = new FanTraxLib(config, logger);
                if (lib.IsEmailValid(email))
                {
                    sharedDb.AddFanTraxUserAuth(userManager.GetUserId(User), FanTraxEmail.Trim());
                    AddMessage("You have successfully authorized with FanTrax.");
                    return RedirectToPage("./Import");
                }
            }

            AddErrorMessage("An error occurred authorizing FanTrax.");
            return RedirectToPage("./Import");
        }

        public IActionResult OnPostFanTraxDisconnect()
        {
            sharedDb.ClearFanTraxAuth(userManager.GetUserId(User));
            return Page();
        }

        public IActionResult OnGetFanTraxList()
        {
            if (IsFanTraxConnected)
            {
                FanTraxUserLeagueTableModel = new UserLeagueTableModel();
                string json = fanTrax.GetLeaguesJson(UserAuth.FanTraxEmail);
                FanTraxUserLeagueTableModel.ProviderUserLeagues = fanTrax.GetLeagues(json, db.Sport.Title);
                FanTraxUserLeagueTableModel.CurrentUserLeagues = SelectedUserLeagues;
                FanTraxUserLeagueTableModel.FantasyProvider = db.GetFantasyProvider("fantrax");
                FanTraxUserLeagueTableModel.ShowMyTeam = true;
            }

            return Page();
        }

        public async Task<IActionResult> OnPostImportFanTraxAsync()
        {
            return await OnGetImportLeagueAsync(FanTraxLeagueId, "FanTrax");
        }

        public async Task<IActionResult> OnGetImportLeagueAsync(string id, string provider)
        {
            UserLeague league = null;

            if (provider == "Yahoo!")
            {
                var providerPlayers = db.GetFantasyProviderPlayers(db.GetFantasyProvider("yahoo"));
                if (UserAuth.MustRefreshYahoo)
                {
                    string outAccessToken = "";
                    string outRefreshToken = "";
                    if (yahoo.RefreshAccessToken(UserAuth.YahooRefreshToken, ref outAccessToken, ref outRefreshToken))
                    {
                        UserAuth = sharedDb.AddYahooUserAuth(UserAuth.UserId, outAccessToken, outRefreshToken);
                    }
                }
                league = sharedDb.ImportUserLeague(UserAuth, db.GetDefaultSeason(), id, db.GetActiveRosterSpots(), db.GetCategories(), logger);
                try
                {
                    var missingPlayers = new List<UserLeagueMissingPlayer>();
                    league.UserLeagueTeams = sharedDb.GetUserLeagueTeams(UserAuth, db.GetDefaultSeason().YahooId, league, providerPlayers, missingPlayers, logger);
                }
                catch
                {
                }
                await db.AddUserLeagueAsync(league);
                Draft draft = sharedDb.ImportDraft(UserAuth, league, providerPlayers, db.GetDefaultSeason().YahooId, logger);
                await db.AddDraftAsync(draft);
                AddMessage("You have imported the Yahoo! league " + league.Title);
            }

            else if (provider == "FanTrax")
            {
                var providerPlayers = db.GetFantasyProviderPlayers(db.GetFantasyProvider("fantrax"));
                league = fanTrax.ImportUserLeague(UserAuth, db.GetDefaultSeason(), id, "", db.GetActiveRosterSpots(), db.GetCategories());
                var missingPlayers = new List<UserLeagueMissingPlayer>();
                league.UserLeagueTeams = fanTrax.GetUserLeagueTeams(UserAuth, db.Sport.Title, league, providerPlayers, missingPlayers);
                if (UserAuth.FanTraxEmail.Length>0)
                {
                    var allLeaguesJson = fanTrax.GetLeaguesJson(UserAuth.FanTraxEmail);
                    var allLeagues = fanTrax.GetLeagues(allLeaguesJson, db.Sport.Title);
                    var matchLeague = (from l in allLeagues where l.ProviderLeagueId==league.ProviderLeagueId select l).FirstOrDefault();
                    if (matchLeague!=null)
                        league.MyProviderTeamId=matchLeague.MyProviderTeamId;
                }
                if (league.MyProviderTeamId.Length==0 && league.UserLeagueTeams.Count > 0)
                    league.MyProviderTeamId = league.UserLeagueTeams.First().ProviderId;

                await db.AddUserLeagueAsync(league);
                Draft draft = fanTrax.ImportDraft(UserAuth, league, providerPlayers);
                await db.AddDraftAsync(draft);
                AddMessage("You have imported the FanTrax league " + league.Title + ". Make sure to edit the league to set your team.");
            }

            else if (provider == "ESPN")
            {
                var providerPlayers = db.GetFantasyProviderPlayers(db.GetFantasyProvider("espn"));
                league = espn.ImportUserLeague(db.Sport, UserAuth, db.GetDefaultSeason(), id, db.GetActiveRosterSpots(), db.GetCategories());
                var missingPlayers = new List<UserLeagueMissingPlayer>();
                league.UserLeagueTeams = espn.GetUserLeagueTeams(UserAuth, db.Sport, db.GetDefaultSeason(), league, providerPlayers, db.GetPlayers(), missingPlayers);
                await db.AddUserLeagueAsync(league);
                // Draft draft = fanTrax.ImportDraft(UserAuth, league, providerPlayers);
                // db.AddDraft(draft);
                AddMessage("You have imported the ESPN league " + league.Title);
            }

            return RedirectToPage("./Index", provider.Replace("!", "") + "List");
        }

    }
}
