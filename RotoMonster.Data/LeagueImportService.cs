using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using RotoMonster.Core;
using RotoMonster.Core.Libs;
using RotoMonsterExternalAPIs.Client.Models.Providers;
using RotoMonsterExternalAPIs.Client.Services.Providers;
using RotoMonsterExternalAPIs.Client.Services.Yahoo;

namespace RotoMonster.Data
{
    /// <summary>
    /// Imports leagues from a provider.
    ///
    /// Sits between the page and the provider layer so the page model does not
    /// have to know about batching, mapping or which provider it is talking
    /// to. Importing one league and importing twenty go through the same
    /// path - the only difference is the length of the list.
    /// </summary>
    public class LeagueImportService
    {
        private readonly IRMData _db;
        private readonly IRMSharedData _sharedDb;
        private readonly IConfiguration _config;

        public LeagueImportService(IRMData db, IRMSharedData sharedDb, IConfiguration config)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _sharedDb = sharedDb ?? throw new ArgumentNullException(nameof(sharedDb));
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        // -------------------------------------------------------------------
        // Listing
        // -------------------------------------------------------------------

        /// <summary>
        /// Every league the user has with this provider, marked with whether
        /// it is already imported.
        /// </summary>
        public async Task<LeagueListResult> ListAsync(string userId, string providerName)
        {
            var result = new LeagueListResult { ProviderName = providerName };

            List<ProviderLeague> providerLeagues;

            if (Normalize(providerName) == "fantrax")
            {
                // Fantrax has no implementation behind the layer, and would
                // gain nothing from one - its API is a call per league either
                // way. Its existing lib does the listing, so the page behaves
                // the same without any parsing being rewritten.
                providerLeagues = ListFanTraxLeagues(userId);
            }
            else
            {
                var provider = GetProvider(providerName);
                if (provider == null)
                {
                    result.ErrorMessage = providerName + " is not set up yet.";
                    return result;
                }

                var leagues = await provider.GetLeaguesAsync(userId, SeasonKeyFor(providerName))
                    .ConfigureAwait(false);

                if (!leagues.Success)
                {
                    result.ErrorMessage = leagues.ErrorMessage;
                    result.NeedsReauthorization = leagues.NeedsReauthorization;
                    return result;
                }

                providerLeagues = leagues.Leagues;
            }

            var existing = await _db.GetUserLeaguesAsync(userId).ConfigureAwait(false);
            var fantasyProvider = _db.GetFantasyProvider(providerName);

            // Keyed by provider league id so the Remove action has the RM id
            // to delete without another lookup.
            var importedByProviderId = new Dictionary<string, UserLeague>(StringComparer.OrdinalIgnoreCase);
            foreach (var league in existing)
            {
                if (fantasyProvider != null && league.FantasyProviderId != fantasyProvider.Id) continue;
                if (string.IsNullOrEmpty(league.ProviderLeagueId)) continue;
                if (importedByProviderId.ContainsKey(league.ProviderLeagueId)) continue;

                importedByProviderId[league.ProviderLeagueId] = league;
            }

            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var league in providerLeagues)
            {
                var listed = new ListedLeague
                {
                    LeagueId = league.LeagueId,
                    ProviderLeagueId = league.LeagueId,
                    Title = league.Title,
                    MyTeamTitle = league.MyTeamTitle
                };

                UserLeague imported;
                if (importedByProviderId.TryGetValue(league.LeagueId, out imported))
                {
                    listed.IsImported = true;
                    listed.UserLeagueId = imported.Id;
                    listed.TrackLeague = imported.TrackLeague;

                    // The provider's league list does not always say which team
                    // is the user's, but the imported copy already knows.
                    if (string.IsNullOrEmpty(listed.MyTeamTitle))
                        listed.MyTeamTitle = imported.MyTeamTitle;
                }

                seen.Add(league.LeagueId);
                result.Leagues.Add(listed);
            }

