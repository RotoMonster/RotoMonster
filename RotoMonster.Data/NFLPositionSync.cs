using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RotoMonster.Core;
using RotoMonsterExternalAPIs.Client.Models.Providers;

namespace RotoMonster.Data
{
    public class NFLPositionSync
    {
        public const int YahooSource = 1;
        public const int EspnSource = 2;
        public const int FanTraxSource = 3;

        public const int YahooProvider = 1;
        public const int EspnProvider = 2;
        public const int FanTraxProvider = 4;
        public const int SportRadarProvider = 5;

        private static readonly Dictionary<string, int> PositionIds =
            new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
            {
                { "QB", 1 },
                { "RB", 2 }, { "HB", 2 }, { "FB", 2 },
                { "WR", 3 },
                { "TE", 4 },
                { "K", 5 }, { "PK", 5 },
                { "DB", 10 },
                { "DL", 11 },
                { "LB", 12 }, { "ILB", 12 }, { "OLB", 12 }, { "MLB", 12 },
                { "DT", 13 }, { "NT", 13 },
                { "DE", 14 }, { "EDGE", 14 },
                { "CB", 15 },
                { "S", 16 }, { "SS", 16 }, { "FS", 16 }, { "SAF", 16 },
                { "OL", 17 }, { "OT", 17 }, { "T", 17 }, { "G", 17 }, { "OG", 17 }, { "C", 17 }, { "LS", 17 },
                { "P", 24 },
                { "DEF", 25 }, { "DST", 25 }, { "D/ST", 25 }, { "D", 25 }
            };

        private readonly RMDBContext _db;

        public NFLPositionSync(RMDBContext db)
        {
            _db = db;
        }

        public static int? PositionIdFor(string code)
        {
            int id;
            return !string.IsNullOrWhiteSpace(code) && PositionIds.TryGetValue(code.Trim(), out id) ? id : (int?)null;
        }

        public async Task<Dictionary<string, int>> ResolveAsync(IEnumerable<ProviderPlayerPosition> feed, int providerId, bool useSharedIds, NFLSyncResult result)
        {
            var players = feed.Where(p => !string.IsNullOrEmpty(p.ProviderPlayerId)).ToList();

            var own = await MapAsync(providerId);
            var mappedPlayers = new HashSet<int>(own.Values);
            var sportRadar = useSharedIds ? await MapAsync(SportRadarProvider) : new Dictionary<string, int>();
            var yahoo = useSharedIds ? await MapAsync(YahooProvider) : new Dictionary<string, int>();
            var nameIndex = useSharedIds ? await BuildNameIndexAsync() : null;
            _feedLastCounts = players
                .GroupBy(x => LastKey(x.Name) + "|" + (x.Team ?? "").Trim().ToUpperInvariant())
                .ToDictionary(g => g.Key, g => g.Count());

            var resolved = new Dictionary<string, int>();

            foreach (var p in players)
            {
                if (resolved.ContainsKey(p.ProviderPlayerId)) continue;

                int playerId;
                if (own.TryGetValue(p.ProviderPlayerId, out playerId))
                {
                    resolved[p.ProviderPlayerId] = playerId;
                    continue;
                }

                var found = (!string.IsNullOrEmpty(p.SportRadarId) && sportRadar.TryGetValue(p.SportRadarId, out playerId))
                            || (!string.IsNullOrEmpty(p.StatsIncId) && yahoo.TryGetValue(p.StatsIncId, out playerId));

                if (!found && nameIndex != null)
                    found = TryNameMatch(p, nameIndex, mappedPlayers, out playerId);

                if (!found)
                {
                    result.Skipped++;
                    continue;
                }

                resolved[p.ProviderPlayerId] = playerId;

                if (mappedPlayers.Add(playerId))
                {
                    _db.Set<FantasyProviderPlayer>().Add(new FantasyProviderPlayer
                    {
                        FantasyProviderId = providerId,
                        PlayerId = playerId,
                        ProviderId = p.ProviderPlayerId
                    });
                    result.Created++;
                }
            }

            if (result.Created > 0) await _db.SaveChangesAsync();
            return resolved;
        }

        private static readonly HashSet<string> NameSuffixes = new HashSet<string> { "jr", "sr", "ii", "iii", "iv", "v" };

        private Dictionary<string, List<int>> _lastIndex;
        private Dictionary<string, int> _feedLastCounts;

