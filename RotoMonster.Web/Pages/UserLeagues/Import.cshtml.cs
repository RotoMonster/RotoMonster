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
using RotoMonster.Models.Shared;
using RotoMonster.Data;

namespace RotoMonster.Pages.UserLeagues
{
    [Authorize]
    public class ImportModel : RMPageModel
    {
        public const string YahooProvider = "Yahoo!";
        public const string ESPNProvider = "ESPN";
        public const string FanTraxProvider = "FanTrax";
        public const string CustomProvider = "Custom";

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

        /// <summary>
        /// Which leagues the user ticked. Bound from the checkboxes, so a
        /// single import and a twenty league import are the same post.
        /// </summary>
        [BindProperty]
        public List<string> SelectedLeagueIds { get; set; } = new List<string>();

        public UserAuth UserAuth { get; set; }
        public Sport Sport { get; set; }

        public bool IsYahooConnected { get; set; }
        public string YahooUrl { get; set; }
        public bool IsESPNConnected { get; set; }
        public bool IsFanTraxConnected { get; set; }

        /// <summary>
        /// Which tab is showing. Kept in the query string so a postback can
        /// come back to the same one.
        /// </summary>
        public string ActiveTab { get; set; } = YahooProvider;

        public List<ImportTab> Tabs { get; set; } = new List<ImportTab>();

        /// <summary>
        /// Filled after an import so the page can report what happened per
        /// league rather than a single success or failure.
        /// </summary>
        public LeagueImportResult ImportResult { get; set; }

        private readonly YahooLib yahoo;
        private readonly FanTraxLib fanTrax;
        private readonly ESPNLib espn;
        private readonly LeagueImportService importService;

        public ImportModel(IRMData db, IRMSharedData sharedDb, IConfiguration config, UserManager<ApplicationUser> userManager, IHttpContextAccessor contextAccessor, ILogger<PageModel> logger)
            : base(config, db, sharedDb, userManager, contextAccessor, logger)
        {
            yahoo = new YahooLib(config, config["YahooClientId"], config["YahooClientSecret"], db.GetDefaultSeason().YahooId, logger);
            fanTrax = new FanTraxLib(config, logger);
            espn = new ESPNLib(config, logger);

            importService = new LeagueImportService(db, sharedDb, config);

            UserAuth = sharedDb.GetUserAuth(UserId);
            Sport = db.Sport;

            YahooUrl = yahoo.GetAuthorizationURL();

            IsYahooConnected = yahoo.IsConnected(UserAuth);
            IsESPNConnected = espn.IsConnected(UserAuth);
            IsFanTraxConnected = fanTrax.IsConnected(UserAuth);
        }

        // -------------------------------------------------------------------
        // Page load
        // -------------------------------------------------------------------

        public async Task OnGetAsync(string tab = null)
        {
            await BuildTabsAsync(tab);
        }

        /// <summary>
        /// Builds every tab, fetching leagues only for the providers that are
        /// connected. That is one call per connected provider, which is what
        /// lets the counts show without the user clicking into each one.
        /// </summary>
        private async Task BuildTabsAsync(string requestedTab)
        {
            Tabs.Clear();

            Tabs.Add(await BuildTabAsync(YahooProvider, IsYahooConnected, true));

            if (Sport.IsNBA || Sport.IsMLB || Sport.IsNFL)
                Tabs.Add(await BuildTabAsync(ESPNProvider, IsESPNConnected, true));

            Tabs.Add(await BuildTabAsync(FanTraxProvider, IsFanTraxConnected, true));

            // Leagues that belong to no provider would otherwise exist in the
            // database and show up nowhere.
            var custom = await importService.ListCustomAsync(
                UserId,
                new[] { YahooProvider, ESPNProvider, FanTraxProvider });

            if (custom.Leagues.Count > 0)
            {
                Tabs.Add(new ImportTab
                {
                    ProviderName = CustomProvider,
                    IsConnected = true,
                    SupportsBulkImport = true,
                    IsCustom = true,
                    Leagues = custom.Leagues
                });
            }

            // Fall back to the first tab rather than showing nothing when the
            // requested one does not exist.
            var match = Tabs.FirstOrDefault(t =>
                string.Equals(t.ProviderName, requestedTab, StringComparison.OrdinalIgnoreCase));

            ActiveTab = match != null
                ? match.ProviderName
                : (Tabs.FirstOrDefault(t => t.IsConnected) ?? Tabs.First()).ProviderName;
        }

