using System;
using System.Collections.Generic;
using System.Linq;
using RotoMonster.Core;
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
                ScoringSystem = s.ScoringSystem,
                LeagueType = s.LeagueType,
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

                var match = FindCategory(category.Code);
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
        }

        // -------------------------------------------------------------------
        // Teams and rosters
        // -------------------------------------------------------------------

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
        private Category FindCategory(string code)
        {
            if (string.IsNullOrEmpty(code)) return null;

            switch (ProviderKey())
            {
                case "yahoo":
                    return _categories.FirstOrDefault(c => c.YahooId == code);
                case "espn":
                    return _categories.FirstOrDefault(c => c.ESPNId == code);
                default:
                    // Fantrax has no id column on Category, so fall back to the
                    // abbreviation, which is how its codes read anyway.
                    return _categories.FirstOrDefault(c =>
                        string.Equals(c.Abbreviation, code, StringComparison.OrdinalIgnoreCase));
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