            // Anything in RM that the provider did not hand back. Usually a
            // league deleted on their end - it still exists here, and hiding it
            // would leave the user no way to remove or edit it.
            foreach (var pair in importedByProviderId)
            {
                if (seen.Contains(pair.Key)) continue;

                result.Leagues.Add(new ListedLeague
                {
                    LeagueId = pair.Key,
                    ProviderLeagueId = pair.Key,
                    Title = pair.Value.DisplayTitle ?? pair.Value.Title,
                    MyTeamTitle = pair.Value.MyTeamTitle,
                    IsImported = true,
                    UserLeagueId = pair.Value.Id,
                    TrackLeague = pair.Value.TrackLeague,
                    NotAtProvider = true
                });
            }

            result.Success = true;
            return result;
        }

        /// <summary>
        /// Leagues that came from no provider, or from one RM no longer knows
        /// about. Without this they would exist in the database and appear
        /// nowhere on the page.
        /// </summary>
        public async Task<LeagueListResult> ListCustomAsync(string userId, IEnumerable<string> providerNames)
        {
            var result = new LeagueListResult { ProviderName = "Custom", Success = true };

            var knownProviderIds = new HashSet<int>();
            foreach (var name in providerNames)
            {
                var provider = _db.GetFantasyProvider(name);
                if (provider != null) knownProviderIds.Add(provider.Id);
            }

            var existing = await _db.GetUserLeaguesAsync(userId).ConfigureAwait(false);

            foreach (var league in existing)
            {
                var belongsToKnownProvider = knownProviderIds.Contains(league.FantasyProviderId)
                                             && !string.IsNullOrEmpty(league.ProviderLeagueId);

                if (belongsToKnownProvider) continue;

                result.Leagues.Add(new ListedLeague
                {
                    LeagueId = league.ProviderLeagueId ?? "",
                    ProviderLeagueId = league.ProviderLeagueId ?? "",
                    Title = league.DisplayTitle ?? league.Title,
                    MyTeamTitle = league.MyTeamTitle,
                    IsImported = true,
                    UserLeagueId = league.Id,
                    TrackLeague = league.TrackLeague
                });
            }

            return result;
        }

        // -------------------------------------------------------------------
        // Importing
        // -------------------------------------------------------------------

