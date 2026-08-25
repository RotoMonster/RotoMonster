using System;
using System.Collections.Generic;
using System.Linq;
using RotoMonster.Core;
using RotoMonster.Core.Libs;
using RotoMonsterExternalAPIs.Client.Models.Providers;

namespace RotoMonster.Data
{
    /// <summary>
    /// Turns what a provider gave us into RM entities.
    ///
    /// The provider layer deliberately knows nothing about RM ids, so this is
    /// where provider codes become Category ids, ActiveRosterSpot ids and
    /// Player ids. Everything it needs is passed in once and reused across a
    /// whole bulk import, rather than re-read per league the way the old
    /// import did.
    ///
    /// Nothing here touches the database. It builds objects and reports what
    /// it could not match, and the caller decides what to save.
    /// </summary>
    public class ProviderImportMapper
    {
        private readonly FantasyProvider _provider;
        private readonly Season _season;
        private readonly List<Category> _categories;
        private readonly List<ActiveRosterSpot> _rosterSpots;

        // Provider player id to RM player id. Built once, because the old
        // import re-read the whole provider player table for every league.
        private readonly Dictionary<string, int> _playersByProviderId;

        public ProviderImportMapper(
            FantasyProvider provider,
            Season season,
            List<Category> categories,
            List<ActiveRosterSpot> rosterSpots,
            List<FantasyProviderPlayer> providerPlayers)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
            _season = season ?? throw new ArgumentNullException(nameof(season));
            _categories = categories ?? new List<Category>();
            _rosterSpots = rosterSpots ?? new List<ActiveRosterSpot>();

            _playersByProviderId = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            if (providerPlayers != null)
            {
                foreach (var pp in providerPlayers)
                {
                    if (pp == null || string.IsNullOrEmpty(pp.ProviderId)) continue;
                    if (pp.FantasyProviderId != _provider.Id) continue;
                    if (_playersByProviderId.ContainsKey(pp.ProviderId)) continue;

                    var playerId = pp.PlayerId != 0
                        ? pp.PlayerId
                        : (pp.Player != null ? pp.Player.Id : 0);

                    if (playerId != 0)
                        _playersByProviderId[pp.ProviderId] = playerId;
                }
            }
        }

        // -------------------------------------------------------------------
        // League
        // -------------------------------------------------------------------

        public ProviderImportMapping MapLeague(string userId, ProviderLeagueData data)
        {
            var mapping = new ProviderImportMapping { LeagueId = data.LeagueId };

            if (data.Settings == null)
            {
                mapping.Warnings.Add("No settings were returned for this league.");
                return mapping;
            }

            var s = data.Settings;

            var league = new UserLeague
            {
                UserId = userId,
                SeasonId = _season.Id,
                FantasyProviderId = _provider.Id,
                ProviderLeagueId = s.LeagueId,
                TrackLeague = true,
                Title = s.Title,
                DisplayTitle = s.Title,
                ScoringSystem = ToScoringSystem(s),
                LeagueType = ToLeagueType(s),
                LineupFrequency = s.LineupFrequency,
                SameDayTransactions = s.SameDayTransactions,
                NumberOfTeams = s.NumberOfTeams,
                PlayersPerTeam = s.PlayersPerTeam,
                IRSpots = s.IRSpots,
                IsAuction = s.IsAuction,
                IsMoney = s.IsMoney,
                IsProLeague = s.IsProLeague,
                IsDynasty = s.IsDynasty,
                HasDrafted = s.HasDrafted,
                DraftDate = s.DraftDate,
                WaiverType = s.WaiverType ?? "",
                WaiverRule = s.WaiverRule ?? "",
                ContinuousWaivers = s.ContinuousWaivers,
                EntryFee = s.EntryFee,
                GameLimit = s.GameLimit,
                CreatedDate = DateTime.UtcNow
            };

            if (s.StartWeekday > 0)
                league.StartWeekday = s.StartWeekday;

            MapRosterSpots(league, s, mapping);
            MapCategories(league, s, mapping);

            league.FillUserLeagueCategoriesCode(_categories);

            if (data.Teams != null)
                MapTeams(league, data.Teams, mapping);

            if (data.DraftPicks != null && data.DraftPicks.Count > 0)
                mapping.Draft = MapDraft(league, data.DraftPicks);

            mapping.UserLeague = league;
            return mapping;
        }

