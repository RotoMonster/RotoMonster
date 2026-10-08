using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RotoMonster.Core;
using RotoMonsterExternalAPIs.Client.Models.Providers;

namespace RotoMonster.Data
{
    public class NHLSyncResult
    {
        public int Created { get; set; }
        public int Updated { get; set; }
        public int Skipped { get; set; }
        public List<string> Notes { get; set; } = new List<string>();

        public override string ToString()
        {
            return $"created {Created}, updated {Updated}, skipped {Skipped}";
        }
    }

    public class NHLDataSync
    {
        public const string ProviderName = "NHL";

        public const int SkaterPlayerTypeId = 1;
        public const int GoaliePlayerTypeId = 2;

        private static readonly Dictionary<string, string> TeamCodeMap =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "LAK", "LA" },
                { "NJD", "NJ" },
                { "SJS", "SJ" },
                { "TBL", "TB" }
            };

        private static readonly Dictionary<string, int> PositionIds =
            new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
            {
                { "D", 1 },
                { "R", 2 },
                { "RW", 2 },
                { "L", 3 },
                { "LW", 3 },
                { "C", 4 },
                { "G", 5 }
            };

        private static readonly HashSet<string> NameSuffixes =
            new HashSet<string> { "jr", "sr", "ii", "iii", "iv", "v" };

        private static readonly TimeZoneInfo Eastern = TimeZoneInfo.FindSystemTimeZoneById("America/New_York");

        private readonly RMDBContext _db;
        private Dictionary<string, int> _teamIds;
        private int? _providerId;

        public NHLDataSync(RMDBContext db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public HashSet<string> IgnoredProviderIds { get; } = new HashSet<string>();

        public static int SeasonIdFor(int year)
        {
            return (year - 2009) * 10;
        }

        public static string SeasonKey(int year)
        {
            return year.ToString(CultureInfo.InvariantCulture) + (year + 1).ToString(CultureInfo.InvariantCulture);
        }

        public static string SeasonTitle(int year)
        {
            return "NHL " + year.ToString(CultureInfo.InvariantCulture) + "-" + ((year + 1) % 100).ToString("00", CultureInfo.InvariantCulture);
        }

        public async Task<int> GetProviderIdAsync()
        {
            if (_providerId.HasValue) return _providerId.Value;

            var provider = await _db.Set<FantasyProvider>()
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Name == ProviderName);

            if (provider == null)
                throw new InvalidOperationException("FantasyProviders has no '" + ProviderName + "' row.");

            _providerId = provider.Id;
            return provider.Id;
        }

        public async Task<Dictionary<string, int>> GetProviderPlayerMapAsync()
        {
            var providerId = await GetProviderIdAsync();

            var rows = await _db.Set<FantasyProviderPlayer>().AsNoTracking()
                .Where(f => f.FantasyProviderId == providerId && f.ProviderId != null)
                .Select(f => new { f.ProviderId, f.PlayerId })
                .ToListAsync();

            var map = new Dictionary<string, int>();
            foreach (var r in rows) map[r.ProviderId.Trim()] = r.PlayerId;
            return map;
        }

        public async Task<int?> GetTeamIdAsync(string providerCode)
        {
            if (string.IsNullOrWhiteSpace(providerCode)) return null;

            if (_teamIds == null)
            {
                _teamIds = await _db.Set<Team>()
                    .AsNoTracking()
                    .ToDictionaryAsync(t => t.Code.Trim(), t => t.Id, StringComparer.OrdinalIgnoreCase);
            }

            var code = providerCode.Trim();
            string mapped;
            if (TeamCodeMap.TryGetValue(code, out mapped)) code = mapped;

            int id;
            return _teamIds.TryGetValue(code, out id) ? id : (int?)null;
        }

        public async Task<NHLSyncResult> EnsureSeasonAsync(int year, DateTime startDate, DateTime endDate, IEnumerable<string> teamCodes)
        {
            var result = new NHLSyncResult();
            var seasonId = SeasonIdFor(year);

            var previous = await _db.Set<Season>()
                .AsNoTracking()
                .Where(s => s.Id < seasonId)
                .OrderByDescending(s => s.Id)
                .FirstOrDefaultAsync();

            var exists = await _db.Set<Season>().AsNoTracking().AnyAsync(s => s.Id == seasonId);
            if (!exists)
            {
                var title = SeasonTitle(year);
                var espnYear = year + 1;

                await _db.Database.ExecuteSqlInterpolatedAsync($@"
INSERT INTO Seasons (Id, [Year], Title, Abbreviation, StartDate, EndDate, IsRegularSeason, YahooId, IsEnabled, DisplayOrder, ESPNYear)
VALUES ({seasonId}, {year}, {title}, {title}, {startDate.Date}, {endDate.Date}, 1, NULL, 1, (SELECT ISNULL(MIN(DisplayOrder), 0) - 1 FROM Seasons), {espnYear});");

                result.Created++;
                result.Notes.Add("Created season " + seasonId + " (" + title + ")");
            }

            var divisions = previous != null
                ? await _db.Set<SeasonTeam>().AsNoTracking()
                    .Where(st => st.SeasonId == previous.Id)
                    .ToDictionaryAsync(st => st.TeamId, st => st.DivisionId)
                : new Dictionary<int, int>();

            var existing = await _db.Set<SeasonTeam>().AsNoTracking()
                .Where(st => st.SeasonId == seasonId)
                .Select(st => st.TeamId)
                .ToListAsync();

            var seen = new HashSet<int>(existing);

            foreach (var code in teamCodes ?? Enumerable.Empty<string>())
            {
                var teamId = await GetTeamIdAsync(code);
                if (!teamId.HasValue)
                {
                    result.Notes.Add("No team for code " + code);
                    result.Skipped++;
                    continue;
                }

                if (!seen.Add(teamId.Value)) continue;

                int divisionId;
                if (!divisions.TryGetValue(teamId.Value, out divisionId))
                {
                    result.Notes.Add("No previous division for " + code);
                    result.Skipped++;
                    continue;
                }

                _db.Set<SeasonTeam>().Add(new SeasonTeam { SeasonId = seasonId, TeamId = teamId.Value, DivisionId = divisionId });
                result.Created++;
            }

            await _db.SaveChangesAsync();
            return result;
        }

        public static (int Percent, int Period, string Clock) LiveState(SportsDataGame g, Game current)
        {
            const int periodSeconds = 1200;
            const int regulationSeconds = 3600;

            if (g.IsFinished)
            {
                var last = Math.Max(g.CurrentPeriod ?? current.Period, 3);
                var label = last >= 5 ? "Final/SO" : last == 4 ? "Final/OT" : "Final";
                return (100, last, label);
            }

            if (!g.IsInProgress)
                return (current.PercentComplete, current.Period, current.GameClock);

            var period = g.CurrentPeriod ?? current.Period;
            if (period <= 0)
                return (current.PercentComplete, current.Period, current.GameClock);

            if (g.Intermission.HasValue && g.Intermission.Value > 0)
            {
                var percent = period >= 3 ? 99 : Math.Min(99, (int)Math.Round(period * periodSeconds * 100.0 / regulationSeconds));
                return (percent, period, "End " + PeriodLabel(period));
            }

            var left = Math.Max(0, Math.Min(periodSeconds, g.PeriodSecondsRemaining ?? periodSeconds));
            var clock = PeriodLabel(period) + " " + (left / 60) + ":" + (left % 60).ToString("00");

            if (period > 3)
                return (99, period, clock);

            var elapsed = (period - 1) * periodSeconds + (periodSeconds - left);
            var live = Math.Min(99, (int)Math.Round(elapsed * 100.0 / regulationSeconds));
            return (live, period, clock);
        }

        private static string PeriodLabel(int period)
        {
            if (period == 4) return "OT";
            if (period >= 5) return "SO";
            return "P" + period;
        }

        public async Task<Dictionary<string, Game>> SyncGamesAsync(int seasonId, IEnumerable<SportsDataGame> games, NHLSyncResult result)
        {
            var map = new Dictionary<string, Game>();
            var list = (games ?? Enumerable.Empty<SportsDataGame>()).ToList();
            if (list.Count == 0) return map;

            var existing = await _db.Set<Game>()
                .Where(g => g.SeasonId == seasonId)
                .ToListAsync();

            var byKey = new Dictionary<string, Game>();
            foreach (var g in existing)
                byKey[GameKey(g.GameDate, g.HomeTeamId, g.AwayTeamId)] = g;

            foreach (var pg in list)
            {
                var homeId = await GetTeamIdAsync(pg.HomeTeamCode);
                var awayId = await GetTeamIdAsync(pg.AwayTeamCode);
                if (!homeId.HasValue || !awayId.HasValue)
                {
                    result.Skipped++;
                    result.Notes.Add("Unknown team on game " + pg.GameId + ": " + pg.AwayTeamCode + "@" + pg.HomeTeamCode);
                    continue;
                }

                var local = TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(pg.StartTimeUtc, DateTimeKind.Utc), Eastern);
                var key = GameKey(local.Date, homeId, awayId);

                Game game;
                var isNew = !byKey.TryGetValue(key, out game);
                if (isNew)
                {
                    game = existing.FirstOrDefault(g => g.HomeTeamId == homeId && g.AwayTeamId == awayId && !g.IsFinished
                                                        && Math.Abs((g.GameDate - local.Date).TotalDays) <= 120
                                                        && !list.Any(o => o != pg && SameMatchup(o, g)));
                    if (game != null)
                    {
                        isNew = false;
                        result.Notes.Add("Moved game " + game.Id + " from " + game.GameDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
                                         + " to " + local.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
                    }
                }

                if (isNew)
                {
                    game = new Game
                    {
                        SeasonId = seasonId,
                        Number = 1,
                        HomeTeamId = homeId,
                        AwayTeamId = awayId
                    };
                    _db.Set<Game>().Add(game);
                }
                byKey[key] = game;

                var state = LiveState(pg, game);

                var changed = isNew
                    | Set(game.GameDate, local.Date, v => game.GameDate = v)
                    | Set(game.GameTime, DateTime.SpecifyKind(pg.StartTimeUtc, DateTimeKind.Unspecified), v => game.GameTime = v)
                    | Set(game.HomeScore, pg.HomeScore ?? game.HomeScore, v => game.HomeScore = v)
                    | Set(game.AwayScore, pg.AwayScore ?? game.AwayScore, v => game.AwayScore = v)
                    | Set(game.IsFinished, pg.IsFinished, v => game.IsFinished = v)
                    | Set(game.PercentComplete, state.Percent, v => game.PercentComplete = v)
                    | Set(game.Period, state.Period, v => game.Period = v)
                    | Set(game.GameClock, state.Clock, v => game.GameClock = v);

                if (isNew) result.Created++;
                else if (changed) result.Updated++;

                map[pg.GameId] = game;
            }

            await _db.SaveChangesAsync();
            return map;
        }

        private bool SameMatchup(SportsDataGame other, Game game)
        {
            var local = TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(other.StartTimeUtc, DateTimeKind.Utc), Eastern);
            if (local.Date != game.GameDate.Date) return false;
            var home = TeamCodeFor(other.HomeTeamCode);
            var away = TeamCodeFor(other.AwayTeamCode);
            int homeId, awayId;
            return _teamIds != null && _teamIds.TryGetValue(home, out homeId) && _teamIds.TryGetValue(away, out awayId)
                   && homeId == game.HomeTeamId && awayId == game.AwayTeamId;
        }

        private static string TeamCodeFor(string providerCode)
        {
            var code = (providerCode ?? "").Trim();
            string mapped;
            return TeamCodeMap.TryGetValue(code, out mapped) ? mapped : code;
        }

        public async Task<Dictionary<string, int>> SyncPlayersAsync(int seasonId, IEnumerable<SportsDataPlayer> players, NHLSyncResult result)
        {
            var providerId = await GetProviderIdAsync();
            var list = (players ?? Enumerable.Empty<SportsDataPlayer>())
                .Where(p => !string.IsNullOrEmpty(p.PlayerId))
                .ToList();

            var mappings = await _db.Set<FantasyProviderPlayer>()
                .Where(f => f.FantasyProviderId == providerId)
                .ToListAsync();

            var byProviderId = new Dictionary<string, int>();
            foreach (var m in mappings)
                if (!string.IsNullOrEmpty(m.ProviderId)) byProviderId[m.ProviderId.Trim()] = m.PlayerId;

            var mappedPlayerIds = new HashSet<int>(mappings.Select(m => m.PlayerId));

            var unmapped = list.Where(p => !byProviderId.ContainsKey(p.PlayerId) && !IgnoredProviderIds.Contains(p.PlayerId)).ToList();

            if (unmapped.Count > 0)
            {
                var pool = await LoadMatchPoolAsync(mappedPlayerIds);

                foreach (var p in unmapped)
                {
                    var match = Match(p, pool);
                    var playerId = match.Item1;
                    var how = match.Item2;

                    if (!playerId.HasValue && how != null)
                    {
                        result.Skipped++;
                        result.Notes.Add("Needs review: " + p.FirstName + " " + p.LastName + " " + p.Position + " " + p.TeamCode + " (" + p.PlayerId + ") " + how);
                        continue;
                    }

                    if (!playerId.HasValue)
                    {
                        if (!p.BirthDate.HasValue)
                        {
                            result.Skipped++;
                            result.Notes.Add("No birthdate, not created: " + p.FirstName + " " + p.LastName + " (" + p.PlayerId + ")");
                            continue;
                        }

                        var created = CreatePlayer(p);
                        await _db.SaveChangesAsync();
                        playerId = created.Id;
                        result.Created++;
                        result.Notes.Add("Created " + created.FirstName + " " + created.LastName + " " + p.Position + " " + p.TeamCode + " (" + created.Id + ")");
                    }
                    else if (how != "exact")
                    {
                        result.Notes.Add("Matched by " + how + ": " + p.FirstName + " " + p.LastName + " (" + p.PlayerId + ") to " + playerId.Value);
                    }

                    pool.Remove(playerId.Value);

                    _db.Set<FantasyProviderPlayer>().Add(new FantasyProviderPlayer
                    {
                        FantasyProviderId = providerId,
                        PlayerId = playerId.Value,
                        ProviderId = p.PlayerId
                    });

                    byProviderId[p.PlayerId] = playerId.Value;
                    mappedPlayerIds.Add(playerId.Value);
                    result.Updated++;
                }

                await _db.SaveChangesAsync();
            }

            await SyncSeasonPlayersAsync(seasonId, list, byProviderId, result);
            await FillMissingPositionsAsync(list, byProviderId, result);

            return byProviderId;
        }

        private async Task SyncSeasonPlayersAsync(int seasonId, List<SportsDataPlayer> players, Dictionary<string, int> byProviderId, NHLSyncResult result)
        {
            var existing = await _db.Set<SeasonPlayer>()
                .Where(sp => sp.SeasonId == seasonId)
                .ToListAsync();

            var byPlayer = new Dictionary<int, SeasonPlayer>();
            foreach (var sp in existing) byPlayer[sp.PlayerId] = sp;

            foreach (var p in players)
            {
                int playerId;
                if (!byProviderId.TryGetValue(p.PlayerId, out playerId)) continue;

                var teamId = await GetTeamIdAsync(p.TeamCode);
                if (!teamId.HasValue) continue;

                var typeId = IsGoalie(p.Position) ? GoaliePlayerTypeId : SkaterPlayerTypeId;

                SeasonPlayer sp;
                if (!byPlayer.TryGetValue(playerId, out sp))
                {
                    sp = new SeasonPlayer { SeasonId = seasonId, PlayerId = playerId, TeamId = teamId.Value, PlayerTypeId = typeId };
                    _db.Set<SeasonPlayer>().Add(sp);
                    byPlayer[playerId] = sp;
                    continue;
                }

                if (sp.TeamId != teamId.Value)
                {
                    sp.TeamId = teamId.Value;
                    result.Notes.Add("Team change: player " + playerId + " now " + p.TeamCode);
                }
            }

            await _db.SaveChangesAsync();
        }

        private async Task FillMissingPositionsAsync(List<SportsDataPlayer> players, Dictionary<string, int> byProviderId, NHLSyncResult result)
        {
            var withPosition = new HashSet<int>(await _db.Set<PlayerDefaultPosition>().AsNoTracking()
                .Select(x => x.PlayerId)
                .Distinct()
                .ToListAsync());

            foreach (var p in players)
            {
                int playerId;
                if (!byProviderId.TryGetValue(p.PlayerId, out playerId)) continue;
                if (withPosition.Contains(playerId)) continue;

                int positionId;
                if (!PositionIds.TryGetValue((p.Position ?? "").Trim(), out positionId)) continue;

                _db.Set<PlayerDefaultPosition>().Add(new PlayerDefaultPosition { PlayerId = playerId, PositionId = positionId });
                withPosition.Add(playerId);
                result.Notes.Add("Added default position " + p.Position + " for player " + playerId);
            }

            await _db.SaveChangesAsync();
        }

        public async Task<NHLSyncResult> SyncPlayerGamesAsync(
            IEnumerable<SportsDataPlayerGame> lines,
            Dictionary<string, Game> games,
            Dictionary<string, int> players)
        {
            var result = new NHLSyncResult();
            var list = (lines ?? Enumerable.Empty<SportsDataPlayerGame>()).ToList();
            if (list.Count == 0) return result;

            var gameIds = games.Values.Select(g => g.Id).Distinct().ToList();

            var skaters = await _db.Set<NHLSkaterGame>().Where(x => gameIds.Contains(x.GameId)).ToListAsync();
            var goalies = await _db.Set<NHLGoalieGame>().Where(x => gameIds.Contains(x.GameId)).ToListAsync();

            var skaterByKey = skaters.ToDictionary(x => Tuple.Create(x.PlayerId, x.GameId));
            var goalieByKey = goalies.ToDictionary(x => Tuple.Create(x.PlayerId, x.GameId));

            foreach (var line in list)
            {
                Game game;
                if (string.IsNullOrEmpty(line.GameId) || !games.TryGetValue(line.GameId, out game)) continue;

                var teamId = await GetTeamIdAsync(line.TeamCode);
                if (!teamId.HasValue) continue;

                int playerId;
                if (string.IsNullOrEmpty(line.PlayerId) || !players.TryGetValue(line.PlayerId, out playerId))
                {
                    result.Skipped++;
                    result.Notes.Add("No player mapping: " + line.PlayerName + " " + line.Position + " " + line.TeamCode + " (" + line.PlayerId + ")");
                    continue;
                }

                var key = Tuple.Create(playerId, game.Id);

                if (IsGoalie(line.Position))
                {
                    NHLGoalieGame g;
                    if (!goalieByKey.TryGetValue(key, out g))
                    {
                        g = new NHLGoalieGame { PlayerId = playerId, GameId = game.Id };
                        _db.Set<NHLGoalieGame>().Add(g);
                        goalieByKey[key] = g;
                        result.Created++;
                    }
                    else result.Updated++;

                    g.TeamId = teamId.Value;
                    FillGoalie(g, line);
                }
                else
                {
                    NHLSkaterGame s;
                    if (!skaterByKey.TryGetValue(key, out s))
                    {
                        s = new NHLSkaterGame { PlayerId = playerId, GameId = game.Id };
                        _db.Set<NHLSkaterGame>().Add(s);
                        skaterByKey[key] = s;
                        result.Created++;
                    }
                    else result.Updated++;

                    s.TeamId = teamId.Value;
                    FillSkater(s, line);
                }
            }

            await _db.SaveChangesAsync();
            return result;
        }

        private static void FillSkater(NHLSkaterGame s, SportsDataPlayerGame l)
        {
            s.Started = B(l, "Started");
            s.PowerPlayTimeOnIce = l.Get("PowerPlayTimeOnIce");
            s.PowerPlayShots = B(l, "PowerPlayShots");
            s.PowerPlayGoals = B(l, "PowerPlayGoals");
            s.PowerPlayMissedShots = B(l, "PowerPlayMissedShots");
            s.PowerPlayAssists = B(l, "PowerPlayAssists");
            s.PowerPlayFaceoffsWon = B(l, "PowerPlayFaceoffsWon");
            s.PowerPlayFaceoffsLost = B(l, "PowerPlayFaceoffsLost");
            s.ShorthandedTimeOnIce = l.Get("ShorthandedTimeOnIce");
            s.ShorthandedShots = B(l, "ShorthandedShots");
            s.ShorthandedGoals = B(l, "ShorthandedGoals");
            s.ShorthandedMissedShots = B(l, "ShorthandedMissedShots");
            s.ShorthandedAssists = B(l, "ShorthandedAssists");
            s.ShorthandedFaceoffsWon = B(l, "ShorthandedFaceoffsWon");
            s.ShorthandedFaceoffsLost = B(l, "ShorthandedFaceoffsLost");
            s.EvenstrengthTimeOnIce = l.Get("EvenstrengthTimeOnIce");
            s.EvenstrengthShots = B(l, "EvenstrengthShots");
            s.EvenstrengthGoals = B(l, "EvenstrengthGoals");
            s.EvenstrengthMissedShots = B(l, "EvenstrengthMissedShots");
            s.EvenstrengthAssists = B(l, "EvenstrengthAssists");
            s.EvenstrengthFaceoffsWon = B(l, "EvenstrengthFaceoffsWon");
            s.EvenstrengthFaceoffsLost = B(l, "EvenstrengthFaceoffsLost");
            s.PenaltyShots = B(l, "PenaltyShots");
            s.PenaltyGoals = B(l, "PenaltyGoals");
            s.PenaltyMissedShots = B(l, "PenaltyMissedShots");
            s.ShootoutShots = B(l, "ShootoutShots");
            s.ShootoutGoals = B(l, "ShootoutGoals");
            s.ShootoutMissedShots = B(l, "ShootoutMissedShots");
            s.Penalties = B(l, "Penalties");
            s.PenaltyMinutes = l.Get("PenaltyMinutes");
            s.BlockedAttempts = B(l, "BlockedAttempts");
            s.Hits = B(l, "Hits");
            s.Giveaways = B(l, "Giveaways");
            s.Takeaways = B(l, "Takeaways");
            s.BlockedShots = B(l, "BlockedShots");
            s.PlusMinus = l.Get("PlusMinus");
            s.OvertimeGoals = B(l, "OvertimeGoals");
            s.OvertimeAssists = B(l, "OvertimeAssists");
            s.OvertimeShots = B(l, "OvertimeShots");
            s.PenaltiesMajor = B(l, "PenaltiesMajor");
            s.PenaltiesMinor = B(l, "PenaltiesMinor");
            s.PenaltiesMisconduct = B(l, "PenaltiesMisconduct");
            s.EmptynetGoals = B(l, "EmptynetGoals");
            s.Shifts = B(l, "Shifts");
        }

        private static void FillGoalie(NHLGoalieGame g, SportsDataPlayerGame l)
        {
            g.Started = B(l, "Started");
            g.Shifts = B(l, "Shifts");
            g.Credit = l.Get("CreditWin") > 0 ? "win"
                     : l.Get("CreditOvertimeLoss") > 0 ? "overtime_loss"
                     : l.Get("CreditLoss") > 0 ? "loss"
                     : "none";
            g.Wins = B(l, "Wins");
            g.Shutouts = B(l, "Shutouts");
            g.Assists = B(l, "Assists");
            g.PowerPlayTimeOnIce = l.Get("PowerPlayTimeOnIce");
            g.PowerPlayShotsAgainst = B(l, "PowerPlayShotsAgainst");
            g.PowerPlayGoalsAgainst = B(l, "PowerPlayGoalsAgainst");
            g.PowerPlaySaves = B(l, "PowerPlaySaves");
            g.ShorthandedTimeOnIce = l.Get("ShorthandedTimeOnIce");
            g.ShorthandedShotsAgainst = B(l, "ShorthandedShotsAgainst");
            g.ShorthandedGoalsAgainst = B(l, "ShorthandedGoalsAgainst");
            g.ShorthandedPlaySaves = B(l, "ShorthandedPlaySaves");
            g.EvenstrengthTimeOnIce = l.Get("EvenstrengthTimeOnIce");
            g.EvenstrengthShotsAgainst = B(l, "EvenstrengthShotsAgainst");
            g.EvenstrengthGoalsAgainst = B(l, "EvenstrengthGoalsAgainst");
            g.EvenstrengthPlaySaves = B(l, "EvenstrengthPlaySaves");
            g.PenaltyShotsAgainst = B(l, "PenaltyShotsAgainst");
            g.PenaltyGoalsAgainst = B(l, "PenaltyGoalsAgainst");
            g.PenaltySaves = B(l, "PenaltySaves");
            g.ShootoutShotsAgainst = B(l, "ShootoutShotsAgainst");
            g.ShootoutGoalsAgainst = B(l, "ShootoutGoalsAgainst");
            g.ShootoutSaves = B(l, "ShootoutSaves");
        }

        private class PoolPlayer
        {
            public int Id;
            public string First;
            public string Last;
            public DateTime Birthdate;
        }

        private async Task<Dictionary<int, PoolPlayer>> LoadMatchPoolAsync(HashSet<int> alreadyMapped)
        {
            var players = await _db.Set<Player>().AsNoTracking()
                .Select(p => new { p.Id, p.FirstName, p.LastName, p.Birthdate })
                .ToListAsync();

            var pool = new Dictionary<int, PoolPlayer>();
            foreach (var p in players)
            {
                if (alreadyMapped.Contains(p.Id)) continue;
                pool[p.Id] = new PoolPlayer { Id = p.Id, First = Norm(p.FirstName), Last = Norm(p.LastName), Birthdate = p.Birthdate.Date };
            }
            return pool;
        }

        private static Tuple<int?, string> Match(SportsDataPlayer p, Dictionary<int, PoolPlayer> pool)
        {
            var first = Norm(p.FirstName);
            var last = Norm(p.LastName);
            var birth = p.BirthDate.HasValue ? p.BirthDate.Value.Date : (DateTime?)null;

            var sameName = pool.Values.Where(c => c.First == first && c.Last == last).ToList();

            if (birth.HasValue)
            {
                var exact = sameName.Where(c => c.Birthdate == birth.Value).ToList();
                if (exact.Count == 1) return Tuple.Create((int?)exact[0].Id, "exact");

                if (sameName.Count == 0)
                {
                    var sameBirthLast = pool.Values.Where(c => c.Last == last && c.Birthdate == birth.Value).ToList();
                    if (sameBirthLast.Count == 1)
                        return Tuple.Create((int?)sameBirthLast[0].Id, "last name + birthdate");
                }

                var placeholder = sameName.Where(c => IsPlaceholder(c.Birthdate)).ToList();
                if (placeholder.Count == 1 && sameName.Count == 1)
                    return Tuple.Create((int?)placeholder[0].Id, "name, placeholder birthdate");
            }

            if (sameName.Count > 0)
                return Tuple.Create((int?)null, "matches " + sameName.Count + " by name with a different birthdate, not matched: "
                    + string.Join(", ", sameName.Select(c => c.Id + " " + c.Birthdate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture))));

            return Tuple.Create((int?)null, (string)null);
        }

        private Player CreatePlayer(SportsDataPlayer p)
        {
            var player = new Player
            {
                FirstName = Truncate(p.FirstName, 80),
                LastName = Truncate(p.LastName, 80),
                Birthdate = p.BirthDate.Value.Date,
                Height = Clamp(p.HeightInches ?? 73, 60, 90),
                Weight = Clamp(p.Weight ?? 195, 120, 300),
                RookieYear = p.RookieYear,
                PickNumber = p.DraftPickNumber
            };

            _db.Set<Player>().Add(player);

            int positionId;
            if (PositionIds.TryGetValue((p.Position ?? "").Trim(), out positionId))
            {
                _db.Set<PlayerDefaultPosition>().Add(new PlayerDefaultPosition { Player = player, PositionId = positionId });
            }

            return player;
        }

        private static bool IsGoalie(string position)
        {
            return string.Equals((position ?? "").Trim(), "G", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsPlaceholder(DateTime birthdate)
        {
            return birthdate.Year < 1950;
        }

        private static string GameKey(DateTime date, int? homeId, int? awayId)
        {
            return date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) + "|" + homeId + "|" + awayId;
        }

        private static bool Set<T>(T current, T value, Action<T> apply)
        {
            if (EqualityComparer<T>.Default.Equals(current, value)) return false;
            apply(value);
            return true;
        }

        private static byte B(SportsDataPlayerGame l, string key)
        {
            return (byte)Math.Max(0, Math.Min(255, Math.Round(l.Get(key))));
        }

        private static string Norm(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";

            var decomposed = s.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();
            foreach (var ch in decomposed)
            {
                if (char.IsLetter(ch) || ch == ' ') sb.Append(char.ToLowerInvariant(ch));
            }

            var parts = sb.ToString().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            while (parts.Count > 1 && NameSuffixes.Contains(parts[parts.Count - 1])) parts.RemoveAt(parts.Count - 1);

            return string.Join("", parts);
        }

        private static int Clamp(int v, int min, int max)
        {
            return Math.Max(min, Math.Min(max, v));
        }

        private static string Truncate(string s, int max)
        {
            s = (s ?? "").Trim();
            return s.Length <= max ? s : s.Substring(0, max);
        }
    }
}