        /// <summary>
        /// Imports the given leagues. One league failing does not stop the
        /// rest - each gets its own entry in the result.
        /// </summary>
        public async Task<LeagueImportResult> ImportAsync(
            string userId,
            string providerName,
            IList<string> leagueIds)
        {
            var result = new LeagueImportResult { ProviderName = providerName };

            if (leagueIds == null || leagueIds.Count == 0)
            {
                result.ErrorMessage = "No leagues were selected.";
                return result;
            }

            var isFanTrax = Normalize(providerName) == "fantrax";
            var provider = isFanTrax ? null : GetProvider(providerName);
            var fantasyProvider = _db.GetFantasyProvider(providerName);

            if ((provider == null && !isFanTrax) || fantasyProvider == null)
            {
                result.ErrorMessage = providerName + " is not set up yet.";
                return result;
            }

            // Skip anything already imported rather than creating a duplicate.
            var existing = await _db.GetUserLeaguesAsync(userId).ConfigureAwait(false);
            var alreadyImported = new HashSet<string>(
                existing.Where(l => l.FantasyProviderId == fantasyProvider.Id)
                        .Select(l => l.ProviderLeagueId ?? ""),
                StringComparer.OrdinalIgnoreCase);

            var toImport = leagueIds.Where(id => !alreadyImported.Contains(id)).ToList();

            foreach (var id in leagueIds.Where(alreadyImported.Contains))
            {
                result.Leagues.Add(new ImportedLeague
                {
                    LeagueId = id,
                    Skipped = true,
                    Message = "Already imported."
                });
            }

            if (toImport.Count == 0)
                return Finish(result);

            if (isFanTrax)
            {
                ImportFanTrax(userId, fantasyProvider, toImport, result);
                return Finish(result);
            }

            var data = await provider.GetLeagueDataAsync(
                userId,
                SeasonKeyFor(providerName),
                toImport,
                ProviderLeagueDataParts.All).ConfigureAwait(false);

            result.RequestCount = data.RequestCount;

            if (!data.Success)
            {
                result.ErrorMessage = data.ErrorMessage;
                result.NeedsReauthorization = data.NeedsReauthorization;
                return result;
            }

            // Read once for the whole batch. The old import re-read the
            // provider player table for every league, which is what made
            // twenty leagues unworkable as much as the API calls did.
            var season = _db.GetDefaultSeason();
            var mapper = new ProviderImportMapper(
                fantasyProvider,
                season,
                _db.GetCategories(),
                _db.GetActiveRosterSpots(),
                _db.GetFantasyProviderPlayers(fantasyProvider));

            foreach (var leagueData in data.Leagues)
            {
                var entry = new ImportedLeague { LeagueId = leagueData.LeagueId };

                if (leagueData.HasError)
                {
                    entry.Message = leagueData.ErrorMessage;
                    result.Leagues.Add(entry);
                    continue;
                }

                try
                {
                    var mapping = mapper.MapLeague(userId, leagueData);

                    if (!mapping.Success)
                    {
                        entry.Message = mapping.Warnings.Count > 0
                            ? string.Join(" ", mapping.Warnings)
                            : "Could not read this league.";
                        result.Leagues.Add(entry);
                        continue;
                    }

                    var saved = await _db.AddUserLeagueAsync(mapping.UserLeague).ConfigureAwait(false);

                    // The draft is a separate record keyed by provider league
                    // id, not by UserLeagueId, so it is saved independently.
                    if (mapping.Draft != null && mapping.Draft.DraftPlayers.Count > 0)
                        await _db.AddDraftAsync(mapping.Draft).ConfigureAwait(false);

                    entry.Imported = true;
                    entry.Title = mapping.UserLeague.Title;
                    entry.Warnings = mapping.Warnings;
                    entry.MissingPlayerCount = mapping.MissingPlayers.Count;
                }
                catch (Exception ex)
                {
                    // One bad league should not lose the other nineteen.
                    entry.Message = ex.Message;
                }

                result.Leagues.Add(entry);
            }

            return Finish(result);
        }

        // -------------------------------------------------------------------
        // Roster refresh
        // -------------------------------------------------------------------