        private void MapRosterSpots(UserLeague league, ProviderLeagueSettings settings, ProviderImportMapping mapping)
        {
            foreach (var spot in settings.RosterSpots)
            {
                // Bench and injury slots are already counted into
                // PlayersPerTeam and IRSpots, and RM has no ActiveRosterSpot
                // row for them, so they are skipped rather than reported as
                // unmatched.
                if (spot.IsBench || spot.IsInjured) continue;

                var match = FindRosterSpot(spot.Code);
                if (match == null)
                {
                    mapping.Warnings.Add("No roster spot matches \"" + spot.Code + "\".");
                    continue;
                }

                league.UserLeagueActiveRosterSpots.Add(new UserLeagueActiveRosterSpot
                {
                    ActiveRosterSpotId = match.Id,
                    NumberOfPlayers = spot.Count
                });
            }
        }

        private void MapCategories(UserLeague league, ProviderLeagueSettings settings, ProviderImportMapping mapping)
        {
            foreach (var category in settings.Categories)
            {
                // Display-only stats are shown by the provider but not scored,
                // so importing them would invent categories nobody plays.
                if (category.IsDisplayOnly) continue;

                var match = FindCategory(category);
                if (match == null)
                {
                    var name = string.IsNullOrEmpty(category.Name) ? category.Code : category.Name;
                    mapping.Warnings.Add("No category matches \"" + name + "\" (" + category.Code + ").");
                    continue;
                }

                league.UserLeagueCategories.Add(new UserLeagueCategory
                {
                    CategoryId = match.Id,
                    IsActive = true,
                    PointsPerStat = category.PointsPerStat.HasValue ? category.PointsPerStat.Value : 0
                });
            }

            MapPlayerTypes(league);
        }

        /// <summary>
        /// One row per player type, holding that type's categories as a colon
        /// separated list of category ids in ascending order, e.g. hitters
        /// with HR, RBI, SB, R and AVG becomes "31:32:33:34:42".
        ///
        /// The code has to be sorted and in that exact shape, because
        /// CategoriesStrings is a lookup keyed on it. A differently ordered
        /// code would create a second row meaning the same thing.
        ///
        /// AddUserLeague turns CategoriesCode1 into the id, so nothing here
        /// needs to touch that table.
        /// </summary>
        private void MapPlayerTypes(UserLeague league)
        {
            var byType = new Dictionary<int, List<int>>();

            foreach (var ulc in league.UserLeagueCategories)
            {
                var category = _categories.FirstOrDefault(c => c.Id == ulc.CategoryId);

                // PlayerType is a navigation property rather than an id, so it
                // is only here if the category was loaded with it included.
                if (category == null || category.PlayerType == null) continue;

                var playerTypeId = category.PlayerType.Id;

                List<int> ids;
                if (!byType.TryGetValue(playerTypeId, out ids))
                {
                    ids = new List<int>();
                    byType[playerTypeId] = ids;
                }

                if (!ids.Contains(category.Id))
                    ids.Add(category.Id);
            }

            foreach (var pair in byType)
            {
                var ids = pair.Value;
                ids.Sort();

                league.UserLeaguePlayerTypes.Add(new UserLeaguePlayerType
                {
                    PlayerTypeId = pair.Key,
                    CategoriesCode1 = string.Join(":", ids)
                });
            }
        }

        // -------------------------------------------------------------------
        // Teams and rosters
        // -------------------------------------------------------------------

        /// <summary>
        /// Maps just the teams and rosters, for refreshing a league that is
        /// already imported. Nothing else about the league is touched, so a
        /// refresh cannot quietly change its settings.
        /// </summary>
        public ProviderRosterMapping MapRosters(ProviderLeagueData data)
        {
            var mapping = new ProviderRosterMapping { LeagueId = data.LeagueId };

            if (data.Teams == null || data.Teams.Count == 0)
            {
                mapping.Warnings.Add("No teams were returned for this league.");
                return mapping;
            }

            foreach (var providerTeam in data.Teams)
            {
                var team = new UserLeagueTeam
                {
                    ProviderId = providerTeam.TeamId,
                    Title = providerTeam.Title,
                    DraftOrder = providerTeam.DraftOrder,
                    UserLeagueTeamPlayers = new List<UserLeagueTeamPlayer>()
                };

                if (providerTeam.IsMyTeam)
                    mapping.MyProviderTeamId = providerTeam.TeamId;

                foreach (var providerPlayer in providerTeam.Players)
                {
                    var playerId = FindPlayerId(providerPlayer.PlayerId);
                    if (playerId == 0)
                    {
                        mapping.MissingPlayers.Add(new UserLeagueMissingPlayer
                        {
                            ProviderId = providerPlayer.PlayerId + "," + providerPlayer.Name
                        });
                        continue;
                    }

                    team.UserLeagueTeamPlayers.Add(new UserLeagueTeamPlayer
                    {
                        PlayerId = playerId,
                        IsActive = providerPlayer.IsActive,
                        IsIR = providerPlayer.IsIR
                    });
                }

                mapping.Teams.Add(team);
            }

            return mapping;
        }