        private static string LastKey(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "";
            var s = name.ToLowerInvariant().Replace(".", " ").Replace("'", "").Replace("-", " ").Replace(",", " ");
            var parts = s.Split(' ', StringSplitOptions.RemoveEmptyEntries).Where(x => !NameSuffixes.Contains(x)).ToList();
            return parts.Count == 0 ? "" : parts[parts.Count - 1];
        }

        private static string NameKey(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "";
            var s = name.ToLowerInvariant().Replace(".", " ").Replace("'", "").Replace("-", " ").Replace(",", " ");
            return string.Concat(s.Split(' ', StringSplitOptions.RemoveEmptyEntries).Where(x => !NameSuffixes.Contains(x)));
        }

        private async Task<Dictionary<string, List<(int Id, string Team)>>> BuildNameIndexAsync()
        {
            var players = await _db.Set<Player>().AsNoTracking()
                .Where(x => x.LastName != "DEF")
                .Select(x => new { x.Id, x.FirstName, x.LastName })
                .ToListAsync();

            var teamCodes = (await _db.Set<Team>().AsNoTracking().Select(t => new { t.Id, t.Code }).ToListAsync())
                .GroupBy(t => t.Id)
                .ToDictionary(g => g.Key, g => (g.First().Code ?? "").Trim().ToUpperInvariant());

            var latestTeam = (await _db.Set<SeasonPlayer>().AsNoTracking().Select(sp => new { sp.PlayerId, sp.SeasonId, sp.TeamId }).ToListAsync())
                .GroupBy(sp => sp.PlayerId)
                .ToDictionary(g => g.Key, g => g.OrderByDescending(x => x.SeasonId).First().TeamId);

            var index = new Dictionary<string, List<(int Id, string Team)>>();
            _lastIndex = new Dictionary<string, List<int>>();
            foreach (var x in players)
            {
                var key = NameKey((x.FirstName ?? "") + " " + (x.LastName ?? ""));
                if (key.Length == 0) continue;

                string team = "";
                if (latestTeam.TryGetValue(x.Id, out var teamId) && teamCodes.TryGetValue(teamId, out var code))
                    team = code;

                if (!index.TryGetValue(key, out var list))
                    index[key] = list = new List<(int Id, string Team)>();
                list.Add((x.Id, team));

                var lastKey = LastKey(x.LastName);
                if (team.Length > 0 && lastKey.Length > 0)
                {
                    if (!_lastIndex.TryGetValue(lastKey + "|" + team, out var ids))
                        _lastIndex[lastKey + "|" + team] = ids = new List<int>();
                    ids.Add(x.Id);
                }
            }
            return index;
        }

        private bool TryNameMatch(ProviderPlayerPosition p, Dictionary<string, List<(int Id, string Team)>> index, HashSet<int> alreadyMapped, out int playerId)
        {
            playerId = 0;
            if (p.Positions.Any(x => string.Equals(x, "DEF", StringComparison.OrdinalIgnoreCase))) return false;

            var key = NameKey(p.Name);
            if (key.Length == 0) return false;
            if (!index.TryGetValue(key, out var list)) return TryLastNameMatch(p, alreadyMapped, out playerId);

            var candidates = list.Where(c => !alreadyMapped.Contains(c.Id)).ToList();
            if (candidates.Count == 0) return false;

            var team = (p.Team ?? "").Trim().ToUpperInvariant();
            var onTeam = team.Length == 0 ? new List<(int Id, string Team)>() : candidates.Where(c => c.Team == team).ToList();

            if (onTeam.Count == 1) { playerId = onTeam[0].Id; return true; }
            if (onTeam.Count == 0 && candidates.Count == 1) { playerId = candidates[0].Id; return true; }
            return false;
        }

        private bool TryLastNameMatch(ProviderPlayerPosition p, HashSet<int> alreadyMapped, out int playerId)
        {
            playerId = 0;
            var team = (p.Team ?? "").Trim().ToUpperInvariant();
            var lastKey = LastKey(p.Name);
            if (team.Length == 0 || lastKey.Length == 0 || _lastIndex == null) return false;

            var key = lastKey + "|" + team;
            if (_feedLastCounts == null || !_feedLastCounts.TryGetValue(key, out var feedCount) || feedCount != 1) return false;
            if (!_lastIndex.TryGetValue(key, out var ids)) return false;

            var free = ids.Where(i => !alreadyMapped.Contains(i)).ToList();
            if (free.Count != 1) return false;

            playerId = free[0];
            return true;
        }