        /// <summary>
        /// Refreshes rosters for every tracked league with this provider.
        ///
        /// The old path refreshed one league per call, so twenty leagues meant
        /// twenty round trips to Yahoo. This asks for all of them at once.
        /// </summary>
        public async Task<RosterRefreshResult> RefreshRostersAsync(string userId, string providerName)
        {
            var result = new RosterRefreshResult { ProviderName = providerName };

            var provider = GetProvider(providerName);
            var fantasyProvider = _db.GetFantasyProvider(providerName);

            if (provider == null || fantasyProvider == null)
            {
                result.ErrorMessage = providerName + " is not set up yet.";
                return result;
            }

            var tracked = await _db.GetTrackedUserLeaguesAsync(userId).ConfigureAwait(false);

            var leagues = tracked
                .Where(l => l.FantasyProviderId == fantasyProvider.Id
                            && !string.IsNullOrEmpty(l.ProviderLeagueId))
                .ToList();

            if (leagues.Count == 0)
            {
                result.Success = true;
                return result;
            }

            var data = await provider.GetLeagueDataAsync(
                userId,
                SeasonKeyFor(providerName),
                leagues.Select(l => l.ProviderLeagueId).ToList(),
                ProviderLeagueDataParts.Rosters).ConfigureAwait(false);

            result.RequestCount = data.RequestCount;

            if (!data.Success)
            {
                result.ErrorMessage = data.ErrorMessage;
                result.NeedsReauthorization = data.NeedsReauthorization;
                return result;
            }

            var mapper = new ProviderImportMapper(
                fantasyProvider,
                _db.GetDefaultSeason(),
                _db.GetCategories(),
                _db.GetActiveRosterSpots(),
                _db.GetFantasyProviderPlayers(fantasyProvider));

            var byProviderId = new Dictionary<string, UserLeague>(StringComparer.OrdinalIgnoreCase);
            foreach (var league in leagues)
            {
                if (!byProviderId.ContainsKey(league.ProviderLeagueId))
                    byProviderId[league.ProviderLeagueId] = league;
            }

            foreach (var leagueData in data.Leagues)
            {
                UserLeague league;
                if (!byProviderId.TryGetValue(leagueData.LeagueId ?? "", out league)) continue;

                var entry = new RefreshedLeague
                {
                    LeagueId = leagueData.LeagueId,
                    Title = league.DisplayTitle ?? league.Title
                };

                if (leagueData.HasError)
                {
                    entry.Message = leagueData.ErrorMessage;
                    result.Leagues.Add(entry);
                    continue;
                }

                try
                {
                    var mapping = mapper.MapRosters(leagueData);

                    // No teams back is treated as a failure, not an empty
                    // roster. Writing it through would wipe the league.
                    if (!mapping.Success)
                    {
                        entry.Message = "No teams came back, so this league was left alone.";
                        result.Leagues.Add(entry);
                        continue;
                    }

                    _db.UpdateUserLeagueTeams(
                        league.Id,
                        mapping.Teams,
                        mapping.MissingPlayers,
                        _db.GetUserLeagueWaiverPlayers(league));

                    entry.Refreshed = true;
                    entry.MissingPlayerCount = mapping.MissingPlayers.Count;
                }
                catch (Exception ex)
                {
                    // One league failing should not stop the others.
                    entry.Message = ex.Message;
                }

                result.Leagues.Add(entry);
            }

            result.Success = true;
            return result;
        }

        private static LeagueImportResult Finish(LeagueImportResult result)
        {
            result.Success = true;
            return result;
        }

        // -------------------------------------------------------------------
        // FanTrax
        // -------------------------------------------------------------------
        //
        // FanTrax keeps using FanTraxLib rather than moving behind
        // IFantasyProvider. Two reasons. Its API is a request per league, so
        // there is no batching to win - the thing that made the layer worth
        // building for Yahoo does not apply. And its parsing carries a lot of
        // hard won detail: a deeply nested category walk, NFL points against
        // ranges, an Ohtani hitter and pitcher split, an extra call just to
        // read the current period. Rewriting that would risk working imports
        // to gain tidiness.
        //
        // The page does not care. Listing and importing go through here either
        // way, so FanTrax gets the same checkboxes and the same one button.

        private List<ProviderLeague> ListFanTraxLeagues(string userId)
        {
            var leagues = new List<ProviderLeague>();

            var userAuth = _sharedDb.GetUserAuth(userId);
            if (userAuth == null || string.IsNullOrEmpty(userAuth.FanTraxEmail))
                return leagues;

            var lib = new FanTraxLib(_config, null);
            var json = lib.GetLeaguesJson(userAuth.FanTraxEmail);

            if (string.IsNullOrEmpty(json))
                return leagues;

            foreach (var league in lib.GetLeagues(json, _db.Sport.Title))
            {
                leagues.Add(new ProviderLeague
                {
                    LeagueId = league.ProviderLeagueId,
                    Title = league.Title,
                    MyTeamId = league.MyProviderTeamId,
                    MyTeamTitle = league.MyTeamTitle
                });
            }

            return leagues;
        }