        private void MapTeams(UserLeague league, List<ProviderTeam> teams, ProviderImportMapping mapping)
        {
            foreach (var providerTeam in teams)
            {
                var team = new UserLeagueTeam
                {
                    ProviderId = providerTeam.TeamId,
                    Title = providerTeam.Title,
                    DraftOrder = providerTeam.DraftOrder,
                    UserLeagueTeamPlayers = new List<UserLeagueTeamPlayer>()
                };

                if (providerTeam.IsMyTeam)
                {
                    league.MyProviderTeamId = providerTeam.TeamId;
                    league.MyTeamTitle = providerTeam.Title;
                }

                foreach (var providerPlayer in providerTeam.Players)
                {
                    var playerId = FindPlayerId(providerPlayer.PlayerId);
                    if (playerId == 0)
                    {
                        // Kept rather than dropped, so someone can see who was
                        // missed instead of the roster quietly coming up short.
                        mapping.MissingPlayers.Add(new UserLeagueMissingPlayer
                        {
                            ProviderId = providerPlayer.PlayerId + "," + providerPlayer.Name
                        });
                        continue;
                    }

                    team.UserLeagueTeamPlayers.Add(new UserLeagueTeamPlayer
                    {
                        PlayerId = playerId,
                        IsActive = providerPlayer.IsActive,
                        IsIR = providerPlayer.IsIR
                    });
                }

                league.UserLeagueTeams.Add(team);
            }

            // A league where no manager was flagged as ours still needs a team
            // set, or the user has to pick it by hand afterwards.
            if (string.IsNullOrEmpty(league.MyProviderTeamId) && league.UserLeagueTeams.Count > 0)
            {
                var first = league.UserLeagueTeams.First();
                league.MyProviderTeamId = first.ProviderId;
                league.MyTeamTitle = first.Title;
                mapping.Warnings.Add("Could not tell which team is yours, so the first was used.");
            }
        }

        // -------------------------------------------------------------------
        // Draft
        // -------------------------------------------------------------------

        private Draft MapDraft(UserLeague league, List<ProviderDraftPick> picks)
        {
            var draft = new Draft
            {
                SeasonId = _season.Id,
                FantasyProviderId = _provider.Id,
                ProviderLeagueId = league.ProviderLeagueId,
                Title = league.Title,
                DraftDate = league.DraftDate.HasValue ? league.DraftDate.Value : DateTime.UtcNow,
                NumberOfTeams = league.NumberOfTeams,
                LeagueSize = league.NumberOfTeams,
                IsAuction = league.IsAuction,
                IsProLeague = league.IsProLeague,
                IsDynasty = league.IsDynasty,
                IsMoney = league.IsMoney,
                LeagueType = league.LeagueType,
                IsFinished = true,
                DraftPlayers = new List<DraftPlayer>()
            };

            foreach (var pick in picks)
            {
                var playerId = FindPlayerId(pick.PlayerId);
                if (playerId == 0) continue;

                draft.DraftPlayers.Add(new DraftPlayer
                {
                    PlayerId = playerId,
                    DraftOrder = pick.PickNumber,
                    Price = pick.Price,
                    ProviderTeamId = pick.TeamId
                });
            }

            return draft;
        }

        // -------------------------------------------------------------------
        // Lookups
        // -------------------------------------------------------------------

        /// <summary>
        /// Each provider has its own column on Category and ActiveRosterSpot,
        /// so which one to read depends on who we are importing from.
        /// </summary>
        private Category FindCategory(ProviderCategory category)
        {
            var code = category.Code;
            if (string.IsNullOrEmpty(code)) return null;

            switch (ProviderKey())
            {
                case "yahoo":
                    return _categories.FirstOrDefault(c => c.YahooId == code);

                case "espn":
                    return _categories.FirstOrDefault(c => c.ESPNId == code);

                case "cbs":
                    return _categories.FirstOrDefault(c => c.CBSId == code);

                case "sleeper":
                    // There is no Sleeper column on Category, so this matches on
                    // the abbreviation instead. Sleeper writes its codes in
                    // lower case where ours are upper, hence the case
                    // insensitive compare. Swap this for a SleeperId column if
                    // the abbreviations ever stop lining up.
                    return _categories.FirstOrDefault(c =>
                        string.Equals(c.Abbreviation, code, StringComparison.OrdinalIgnoreCase));

                case "fantrax":
                    // Fantrax scopes its ids by group, so the same id can mean
                    // different things depending on which group it came from.
                    // Matching on the id alone can land on the wrong category,
                    // which is why this goes through CategoryLib rather than a
                    // straight comparison.
                    var matches = new CategoryLib()
                        .GetFanTraxCategories(_categories, category.GroupCode, code);
                    return matches.FirstOrDefault();

                default:
                    return null;
            }
        }

