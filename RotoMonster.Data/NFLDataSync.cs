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
    public class NFLSyncResult
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

    public class NFLDataSync
    {
        public const string ProviderName = "MySportsFeeds";

        public const int OffensivePlayerTypeId = 4;
        public const int KickerPlayerTypeId = 5;
        public const int DefensePlayerTypeId = 6;

        private static readonly Dictionary<string, string> TeamCodeMap =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "LA", "LAR" },
                { "JAX", "JAC" },
                { "WSH", "WAS" },
                { "OAK", "LV" }
            };

        private static readonly Dictionary<string, int> PositionIds =
            new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
            {
                { "QB", 1 },
                { "RB", 2 },
                { "FB", 2 },
                { "WR", 3 },
                { "TE", 4 },
                { "K", 5 },
                { "PK", 5 }
            };

        private const int MaxBirthdateGapDays = 730;

        private static readonly HashSet<string> NameSuffixes =
            new HashSet<string> { "jr", "sr", "ii", "iii", "iv", "v" };

        private static readonly TimeZoneInfo Eastern = TimeZoneInfo.FindSystemTimeZoneById("America/New_York");

        public const string NflverseProviderName = "NFL";

        private readonly RMDBContext _db;
        private Dictionary<string, int> _teamIds;
        private readonly Dictionary<string, int> _providerIds = new Dictionary<string, int>();

        public NFLDataSync(RMDBContext db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public static Dictionary<string, Game> NflverseGameMap(int year, IEnumerable<SportsDataGame> providerGames, Dictionary<string, Game> gameMap)
        {
            var map = new Dictionary<string, Game>();
            foreach (var g in providerGames ?? Enumerable.Empty<SportsDataGame>())
            {
                Game game;
                if (!gameMap.TryGetValue(g.GameId, out game)) continue;
                var id = year.ToString(CultureInfo.InvariantCulture) + "_" + g.Week.ToString("00", CultureInfo.InvariantCulture)
                         + "_" + g.AwayTeamCode + "_" + g.HomeTeamCode;
                map[id] = game;
            }
            return map;
        }

        public static int SeasonIdFor(int year)
        {
            return (year - 2009) * 10;
        }

        public Task<int> GetProviderIdAsync()
        {
            return GetProviderIdAsync(ProviderName);
        }

        public async Task<int> GetProviderIdAsync(string name)
        {
            int cached;
            if (_providerIds.TryGetValue(name, out cached)) return cached;

            var provider = await _db.Set<FantasyProvider>()
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Name == name);

            if (provider == null)
                throw new InvalidOperationException("FantasyProviders has no '" + name + "' row.");

            _providerIds[name] = provider.Id;
            return provider.Id;
        }

        public async Task<Dictionary<string, int>> GetProviderPlayerMapAsync(string providerName)
        {
            var providerId = await GetProviderIdAsync(providerName);

            var rows = await _db.Set<FantasyProviderPlayer>().AsNoTracking()
                .Where(f => f.FantasyProviderId == providerId && f.ProviderId != null)
                .Select(f => new { f.ProviderId, f.PlayerId })
                .ToListAsync();

            var map = new Dictionary<string, int>();
            foreach (var r in rows) map[r.ProviderId] = r.PlayerId;
            return map;
        }

        public async Task<Dictionary<string, int>> MapByNameAndTeamAsync(
            int seasonId, string providerName, IEnumerable<SportsDataPlayerGame> lines, NFLSyncResult result)
        {
            var providerId = await GetProviderIdAsync(providerName);
            var map = await GetProviderPlayerMapAsync(providerName);

            var missing = (lines ?? Enumerable.Empty<SportsDataPlayerGame>())
                .Where(l => !string.IsNullOrEmpty(l.PlayerId) && !map.ContainsKey(l.PlayerId)
                            && !string.IsNullOrEmpty(l.PlayerName) && IsTracked(l.Position))
                .GroupBy(l => l.PlayerId)
                .Select(g => g.First())
                .ToList();

            if (missing.Count == 0) return map;

            var alreadyMapped = new HashSet<int>(map.Values);

            var roster = await (from sp in _db.Set<SeasonPlayer>()
                                join p in _db.Set<Player>() on sp.PlayerId equals p.Id
                                where sp.SeasonId == seasonId
                                select new { p.Id, p.FirstName, p.LastName, sp.TeamId })
                               .AsNoTracking()
                               .ToListAsync();

            foreach (var line in missing)
            {
                var teamId = await GetTeamIdAsync(line.TeamCode);
                var name = Norm(line.PlayerName);

                var hits = roster
                    .Where(r => (!teamId.HasValue || r.TeamId == teamId.Value)
                                && Norm(r.FirstName + " " + r.LastName) == name
                                && !alreadyMapped.Contains(r.Id))
                    .ToList();

                if (hits.Count != 1)
                {
                    result.Skipped++;
                    result.Notes.Add("No " + providerName + " match: " + line.PlayerName + " " + line.Position + " " + line.TeamCode + " (" + line.PlayerId + "), " + hits.Count + " candidates");
                    continue;
                }

                _db.Set<FantasyProviderPlayer>().Add(new FantasyProviderPlayer
                {
                    FantasyProviderId = providerId,
                    PlayerId = hits[0].Id,
                    ProviderId = line.PlayerId
                });

                map[line.PlayerId] = hits[0].Id;
                alreadyMapped.Add(hits[0].Id);
                result.Updated++;
                result.Notes.Add("Mapped " + line.PlayerName + " (" + line.PlayerId + ") to " + hits[0].Id + " by name + team");
            }

            await _db.SaveChangesAsync();
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

        public async Task<NFLSyncResult> EnsureSeasonAsync(int year, DateTime startDate, DateTime endDate, IEnumerable<string> teamCodes)
        {
            var result = new NFLSyncResult();
            var seasonId = SeasonIdFor(year);

            var exists = await _db.Set<Season>().AsNoTracking().AnyAsync(s => s.Id == seasonId);
            if (!exists)
            {
                var title = "NFL " + year.ToString(CultureInfo.InvariantCulture);

                await _db.Database.ExecuteSqlInterpolatedAsync($@"
UPDATE Seasons SET DisplayOrder = DisplayOrder + 1;
INSERT INTO Seasons (Id, [Year], Title, Abbreviation, StartDate, EndDate, IsRegularSeason, YahooId, IsEnabled, DisplayOrder, ESPNYear)
VALUES ({seasonId}, {year}, {title}, {title}, {startDate.Date}, {endDate.Date}, 1, NULL, 1, 1, {year});");

                result.Created++;
                result.Notes.Add("Created season " + seasonId);
            }

            var previousId = await _db.Set<Season>()
                .AsNoTracking()
                .Where(s => s.Id < seasonId)
                .OrderByDescending(s => s.Id)
                .Select(s => (int?)s.Id)
                .FirstOrDefaultAsync();

            var divisions = previousId.HasValue
                ? await _db.Set<SeasonTeam>().AsNoTracking()
                    .Where(st => st.SeasonId == previousId.Value)
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

        public async Task<Dictionary<string, Game>> SyncGamesAsync(int seasonId, IEnumerable<SportsDataGame> games, NFLSyncResult result)
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
                    game = new Game
                    {
                        SeasonId = seasonId,
                        Number = 1,
                        HomeTeamId = homeId,
                        AwayTeamId = awayId
                    };
                    _db.Set<Game>().Add(game);
                    byKey[key] = game;
                }

                var changed = isNew
                    | Set(game.GameDate, local.Date, v => game.GameDate = v)
                    | Set(game.GameTime, local, v => game.GameTime = v)
                    | Set(game.HomeScore, pg.HomeScore ?? 0, v => game.HomeScore = v)
                    | Set(game.AwayScore, pg.AwayScore ?? 0, v => game.AwayScore = v)
                    | Set(game.IsFinished, pg.IsFinished, v => game.IsFinished = v)
                    | Set(game.PercentComplete, pg.IsFinished ? 100 : game.PercentComplete, v => game.PercentComplete = v);

                if (isNew) result.Created++;
                else if (changed) result.Updated++;

                map[pg.GameId] = game;
            }

            await _db.SaveChangesAsync();
            return map;
        }

        public async Task<Dictionary<string, int>> SyncPlayersAsync(int seasonId, IEnumerable<SportsDataPlayer> players, NFLSyncResult result)
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
                if (!string.IsNullOrEmpty(m.ProviderId)) byProviderId[m.ProviderId] = m.PlayerId;

            var mappedPlayerIds = new HashSet<int>(mappings.Select(m => m.PlayerId));

            var unmapped = list.Where(p => !byProviderId.ContainsKey(p.PlayerId) && IsTracked(p.Position)).ToList();

            if (unmapped.Count > 0)
            {
                var pool = await LoadMatchPoolAsync(mappedPlayerIds);

                foreach (var p in unmapped)
                {
                    var match = await MatchAsync(p, pool);
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

                    PoolPlayer matched;
                    if (p.BirthDate.HasValue && pool.TryGetValue(playerId.Value, out matched) && IsPlaceholder(matched.Birthdate))
                    {
                        var existing = await _db.Set<Player>().FindAsync(playerId.Value);
                        if (existing != null)
                        {
                            existing.Birthdate = p.BirthDate.Value.Date;
                            result.Notes.Add("Filled placeholder birthdate for " + existing.FirstName + " " + existing.LastName + " (" + existing.Id + ")");
                        }
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

            return byProviderId;
        }

        private async Task SyncSeasonPlayersAsync(int seasonId, List<SportsDataPlayer> players, Dictionary<string, int> byProviderId, NFLSyncResult result)
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
                if (!IsTracked(p.Position)) continue;

                var teamId = await GetTeamIdAsync(p.TeamCode);
                if (!teamId.HasValue) continue;

                var typeId = IsKicker(p.Position) ? KickerPlayerTypeId : OffensivePlayerTypeId;

                SeasonPlayer sp;
                if (!byPlayer.TryGetValue(playerId, out sp))
                {
                    sp = new SeasonPlayer { SeasonId = seasonId, PlayerId = playerId, TeamId = teamId.Value, PlayerTypeId = typeId };
                    _db.Set<SeasonPlayer>().Add(sp);
                    byPlayer[playerId] = sp;
                    continue;
                }

                if (sp.TeamId != teamId.Value) sp.TeamId = teamId.Value;
            }

            await _db.SaveChangesAsync();
        }

        public async Task<NFLSyncResult> SyncPlayerGamesAsync(
            IEnumerable<SportsDataPlayerGame> lines,
            Dictionary<string, Game> games,
            Dictionary<string, int> players)
        {
            var result = new NFLSyncResult();
            var list = (lines ?? Enumerable.Empty<SportsDataPlayerGame>()).ToList();
            if (list.Count == 0) return result;

            var gameIds = games.Values.Select(g => g.Id).Distinct().ToList();

            var offense = await _db.Set<NFLOffensiveGame>().Where(x => gameIds.Contains(x.GameId)).ToListAsync();
            var kickers = await _db.Set<NFLKickerGame>().Where(x => gameIds.Contains(x.GameId)).ToListAsync();
            var defense = await _db.Set<NFLDefenseGame>().Where(x => gameIds.Contains(x.GameId)).ToListAsync();

            var offenseByKey = offense.ToDictionary(x => Tuple.Create(x.PlayerId, x.GameId));
            var kickerByKey = kickers.ToDictionary(x => Tuple.Create(x.PlayerId, x.GameId));
            var defenseByKey = defense.ToDictionary(x => Tuple.Create(x.PlayerId, x.GameId));

            var teamTotals = new Dictionary<Tuple<int, int>, Dictionary<string, double>>();

            foreach (var line in list)
            {
                Game game;
                if (string.IsNullOrEmpty(line.GameId) || !games.TryGetValue(line.GameId, out game)) continue;

                var teamId = await GetTeamIdAsync(line.TeamCode);
                if (!teamId.HasValue) continue;

                var totalsKey = Tuple.Create(game.Id, teamId.Value);
                Dictionary<string, double> totals;
                if (!teamTotals.TryGetValue(totalsKey, out totals))
                {
                    totals = new Dictionary<string, double>();
                    teamTotals[totalsKey] = totals;
                }
                foreach (var kv in line.Stats)
                {
                    double v;
                    totals.TryGetValue(kv.Key, out v);
                    totals[kv.Key] = v + kv.Value;
                }

                int playerId;
                if (string.IsNullOrEmpty(line.PlayerId) || !players.TryGetValue(line.PlayerId, out playerId)) continue;

                var key = Tuple.Create(playerId, game.Id);

                if (IsKicker(line.Position))
                {
                    NFLKickerGame k;
                    if (!kickerByKey.TryGetValue(key, out k))
                    {
                        k = new NFLKickerGame { PlayerId = playerId, GameId = game.Id };
                        _db.Set<NFLKickerGame>().Add(k);
                        kickerByKey[key] = k;
                        result.Created++;
                    }
                    else result.Updated++;

                    k.TeamId = teamId.Value;
                    FillKicker(k, line);
                }
                else if (IsOffense(line.Position))
                {
                    NFLOffensiveGame o;
                    if (!offenseByKey.TryGetValue(key, out o))
                    {
                        o = new NFLOffensiveGame { PlayerId = playerId, GameId = game.Id };
                        _db.Set<NFLOffensiveGame>().Add(o);
                        offenseByKey[key] = o;
                        result.Created++;
                    }
                    else result.Updated++;

                    o.TeamId = teamId.Value;
                    FillOffense(o, line);
                }
            }

            var defensePlayers = await GetDefensePlayerIdsAsync();

            foreach (var game in games.Values.Distinct())
            {
                if (!game.HomeTeamId.HasValue || !game.AwayTeamId.HasValue) continue;

                foreach (var side in new[] { game.HomeTeamId.Value, game.AwayTeamId.Value })
                {
                    var opponent = side == game.HomeTeamId.Value ? game.AwayTeamId.Value : game.HomeTeamId.Value;

                    Dictionary<string, double> own;
                    Dictionary<string, double> opp;
                    if (!teamTotals.TryGetValue(Tuple.Create(game.Id, side), out own)) continue;
                    teamTotals.TryGetValue(Tuple.Create(game.Id, opponent), out opp);
                    opp = opp ?? new Dictionary<string, double>();

                    int defPlayerId;
                    if (!defensePlayers.TryGetValue(side, out defPlayerId))
                    {
                        result.Skipped++;
                        result.Notes.Add("No DEF player for team " + side);
                        continue;
                    }

                    var key = Tuple.Create(defPlayerId, game.Id);
                    NFLDefenseGame d;
                    if (!defenseByKey.TryGetValue(key, out d))
                    {
                        d = new NFLDefenseGame { PlayerId = defPlayerId, GameId = game.Id };
                        _db.Set<NFLDefenseGame>().Add(d);
                        defenseByKey[key] = d;
                        result.Created++;
                    }
                    else result.Updated++;

                    d.TeamId = side;
                    var allowed = side == game.HomeTeamId.Value ? game.AwayScore : game.HomeScore;
                    FillDefense(d, own, opp, allowed);
                }
            }

            await _db.SaveChangesAsync();
            return result;
        }

        private async Task<Dictionary<int, int>> GetDefensePlayerIdsAsync()
        {
            var teams = await _db.Set<Team>().AsNoTracking().ToListAsync();
            var codes = teams.Select(t => t.Code.Trim()).ToList();

            var defPlayers = await _db.Set<Player>().AsNoTracking()
                .Where(p => p.LastName == "DEF" && codes.Contains(p.FirstName))
                .ToListAsync();

            var result = new Dictionary<int, int>();
            foreach (var t in teams)
            {
                var p = defPlayers.FirstOrDefault(x => string.Equals(x.FirstName.Trim(), t.Code.Trim(), StringComparison.OrdinalIgnoreCase));
                if (p != null) result[t.Id] = p.Id;
            }
            return result;
        }

        private static void FillOffense(NFLOffensiveGame o, SportsDataPlayerGame s)
        {
            PutByte(s, "PassAttempts", v => o.PassAttempts = v);
            PutByte(s, "PassCompletions", v => o.PassCompletions = v);
            PutInt(s, "PassYards", v => o.PassYards = v);
            PutByte(s, "PassTD", v => o.PassTD = v);
            PutByte(s, "PassInt", v => o.PassInt = v);
            PutByte(s, "PassSacks", v => o.PassSacks = v);
            PutInt(s, "PassSackYards", v => o.PassSackYards = v);
            PutInt(s, "PassAirYards", v => o.PassAirYards = v);
            PutByte(s, "RushAttempts", v => o.RushAttempts = v);
            PutInt(s, "RushYards", v => o.RushYards = v);
            PutByte(s, "RushTD", v => o.RushTD = v);
            PutByte(s, "RushFumbles", v => o.RushFumbles = v);
            PutByte(s, "RecTargets", v => o.RecTargets = v);
            PutByte(s, "RecReceptions", v => o.RecReceptions = v);
            PutInt(s, "RecYards", v => o.RecYards = v);
            PutByte(s, "RecTD", v => o.RecTD = v);
            PutInt(s, "RecAirYards", v => o.RecAirYards = v);
            PutInt(s, "RecYardsAfterCatch", v => o.RecYardsAfterCatch = v);
            PutByte(s, "Fumbles", v => o.Fumbles = v);
            PutByte(s, "FumblesLost", v => o.FumblesLost = v);
            PutByte(s, "ReturnReturns", v => o.ReturnReturns = v);
            PutInt(s, "ReturnYards", v => o.ReturnYards = v);
            PutByte(s, "ReturnTD", v => o.ReturnTD = v);
        }

        private static void FillKicker(NFLKickerGame k, SportsDataPlayerGame s)
        {
            PutByte(s, "FieldGoals", v => k.FieldGoals = v);
            PutByte(s, "FieldGoalsMade", v => k.FieldGoalsMade = v);
            PutByte(s, "FieldGoals0to19", v => k.FieldGoals0to19 = v);
            PutByte(s, "FieldGoals20to29", v => k.FieldGoals20to29 = v);
            PutByte(s, "FieldGoals30to39", v => k.FieldGoals30to39 = v);
            PutByte(s, "FieldGoals40to49", v => k.FieldGoals40to49 = v);
            PutByte(s, "FieldGoals50", v => k.FieldGoals50 = v);
            PutByte(s, "FieldGoalsBlocked", v => k.FieldGoalsBlocked = v);
            PutByte(s, "FieldGoalsLongest", v => k.FieldGoalsLongest = v);
            PutByte(s, "ExtraPointsAttempts", v => k.ExtraPointsAttempts = v);
            PutByte(s, "ExtraPointsBlocked", v => k.ExtraPointsBlocked = v);
            PutByte(s, "ExtraPointsMade", v => k.ExtraPointsMade = v);

            if (s.Stats.ContainsKey("FieldGoals0to19") || s.Stats.ContainsKey("FieldGoals20to29") || s.Stats.ContainsKey("FieldGoals30to39"))
                k.FieldGoals0to39 = ToByte(s.Get("FieldGoals0to19") + s.Get("FieldGoals20to29") + s.Get("FieldGoals30to39"));
        }

        private static void PutByte(SportsDataPlayerGame s, string key, Action<byte?> set)
        {
            if (s.Stats.ContainsKey(key)) set(ToByte(s.Get(key)));
        }

        private static void PutInt(SportsDataPlayerGame s, string key, Action<int?> set)
        {
            if (s.Stats.ContainsKey(key)) set(Convert.ToInt32(Math.Round(s.Get(key))));
        }

        private static void FillDefense(NFLDefenseGame d, Dictionary<string, double> own, Dictionary<string, double> opp, int allowed)
        {
            d.Sacks = ToByte(Sum(own, "Sacks"));
            d.Interceptions = ToByte(Sum(own, "Interceptions"));
            d.FumbleRecoveries = ToByte(Sum(own, "FumbleRecoveries"));
            d.Safeties = ToByte(Sum(own, "Safeties"));
            d.Touchdowns = ToByte(Sum(own, "InterceptionTouchdowns") + Sum(own, "FumbleTouchdowns")
                                  + Sum(own, "KickReturnTouchdowns") + Sum(own, "PuntReturnTouchdowns"));
            d.Points = ToByte(allowed);

            d.PassAttempts = ToByte(Sum(opp, "PassAttempts"));
            d.PassCompletion = ToByte(Sum(opp, "PassCompletions"));
            d.PassYards = ToShort(Sum(opp, "PassYards"));
            d.PassTouchdowns = ToByte(Sum(opp, "PassTD"));
            d.RushAttempts = ToByte(Sum(opp, "RushAttempts"));
            d.RushYards = ToShort(Sum(opp, "RushYards"));
            d.RushTouchdowns = ToByte(Sum(opp, "RushTD"));
            d.PassSacks = ToByte(Sum(opp, "PassSacks"));

            d.Points0 = Flag(allowed == 0);
            d.Points1to6 = Flag(allowed >= 1 && allowed <= 6);
            d.Points7to13 = Flag(allowed >= 7 && allowed <= 13);
            d.Points14to20 = Flag(allowed >= 14 && allowed <= 20);
            d.Points21to27 = Flag(allowed >= 21 && allowed <= 27);
            d.Points28to34 = Flag(allowed >= 28 && allowed <= 34);
            d.Points35 = Flag(allowed >= 35);
        }

        private class PoolPlayer
        {
            public int Id;
            public string First;
            public string Last;
            public DateTime Birthdate;
            public HashSet<int> PositionIds = new HashSet<int>();
            public int? LastTeamId;
        }

        private async Task<Dictionary<int, PoolPlayer>> LoadMatchPoolAsync(HashSet<int> alreadyMapped)
        {
            var players = await _db.Set<Player>().AsNoTracking()
                .Where(p => p.LastName != "DEF")
                .Select(p => new { p.Id, p.FirstName, p.LastName, p.Birthdate })
                .ToListAsync();

            var pool = new Dictionary<int, PoolPlayer>();
            foreach (var p in players)
            {
                if (alreadyMapped.Contains(p.Id)) continue;
                pool[p.Id] = new PoolPlayer { Id = p.Id, First = Norm(p.FirstName), Last = Norm(p.LastName), Birthdate = p.Birthdate.Date };
            }

            var positions = await _db.Set<PlayerDefaultPosition>().AsNoTracking()
                .Select(x => new { x.PlayerId, x.PositionId })
                .ToListAsync();
            foreach (var x in positions)
            {
                PoolPlayer pp;
                if (pool.TryGetValue(x.PlayerId, out pp)) pp.PositionIds.Add(x.PositionId);
            }

            var offense = await _db.Set<NFLOffensiveGame>().AsNoTracking()
                .Select(x => new { x.PlayerId, x.TeamId, x.Game.GameDate })
                .ToListAsync();
            var kicking = await _db.Set<NFLKickerGame>().AsNoTracking()
                .Select(x => new { x.PlayerId, x.TeamId, x.Game.GameDate })
                .ToListAsync();

            foreach (var x in offense.Concat(kicking).OrderBy(x => x.GameDate))
            {
                PoolPlayer pp;
                if (pool.TryGetValue(x.PlayerId, out pp)) pp.LastTeamId = x.TeamId;
            }

            return pool;
        }

        private async Task<Tuple<int?, string>> MatchAsync(SportsDataPlayer p, Dictionary<int, PoolPlayer> pool)
        {
            var first = Norm(p.FirstName);
            var last = Norm(p.LastName);
            var birth = p.BirthDate.HasValue ? p.BirthDate.Value.Date : (DateTime?)null;

            var sameName = pool.Values.Where(c => c.First == first && c.Last == last).ToList();

            if (birth.HasValue)
            {
                var exact = sameName.Where(c => c.Birthdate == birth.Value).ToList();
                if (exact.Count == 1) return Tuple.Create((int?)exact[0].Id, "exact");
            }

            int positionId;
            var hasPosition = PositionIds.TryGetValue((p.Position ?? "").Trim(), out positionId);
            var teamId = await GetTeamIdAsync(p.TeamCode);

            var samePosition = sameName
                .Where(c => (c.PositionIds.Count == 0 && c.LastTeamId.HasValue) || (hasPosition && c.PositionIds.Contains(positionId)))
                .Where(c => !birth.HasValue
                            || IsPlaceholder(c.Birthdate)
                            || Math.Abs((c.Birthdate - birth.Value).TotalDays) <= MaxBirthdateGapDays
                            || (teamId.HasValue && c.LastTeamId == teamId.Value))
                .ToList();

            if (samePosition.Count > 1 && teamId.HasValue)
            {
                var sameTeam = samePosition.Where(c => c.LastTeamId == teamId.Value).ToList();
                if (sameTeam.Count == 1)
                    return Tuple.Create((int?)sameTeam[0].Id, "name + position + team, birthdate differs");
            }

            if (samePosition.Count == 1)
            {
                var detail = (teamId.HasValue && samePosition[0].LastTeamId == teamId.Value ? "name + position + team, birthdate " : "name + position, birthdate ") + samePosition[0].Birthdate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
                             + " vs " + (birth.HasValue ? birth.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) : "none");
                return Tuple.Create((int?)samePosition[0].Id, detail);
            }

            if (birth.HasValue && sameName.Count == 0)
            {
                var sameBirthLast = pool.Values.Where(c => c.Last == last && c.Birthdate == birth.Value).ToList();
                if (sameBirthLast.Count == 1)
                    return Tuple.Create((int?)sameBirthLast[0].Id, "last name + birthdate");
            }

            if (sameName.Count > 0)
                return Tuple.Create((int?)null, "matches " + sameName.Count + " by name (" + samePosition.Count + " at this position within 2 years), not matched: " + string.Join(", ", sameName.Select(c => c.Id + " " + c.Birthdate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture))));

            return Tuple.Create((int?)null, (string)null);
        }

        private Player CreatePlayer(SportsDataPlayer p)
        {
            var player = new Player
            {
                FirstName = Truncate(p.FirstName, 80),
                LastName = Truncate(p.LastName, 80),
                Birthdate = p.BirthDate.Value.Date,
                Height = Clamp(p.HeightInches ?? 72, 50, 100),
                Weight = Clamp(p.Weight ?? 220, 100, 500),
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

        private static bool IsPlaceholder(DateTime birthdate)
        {
            return birthdate.Year < 1950;
        }

        private static bool IsTracked(string position)
        {
            return IsOffense(position) || IsKicker(position);
        }

        private static bool IsOffense(string position)
        {
            var p = (position ?? "").Trim().ToUpperInvariant();
            return p == "QB" || p == "RB" || p == "FB" || p == "WR" || p == "TE";
        }

        private static bool IsKicker(string position)
        {
            var p = (position ?? "").Trim().ToUpperInvariant();
            return p == "K" || p == "PK";
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

        private static double Sum(Dictionary<string, double> totals, string key)
        {
            double v;
            return totals.TryGetValue(key, out v) ? v : 0;
        }

        private static byte? ToByte(double v)
        {
            return (byte)Math.Max(0, Math.Min(255, Math.Round(v)));
        }

        private static short? ToShort(double v)
        {
            return (short)Math.Max(short.MinValue, Math.Min(short.MaxValue, Math.Round(v)));
        }

        private static byte? Flag(bool on)
        {
            return on ? (byte)1 : (byte)0;
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