        private void ImportFanTrax(
            string userId,
            FantasyProvider fantasyProvider,
            List<string> leagueIds,
            LeagueImportResult result)
        {
            var userAuth = _sharedDb.GetUserAuth(userId);
            if (userAuth == null)
            {
                result.ErrorMessage = "There is no FanTrax authorization on your account.";
                return;
            }

            var lib = new FanTraxLib(_config, null);
            var season = _db.GetDefaultSeason();

            // Read once for the batch rather than per league, the same saving
            // the Yahoo path makes.
            var providerPlayers = _db.GetFantasyProviderPlayers(fantasyProvider);
            var rosterSpots = _db.GetActiveRosterSpots();
            var categories = _db.GetCategories();

            // The league list carries the user's own team, which the per league
            // import does not return. Fetched once so it can be filled in
            // afterwards rather than per league.
            var allLeagues = new List<UserLeague>();
            if (!string.IsNullOrEmpty(userAuth.FanTraxEmail))
            {
                var json = lib.GetLeaguesJson(userAuth.FanTraxEmail);
                if (!string.IsNullOrEmpty(json))
                    allLeagues = lib.GetLeagues(json, _db.Sport.Title);
            }

            foreach (var leagueId in leagueIds)
            {
                var entry = new ImportedLeague { LeagueId = leagueId };

                try
                {
                    // The settings call does not return the league name, only
                    // the list does, so it is passed in rather than leaving the
                    // league stored as "FanTrax <id>".
                    var known = allLeagues.FirstOrDefault(l => l.ProviderLeagueId == leagueId);
                    var title = known != null ? known.Title : "";

                    var league = lib.ImportUserLeague(userAuth, season, leagueId, title,
                        rosterSpots, categories);

                    if (league == null)
                    {
                        entry.Message = "FanTrax did not return settings for this league.";
                        result.Leagues.Add(entry);
                        continue;
                    }

                    var missingPlayers = new List<UserLeagueMissingPlayer>();
                    league.UserLeagueTeams = lib.GetUserLeagueTeams(userAuth, _db.Sport.Title,
                        league, providerPlayers, missingPlayers);

                    if (known != null)
                    {
                        league.MyProviderTeamId = known.MyProviderTeamId;
                        league.MyTeamTitle = known.MyTeamTitle;
                    }

                    // Falling back to the first team keeps the league usable
                    // rather than leaving it with no team set at all.
                    if (string.IsNullOrEmpty(league.MyProviderTeamId) && league.UserLeagueTeams.Count > 0)
                    {
                        league.MyProviderTeamId = league.UserLeagueTeams.First().ProviderId;
                        entry.Warnings.Add("Could not tell which team is yours, so the first was used.");
                    }

                    _db.AddUserLeagueAsync(league).GetAwaiter().GetResult();

                    var draft = lib.ImportDraft(userAuth, league, providerPlayers);
                    if (draft != null)
                        _db.AddDraftAsync(draft).GetAwaiter().GetResult();

                    entry.Imported = true;
                    entry.Title = league.DisplayTitle ?? league.Title;
                    entry.MissingPlayerCount = missingPlayers.Count;
                }
                catch (Exception ex)
                {
                    // One league failing should not lose the rest.
                    entry.Message = ex.Message;
                }

                result.Leagues.Add(entry);
            }
        }

        // -------------------------------------------------------------------
        // Providers
        // -------------------------------------------------------------------

        /// <summary>
        /// Returns null for a provider that has no implementation yet, which
        /// is how the page knows to leave that tab out.
        /// </summary>
        private IFantasyProvider GetProvider(string providerName)
        {
            switch (Normalize(providerName))
            {
                case "yahoo":
                    var client = new YahooApiClient(
                        new YahooOAuth(_config["YahooClientId"], _config["YahooClientSecret"]),
                        (IYahooTokenStore)_sharedDb);
                    return new YahooFantasyProvider(client);

                default:
                    return null;
            }
        }

        /// <summary>
        /// The provider's own season identifier. Yahoo wants its game key,
        /// ESPN its year, and Fantrax nothing at all.
        /// </summary>
        private string SeasonKeyFor(string providerName)
        {
            var season = _db.GetDefaultSeason();
            if (season == null) return "";

            switch (Normalize(providerName))
            {
                case "yahoo":
                    return season.YahooId ?? "";
                case "espn":
                    return season.ESPNYear ?? "";
                default:
                    return "";
            }
        }