        private ActiveRosterSpot FindRosterSpot(string code)
        {
            if (string.IsNullOrEmpty(code)) return null;

            Func<ActiveRosterSpot, string> providerTitle;
            switch (ProviderKey())
            {
                case "yahoo":
                    providerTitle = spot => spot.YahooTitle;
                    break;
                case "espn":
                    providerTitle = spot => spot.ESPNTitle;
                    break;
                case "fantrax":
                    providerTitle = spot => spot.FanTraxTitle;
                    break;
                default:
                    providerTitle = spot => null;
                    break;
            }

            return _rosterSpots.FirstOrDefault(spot =>
                       string.Equals(providerTitle(spot), code, StringComparison.OrdinalIgnoreCase))
                   ?? _rosterSpots.FirstOrDefault(spot =>
                       string.Equals(spot.Title, code, StringComparison.OrdinalIgnoreCase));
        }

        private int FindPlayerId(string providerPlayerId)
        {
            if (string.IsNullOrEmpty(providerPlayerId)) return 0;

            int playerId;
            return _playersByProviderId.TryGetValue(providerPlayerId, out playerId) ? playerId : 0;
        }

        /// <summary>
        /// FantasyProvider.Name is "Yahoo!", "ESPN", "FanTrax". Normalised here
        /// so the switches above do not have to carry the punctuation.
        /// </summary>
        private string ProviderKey()
        {
            var name = _provider.Name ?? "";
            return name.Replace("!", "").Trim().ToLowerInvariant();
        }

        /// <summary>
        /// C for categories, P for points per stat.
        ///
        /// Providers word this differently and some bundle it with the format,
        /// so CBS says "Head-to-Head, Points" where Yahoo says "head". The
        /// surest signal is the categories themselves: a points league gives a
        /// points value per stat and a categories league does not.
        /// </summary>
        private static string ToScoringSystem(ProviderLeagueSettings s)
        {
            foreach (var category in s.Categories)
            {
                if (category.PointsPerStat.HasValue)
                    return "P";
            }

            var raw = (s.ScoringSystem ?? "").ToLowerInvariant();
            return raw.Contains("point") ? "P" : "C";
        }

        /// <summary>
        /// H for head to head, R for rotisserie. Anything that is not clearly
        /// rotisserie is treated as head to head, which is the commoner case.
        /// </summary>
        private static string ToLeagueType(ProviderLeagueSettings s)
        {
            var raw = ((s.LeagueType ?? "") + " " + (s.ScoringSystem ?? "")).ToLowerInvariant();

            if (raw.Contains("roto"))
                return "R";

            if (raw.Contains("head"))
                return "H";

            // Yahoo reports these as single letters already.
            var type = (s.LeagueType ?? "").Trim().ToUpperInvariant();
            if (type == "R" || type == "H")
                return type;

            return "H";
        }

    }

    /// <summary>
    /// Teams and rosters for a league that already exists, from a refresh.
    /// </summary>
    public class ProviderRosterMapping
    {
        public string LeagueId { get; set; }

        public List<UserLeagueTeam> Teams { get; set; } = new List<UserLeagueTeam>();

        public List<UserLeagueMissingPlayer> MissingPlayers { get; set; } = new List<UserLeagueMissingPlayer>();

        /// <summary>
        /// Empty when the provider did not say which team is the user's, in
        /// which case the caller should keep whatever it already had.
        /// </summary>
        public string MyProviderTeamId { get; set; }

        public List<string> Warnings { get; set; } = new List<string>();

        /// <summary>
        /// A provider returning no teams is treated as a failure rather than
        /// an empty roster, so a bad response cannot wipe a league's teams.
        /// </summary>
        public bool Success
        {
            get { return Teams.Count > 0; }
        }
    }

    /// <summary>
    /// One league's worth of mapped entities, plus anything that did not map.
    ///
    /// Warnings are returned rather than logged so the import page can show
    /// the user which categories or players were missed, instead of the
    /// league importing with silent gaps.
    /// </summary>
    public class ProviderImportMapping
    {
        public string LeagueId { get; set; }

        public UserLeague UserLeague { get; set; }

        public Draft Draft { get; set; }

        public List<UserLeagueMissingPlayer> MissingPlayers { get; set; } = new List<UserLeagueMissingPlayer>();

        public List<string> Warnings { get; set; } = new List<string>();

        public bool Success
        {
            get { return UserLeague != null; }
        }

    }
}
