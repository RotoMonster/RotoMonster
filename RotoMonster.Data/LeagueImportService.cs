using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using RotoMonster.Core;
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

            foreach (var league in leagues.Leagues)
            {
                var listed = new ListedLeague
                {
                    LeagueId = league.LeagueId,
                    Title = league.Title,
                    MyTeamTitle = league.MyTeamTitle
                };

                UserLeague imported;
                if (importedByProviderId.TryGetValue(league.LeagueId, out imported))
                {
                    listed.IsImported = true;
                    listed.UserLeagueId = imported.Id;

                    // The provider's league list does not always say which team
                    // is the user's, but the imported copy already knows.
                    if (string.IsNullOrEmpty(listed.MyTeamTitle))
                        listed.MyTeamTitle = imported.MyTeamTitle;
                }

                result.Leagues.Add(listed);
            }

            result.Success = true;
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

            var provider = GetProvider(providerName);
            var fantasyProvider = _db.GetFantasyProvider(providerName);

            if (provider == null || fantasyProvider == null)
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

        private static LeagueImportResult Finish(LeagueImportResult result)
        {
            result.Success = true;
            return result;
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
