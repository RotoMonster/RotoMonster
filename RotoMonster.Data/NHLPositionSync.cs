using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RotoMonster.Core;
using RotoMonsterExternalAPIs.Client.Models.Providers;

namespace RotoMonster.Data
{
    public class NHLPositionSync
    {
        public const int YahooProvider = 1;
        public const int EspnProvider = 2;
        public const int FanTraxProvider = 4;
        public const int SportRadarProvider = 5;
        public const int NhlProvider = 7;

        private static readonly HashSet<string> Suffixes = new HashSet<string> { "jr", "sr", "ii", "iii", "iv" };

        private readonly RMDBContext _db;
        private Dictionary<string, int> _positionIds;
        private Dictionary<string, List<(int PlayerId, int TeamId)>> _rosterByName;

        public NHLPositionSync(RMDBContext db)
        {
            _db = db;
        }

        public async Task<int?> GetPositionSourceIdAsync(int fantasyProviderId)
        {
            var rows = await QueryAsync("SELECT TOP 1 Id FROM PositionSources WHERE FantasyProviderId = @p ORDER BY Id", ("@p", fantasyProviderId));
            return rows.Count > 0 ? Convert.ToInt32(rows[0][0]) : (int?)null;
        }

        public async Task<int?> PositionIdForAsync(string code)
        {
            if (_positionIds == null)
            {
                _positionIds = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                foreach (var r in await QueryAsync("SELECT Id, Abbreviation FROM Positions WHERE IsActualPosition = 1"))
                    _positionIds[Convert.ToString(r[1]).Trim()] = Convert.ToInt32(r[0]);
            }
            int id;
            return !string.IsNullOrWhiteSpace(code) && _positionIds.TryGetValue(code.Trim(), out id) ? id : (int?)null;
        }

        public async Task<Dictionary<string, int>> ResolveAsync(int seasonId, IEnumerable<ProviderPlayerPosition> feed, int providerId,
            bool useSharedIds, bool matchByName, Func<string, Task<int?>> teamIdFor, NHLSyncResult result)
        {
            var players = feed.Where(p => !string.IsNullOrEmpty(p.ProviderPlayerId)).ToList();

            var own = await MapAsync(providerId);
            var mappedPlayers = new HashSet<int>(own.Values);
            var sportRadar = useSharedIds ? await MapAsync(SportRadarProvider) : new Dictionary<string, int>();
            var yahoo = useSharedIds ? await MapAsync(YahooProvider) : new Dictionary<string, int>();
            if (matchByName) await LoadRosterAsync(seasonId);

            var resolved = new Dictionary<string, int>();
            var byName = 0;

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

                if (!found && matchByName)
                {
                    var teamId = string.IsNullOrEmpty(p.Team) ? null : await teamIdFor(p.Team);
                    var match = MatchByName(p.Name, teamId);
                    if (match.HasValue)
                    {
                        playerId = match.Value;
                        found = true;
                        byName++;
                    }
                }

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
            if (byName > 0) result.Notes.Add($"{byName} matched by name on the current roster");
            return resolved;
        }

        private int? MatchByName(string fullName, int? teamId)
        {
            var key = NameKey(fullName);
            if (key == null) return null;

            List<(int PlayerId, int TeamId)> candidates;
            if (!_rosterByName.TryGetValue(key, out candidates)) return null;
            if (candidates.Count == 1) return candidates[0].PlayerId;

            if (teamId.HasValue)
            {
                var onTeam = candidates.Where(c => c.TeamId == teamId.Value).ToList();
                if (onTeam.Count == 1) return onTeam[0].PlayerId;
            }
            return null;
        }

        private async Task LoadRosterAsync(int seasonId)
        {
            if (_rosterByName != null) return;

            var rows = await (from sp in _db.Set<SeasonPlayer>().AsNoTracking()
                              join pl in _db.Set<Player>().AsNoTracking() on sp.PlayerId equals pl.Id
                              where sp.SeasonId == seasonId
                              select new { pl.Id, pl.FirstName, pl.LastName, sp.TeamId }).ToListAsync();

            _rosterByName = new Dictionary<string, List<(int, int)>>();
            foreach (var r in rows)
            {
                var key = NameKey(r.FirstName + " " + r.LastName);
                if (key == null) continue;
                List<(int, int)> list;
                if (!_rosterByName.TryGetValue(key, out list)) _rosterByName[key] = list = new List<(int, int)>();
                if (!list.Any(x => x.Item1 == r.Id)) list.Add((r.Id, r.TeamId));
            }
        }

        private static string NameKey(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return null;
            var decomposed = name.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();
            foreach (var c in decomposed)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.NonSpacingMark) continue;
                if (char.IsLetter(c)) sb.Append(char.ToLowerInvariant(c));
                else if (char.IsWhiteSpace(c) || c == '-') sb.Append(' ');
            }
            var parts = sb.ToString().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .Where(w => !Suffixes.Contains(w)).ToList();
            return parts.Count == 0 ? null : string.Join(" ", parts);
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
                var id = r.ProviderId.Trim();
                if (!map.ContainsKey(id)) map[id] = r.PlayerId;
            }
            return map;
        }

        public async Task<NHLSyncResult> SyncSourceAsync(int seasonId, int positionSourceId, IEnumerable<ProviderPlayerPosition> feed, Dictionary<string, int> resolved)
        {
            var result = new NHLSyncResult();
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
                    var positionId = await PositionIdForAsync(code);
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

        public async Task<Dictionary<int, int>> PrimaryPositionsAsync(IEnumerable<ProviderPlayerPosition> feed, Dictionary<string, int> resolved)
        {
            var map = new Dictionary<int, int>();
            foreach (var p in feed)
            {
                int playerId;
                if (string.IsNullOrEmpty(p.ProviderPlayerId) || !resolved.TryGetValue(p.ProviderPlayerId, out playerId)) continue;
                if (map.ContainsKey(playerId)) continue;

                foreach (var code in p.Positions)
                {
                    var positionId = await PositionIdForAsync(code);
                    if (positionId != null)
                    {
                        map[playerId] = positionId.Value;
                        break;
                    }
                }
            }
            return map;
        }

        public async Task<NHLSyncResult> FillMissingDefaultsAsync(IEnumerable<Dictionary<int, int>> sourcesInPriority)
        {
            var result = new NHLSyncResult();
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

        private async Task<List<object[]>> QueryAsync(string sql, params (string Name, object Value)[] parameters)
        {
            var conn = _db.Database.GetDbConnection();
            if (conn.State != ConnectionState.Open) await conn.OpenAsync();
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = sql;
                foreach (var (name, value) in parameters)
                {
                    var p = cmd.CreateParameter();
                    p.ParameterName = name;
                    p.Value = value ?? DBNull.Value;
                    cmd.Parameters.Add(p);
                }
                var rows = new List<object[]>();
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var values = new object[reader.FieldCount];
                        reader.GetValues(values);
                        rows.Add(values);
                    }
                }
                return rows;
            }
        }
    }
}