        private async Task<ImportTab> BuildTabAsync(string providerName, bool isConnected, bool useBulkImport)
        {
            var tab = new ImportTab
            {
                ProviderName = providerName,
                IsConnected = isConnected,
                SupportsBulkImport = useBulkImport
            };

            if (!isConnected)
                return tab;

            var result = await importService.ListAsync(UserId, providerName);

            tab.Leagues = result.Leagues;
            tab.ErrorMessage = result.ErrorMessage;
            tab.NeedsReauthorization = result.NeedsReauthorization;

            return tab;
        }

        // -------------------------------------------------------------------
        // Bulk import
        // -------------------------------------------------------------------

        public async Task<IActionResult> OnPostImportSelectedAsync(string provider)
        {
            if (SelectedLeagueIds == null || SelectedLeagueIds.Count == 0)
            {
                AddErrorMessage("Pick at least one league to import.");
                await BuildTabsAsync(provider);
                return Page();
            }

            ImportResult = await importService.ImportAsync(UserId, provider, SelectedLeagueIds);

            if (!ImportResult.Success)
            {
                AddErrorMessage(ImportResult.ErrorMessage);
            }
            else
            {
                if (ImportResult.ImportedCount > 0)
                {
                    AddMessage("Imported " + ImportResult.ImportedCount
                               + (ImportResult.ImportedCount == 1 ? " league." : " leagues."));
                }

                if (ImportResult.FailedCount > 0)
                    AddErrorMessage(ImportResult.FailedCount + " could not be imported. See below.");
            }

            // Rebuilt rather than redirected, so the per league results stay on
            // screen instead of being lost to a fresh GET.
            await BuildTabsAsync(provider);
            return Page();
        }

        public async Task<IActionResult> OnPostToggleTrackAsync(int userLeagueId, string provider)
        {
            var leagues = await db.GetUserLeaguesAsync(UserId);
            var owned = leagues.FirstOrDefault(l => l.Id == userLeagueId);

            if (owned == null)
            {
                AddErrorMessage("That league could not be found.");
            }
            else
            {
                // Targeted update. UpdateUserLeagueAsync rebuilds the child
                // collections from what it is handed, which would wipe the
                // categories and roster spots this league already has.
                await db.SetUserLeagueTrackAsync(userLeagueId, !owned.TrackLeague);
                await db.CommitAsync();
            }

            return RedirectToPage("./Import", new { tab = provider });
        }

        public async Task<IActionResult> OnPostRemoveLeagueAsync(int userLeagueId, string provider)
        {
            if (userLeagueId > 0)
            {
                var leagues = await db.GetUserLeaguesAsync(UserId);
                var owned = leagues.FirstOrDefault(l => l.Id == userLeagueId);

                // Checked against the signed in user's own leagues, so an id
                // from somewhere else cannot delete someone else's league.
                if (owned == null)
                {
                    AddErrorMessage("That league could not be found.");
                }
                else
                {
                    await db.DeleteUserLeagueAsync(userLeagueId);
                    await db.CommitAsync();
                    AddMessage("Removed " + owned.Title + ".");
                }
            }

            return RedirectToPage("./Import", new { tab = provider });
        }

        // -------------------------------------------------------------------
        // Connecting
        // -------------------------------------------------------------------

        public IActionResult OnPostYahoo()
        {
            if (!string.IsNullOrEmpty(YahooCode))
            {
                string outAccessToken = "";
                string outRefreshToken = "";
                if (yahoo.GetAccessToken(YahooCode, ref outAccessToken, ref outRefreshToken))
                {
                    sharedDb.AddYahooUserAuth(userManager.GetUserId(User), outAccessToken, outRefreshToken);
                    AddMessage("You have successfully authorized with Yahoo!");
                    return RedirectToPage("./Import", new { tab = YahooProvider });
                }
            }

            AddErrorMessage("An error occurred authorizing Yahoo!");
            return RedirectToPage("./Import", new { tab = YahooProvider });
        }

        public IActionResult OnPostYahooDisconnect()
        {
            sharedDb.ClearYahooAuth(userManager.GetUserId(User));
            return RedirectToPage("./Import", new { tab = YahooProvider });
        }

        public IActionResult OnPostESPN()
        {
            if (!string.IsNullOrEmpty(ESPNSWID) && ESPNS2 != null)
            {
                sharedDb.AddESPNUserAuth(userManager.GetUserId(User), ESPNSWID.Trim(), ESPNS2.Trim());
                AddMessage("You have successfully authorized with ESPN.");
                return RedirectToPage("./Import", new { tab = ESPNProvider });
            }

            AddErrorMessage("An error occurred authorizing ESPN.");
            return RedirectToPage("./Import", new { tab = ESPNProvider });
        }

        public IActionResult OnPostESPNDisconnect()
        {
            sharedDb.ClearESPNAuth(userManager.GetUserId(User));
            return RedirectToPage("./Import", new { tab = ESPNProvider });
        }