        private static string Normalize(string providerName)
        {
            return (providerName ?? "").Replace("!", "").Trim().ToLowerInvariant();
        }
    }

    public class RosterRefreshResult
    {
        public bool Success { get; set; }

        public string ProviderName { get; set; }

        public string ErrorMessage { get; set; }

        public bool NeedsReauthorization { get; set; }

        /// <summary>
        /// What the batching bought. One or two rather than one per league.
        /// </summary>
        public int RequestCount { get; set; }

        public List<RefreshedLeague> Leagues { get; set; } = new List<RefreshedLeague>();

        public int RefreshedCount
        {
            get { return Leagues.Count(l => l.Refreshed); }
        }

        public int FailedCount
        {
            get { return Leagues.Count(l => !l.Refreshed); }
        }
    }

    public class RefreshedLeague
    {
        public string LeagueId { get; set; }

        public string Title { get; set; }

        public bool Refreshed { get; set; }

        public string Message { get; set; }

        public int MissingPlayerCount { get; set; }
    }

    public class LeagueListResult
    {
        public bool Success { get; set; }

        public string ProviderName { get; set; }

        public string ErrorMessage { get; set; }

        /// <summary>
        /// The provider rejected the credentials, so the user needs to connect
        /// again rather than retry.
        /// </summary>
        public bool NeedsReauthorization { get; set; }

        public List<ListedLeague> Leagues { get; set; } = new List<ListedLeague>();

        public int ImportedCount
        {
            get { return Leagues.Count(l => l.IsImported); }
        }
    }

    public class ListedLeague
    {
        public string LeagueId { get; set; }

        public string Title { get; set; }

        public string MyTeamTitle { get; set; }

        public bool IsImported { get; set; }

        /// <summary>
        /// RM's own id for the imported league, so it can be removed without
        /// a second lookup. Zero when not imported.
        /// </summary>
        public int UserLeagueId { get; set; }

        /// <summary>
        /// Whether RM is tracking this league. Only meaningful once imported.
        /// </summary>
        public bool TrackLeague { get; set; }

        /// <summary>
        /// Imported into RM but no longer in the provider's list, usually
        /// because it was deleted on their end. Shown rather than hidden, or
        /// it would be stuck in RM with no way to reach it.
        /// </summary>
        public bool NotAtProvider { get; set; }

        /// <summary>
        /// The provider's own league id, which users need when contacting
        /// support or matching a league up by hand.
        /// </summary>
        public string ProviderLeagueId { get; set; }
    }

    public class LeagueImportResult
    {
        public bool Success { get; set; }

        public string ProviderName { get; set; }

        public string ErrorMessage { get; set; }

        public bool NeedsReauthorization { get; set; }

        /// <summary>
        /// Only for logging, but it is the number the whole batching effort
        /// exists to keep down.
        /// </summary>
        public int RequestCount { get; set; }

        public List<ImportedLeague> Leagues { get; set; } = new List<ImportedLeague>();

        public int ImportedCount
        {
            get { return Leagues.Count(l => l.Imported); }
        }

        public int FailedCount
        {
            get { return Leagues.Count(l => !l.Imported && !l.Skipped); }
        }
    }

    public class ImportedLeague
    {
        public string LeagueId { get; set; }

        public string Title { get; set; }

        public bool Imported { get; set; }

        /// <summary>
        /// Already in the user's list, so it was left alone.
        /// </summary>
        public bool Skipped { get; set; }

        /// <summary>
        /// Why it did not import. Empty when it did.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Imported, but something did not map cleanly - a category with no
        /// match, or a team we could not identify as the user's.
        /// </summary>
        public List<string> Warnings { get; set; } = new List<string>();

        public int MissingPlayerCount { get; set; }
    }
}
