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