        public IActionResult OnPostFanTrax()
        {
            if (!string.IsNullOrEmpty(FanTraxEmail))
            {
                var email = FanTraxEmail.Trim();
                var lib = new FanTraxLib(config, logger);
                if (lib.IsEmailValid(email))
                {
                    sharedDb.AddFanTraxUserAuth(userManager.GetUserId(User), email);
                    AddMessage("You have successfully authorized with FanTrax.");
                    return RedirectToPage("./Import", new { tab = FanTraxProvider });
                }
            }

            AddErrorMessage("An error occurred authorizing FanTrax.");
            return RedirectToPage("./Import", new { tab = FanTraxProvider });
        }

        public IActionResult OnPostFanTraxDisconnect()
        {
            sharedDb.ClearFanTraxAuth(userManager.GetUserId(User));
            return RedirectToPage("./Import", new { tab = FanTraxProvider });
        }

        // -------------------------------------------------------------------
        // Legacy single import, still used by ESPN and FanTrax
        // -------------------------------------------------------------------

        public async Task<IActionResult> OnPostImportFanTraxAsync()
        {
            return await OnGetImportLeagueAsync(FanTraxLeagueId, FanTraxProvider);
        }

        public async Task<IActionResult> OnGetImportLeagueAsync(string id, string provider)
        {
            UserLeague league = null;

            if (provider == YahooProvider)
            {
                // Yahoo goes through the import service now, which handles one
                // league and twenty the same way.
                var result = await importService.ImportAsync(UserId, YahooProvider, new List<string> { id });

                if (result.ImportedCount > 0)
                    AddMessage("Imported the Yahoo! league.");
                else
                    AddErrorMessage(result.ErrorMessage ?? "That league could not be imported.");

                return RedirectToPage("./Import", new { tab = YahooProvider });
            }

            if (provider == FanTraxProvider)
            {
                // Same path the checkboxes use, so the manual box cannot drift
                // from the list.
                var fanTraxResult = await importService.ImportAsync(UserId, FanTraxProvider,
                    new List<string> { id });

                if (fanTraxResult.ImportedCount > 0)
                    AddMessage("Imported the FanTrax league.");
                else
                    AddErrorMessage(fanTraxResult.ErrorMessage ?? "That league could not be imported.");

                return RedirectToPage("./Import", new { tab = FanTraxProvider });
            }

            var importResult = await importService.ImportAsync(UserId, provider,
                new List<string> { id });

            if (importResult.ImportedCount > 0)
                AddMessage("Imported the league.");
            else
                AddErrorMessage(importResult.ErrorMessage ?? "That league could not be imported.");

            return RedirectToPage("./Import", new { tab = provider });
        }

        // -------------------------------------------------------------------
        // Custom league
        // -------------------------------------------------------------------

        public async Task<IActionResult> OnPostCustomLeagueAsync()
        {
            var newUserLeague = await db.GetNewCustomUserLeagueAsync();
            if (newUserLeague != null)
            {
                newUserLeague.UserId = UserId;
                await db.AddUserLeagueAsync(newUserLeague);
            }

            return RedirectToPage("./Index");
        }
    }

    /// <summary>
    /// One provider's tab. Holds either the new listing or the legacy table,
    /// never both, depending on whether that provider has been moved over to
    /// the provider layer yet.
    /// </summary>
    public class ImportTab
    {
        public string ProviderName { get; set; }

        public bool IsConnected { get; set; }

        /// <summary>
        /// Every provider lists and imports the same way now, whether it runs
        /// through the provider layer or its own lib. Kept so the Custom tab,
        /// which imports from nothing, can still say so.
        /// </summary>
        public bool SupportsBulkImport { get; set; }

        /// <summary>
        /// The catch all tab. Nothing here can be imported, since these
        /// leagues have no provider to import from.
        /// </summary>
        public bool IsCustom { get; set; }

        public List<ListedLeague> Leagues { get; set; } = new List<ListedLeague>();

        public string ErrorMessage { get; set; }

        public bool NeedsReauthorization { get; set; }

        public int TotalCount
        {
            get { return Leagues.Count; }
        }

        public int ImportedCount
        {
            get { return SupportsBulkImport ? Leagues.Count(l => l.IsImported) : 0; }
        }

        public int TrackedCount
        {
            get { return Leagues.Count(l => l.IsImported && l.TrackLeague); }
        }

        /// <summary>
        /// Anchor-safe id for the tab button and panel.
        /// </summary>
        public string Slug
        {
            get { return (ProviderName ?? "").Replace("!", "").Replace(" ", "").ToLowerInvariant(); }
        }
    }
}