        private async Task<Dictionary<string, int>> MapAsync(int providerId)
        {
            var rows = await _db.Set<FantasyProviderPlayer>().AsNoTracking()
                .Where(f => f.FantasyProviderId == providerId && f.ProviderId != null)
                .Select(f => new { f.ProviderId, f.PlayerId })
                .ToListAsync();

            var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (var r in rows)
            {
                if (!map.ContainsKey(r.ProviderId)) map[r.ProviderId] = r.PlayerId;
            }
            return map;
        }

        public async Task<NFLSyncResult> SyncSourceAsync(int seasonId, int positionSourceId, IEnumerable<ProviderPlayerPosition> feed, Dictionary<string, int> resolved)
        {
            var result = new NFLSyncResult();
            var desired = new HashSet<(int PlayerId, int PositionId)>();
            var unknown = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            foreach (var p in feed)
            {
                int playerId;
                if (string.IsNullOrEmpty(p.ProviderPlayerId) || !resolved.TryGetValue(p.ProviderPlayerId, out playerId))
                {
                    result.Skipped++;
                    continue;
                }

                foreach (var code in p.Positions)
                {
                    var positionId = PositionIdFor(code);
                    if (positionId == null)
                    {
                        unknown[code] = unknown.TryGetValue(code, out var n) ? n + 1 : 1;
                        continue;
                    }
                    desired.Add((playerId, positionId.Value));
                }
            }

            var inFeed = new HashSet<int>(desired.Select(d => d.PlayerId));

            var existing = await _db.Set<PositionSourcePlayer>()
                .Where(x => x.SeasonId == seasonId && x.PositionSourceId == positionSourceId)
                .ToListAsync();

            var have = new HashSet<(int, int)>(existing.Select(x => (x.PlayerId, x.PositionId)));

            foreach (var row in existing)
            {
                if (inFeed.Contains(row.PlayerId) && !desired.Contains((row.PlayerId, row.PositionId)))
                {
                    _db.Set<PositionSourcePlayer>().Remove(row);
                    result.Updated++;
                }
            }

            foreach (var d in desired)
            {
                if (have.Contains((d.PlayerId, d.PositionId))) continue;
                _db.Set<PositionSourcePlayer>().Add(new PositionSourcePlayer
                {
                    SeasonId = seasonId,
                    PositionSourceId = positionSourceId,
                    PlayerId = d.PlayerId,
                    PositionId = d.PositionId
                });
                result.Created++;
            }

            await _db.SaveChangesAsync();

            foreach (var u in unknown.OrderByDescending(u => u.Value))
                result.Notes.Add($"Unknown position code '{u.Key}' ({u.Value})");

            return result;
        }

        public static Dictionary<int, int> PrimaryPositions(IEnumerable<ProviderPlayerPosition> feed, Dictionary<string, int> resolved)
        {
            var map = new Dictionary<int, int>();
            foreach (var p in feed)
            {
                int playerId;
                if (string.IsNullOrEmpty(p.ProviderPlayerId) || !resolved.TryGetValue(p.ProviderPlayerId, out playerId)) continue;
                if (map.ContainsKey(playerId)) continue;

                foreach (var code in p.Positions)
                {
                    var positionId = PositionIdFor(code);
                    if (positionId != null)
                    {
                        map[playerId] = positionId.Value;
                        break;
                    }
                }
            }
            return map;
        }

        public async Task<NFLSyncResult> FillMissingDefaultsAsync(IEnumerable<Dictionary<int, int>> sourcesInPriority)
        {
            var result = new NFLSyncResult();
            var sources = sourcesInPriority.ToList();

            var withDefault = new HashSet<int>(await _db.Set<PlayerDefaultPosition>().AsNoTracking()
                .Select(d => d.PlayerId).Distinct().ToListAsync());

            var allPlayers = await _db.Set<Player>().AsNoTracking().Select(p => p.Id).ToListAsync();

            foreach (var playerId in allPlayers)
            {
                if (withDefault.Contains(playerId)) continue;

                int positionId = 0;
                var found = false;
                foreach (var source in sources)
                {
                    if (source.TryGetValue(playerId, out positionId))
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    result.Skipped++;
                    continue;
                }

                _db.Set<PlayerDefaultPosition>().Add(new PlayerDefaultPosition { PlayerId = playerId, PositionId = positionId });
                result.Created++;
            }

            if (result.Created > 0) await _db.SaveChangesAsync();
            return result;
        }
    }
}
