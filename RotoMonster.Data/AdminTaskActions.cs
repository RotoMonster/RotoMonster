using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RotoMonster.Core;

namespace RotoMonster.Data
{
    internal static class ActionJson
    {
        public static bool TryGet(JsonElement data, string name, out JsonElement value)
        {
            value = default;
            if (data.ValueKind != JsonValueKind.Object) return false;
            foreach (var p in data.EnumerateObject())
            {
                if (string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase) && p.Value.ValueKind != JsonValueKind.Null)
                {
                    value = p.Value;
                    return true;
                }
            }
            return false;
        }

        public static int? Int(JsonElement data, string name)
        {
            if (!TryGet(data, name, out var v)) return null;
            if (v.ValueKind == JsonValueKind.Number && v.TryGetInt32(out var n)) return n;
            if (v.ValueKind == JsonValueKind.String && int.TryParse(v.GetString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out n)) return n;
            return null;
        }

        public static string Str(JsonElement data, string name)
        {
            if (!TryGet(data, name, out var v)) return null;
            var s = v.ValueKind == JsonValueKind.String ? v.GetString() : v.GetRawText();
            return string.IsNullOrWhiteSpace(s) ? null : s.Trim();
        }

        public static bool Bool(JsonElement data, string name)
        {
            if (!TryGet(data, name, out var v)) return false;
            if (v.ValueKind == JsonValueKind.True) return true;
            if (v.ValueKind == JsonValueKind.String && bool.TryParse(v.GetString(), out var b)) return b;
            return false;
        }

        public static DateTime? Date(JsonElement data, string name)
        {
            var s = Str(data, name);
            return s != null && DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.None, out var d) ? d : (DateTime?)null;
        }
    }

    public class PostInjuryAction : IAdminTaskAction
    {
        private readonly RMDBContext _db;
        private readonly IRMData _data;

        public PostInjuryAction(RMDBContext db, IRMData data)
        {
            _db = db;
            _data = data;
        }

        public string ActionType => "PostInjury";

        public async Task<string> RunAsync(AdminTask task, JsonElement data, CancellationToken ct)
        {
            var playerId = ActionJson.Int(data, "playerId") ?? throw new InvalidOperationException("playerId is missing");
            var status = ActionJson.Str(data, "status") ?? throw new InvalidOperationException("status is missing");

            var player = await _db.Players.AsNoTracking().FirstOrDefaultAsync(p => p.Id == playerId, ct)
                ?? throw new InvalidOperationException($"Player {playerId} not found");
            var type = _data.GetPlayerStatusTypeByName(status)
                ?? throw new InvalidOperationException($"No status type for {status}");

            var now = DateTime.UtcNow;
            var active = await _db.PlayerStatuses.Where(s => s.PlayerId == playerId && s.IsActive).ToListAsync(ct);
            foreach (var s in active)
            {
                s.IsActive = false;
                s.DateDeactivated = now;
            }

            _db.PlayerStatuses.Add(new PlayerStatus
            {
                PlayerId = playerId,
                GameId = ActionJson.Int(data, "gameId"),
                IsActive = true,
                PlayerStatusTypeId = type.Id,
                DateAdded = now,
                Comment = ActionJson.Str(data, "comment") ?? ActionJson.Str(data, "injury"),
                Source = ActionJson.Str(data, "source") ?? "RotoMonster",
                SourceUrl = ActionJson.Str(data, "sourceUrl"),
                EstimatedReturnDate = ActionJson.Date(data, "estimatedReturnDate")
            });
            await _db.SaveChangesAsync(ct);

            return $"Posted {type.Title} for {player.FirstName} {player.LastName}";
        }
    }

    public class PostLineupAction : IAdminTaskAction
    {
        private const int DefaultProbableTypeId = 1;
        private const int DefaultConfirmedTypeId = 2;

        private readonly RMDBContext _db;

        public PostLineupAction(RMDBContext db)
        {
            _db = db;
        }

        public string ActionType => "PostLineup";

        public async Task<string> RunAsync(AdminTask task, JsonElement data, CancellationToken ct)
        {
            var gameId = ActionJson.Int(data, "gameId") ?? throw new InvalidOperationException("gameId is missing");
            var teamId = ActionJson.Int(data, "teamId") ?? throw new InvalidOperationException("teamId is missing");
            var confirmed = ActionJson.Bool(data, "confirmed");
            var details = ActionJson.Str(data, "details") ?? (confirmed ? "Confirmed" : "Posted");

            if (!ActionJson.TryGet(data, "players", out var playersNode) || playersNode.ValueKind != JsonValueKind.Array)
                throw new InvalidOperationException("players is missing");

            var players = new List<(int PlayerId, int? PositionId)>();
            foreach (var p in playersNode.EnumerateArray())
            {
                int? id = p.ValueKind == JsonValueKind.Number && p.TryGetInt32(out var n) ? n : ActionJson.Int(p, "playerId");
                if (id == null) throw new InvalidOperationException("Each player needs a playerId");
                players.Add((id.Value, p.ValueKind == JsonValueKind.Object ? ActionJson.Int(p, "positionId") : null));
            }
            if (players.Count == 0)
                throw new InvalidOperationException("players is empty");

            var game = await _db.Games.AsNoTracking().FirstOrDefaultAsync(g => g.Id == gameId, ct)
                ?? throw new InvalidOperationException($"Game {gameId} not found");
            if (game.HomeTeamId != teamId && game.AwayTeamId != teamId)
                throw new InvalidOperationException($"Team {teamId} is not in game {gameId}");

            var types = await _db.PlayerGameStateTypes.AsNoTracking().ToListAsync(ct);
            var confirmedTypeId = types.FirstOrDefault(t => t.IsStarter)?.Id ?? DefaultConfirmedTypeId;
            var probableTypeId = types.FirstOrDefault(t => t.IsProbableStarter && !t.IsStarter)?.Id ?? DefaultProbableTypeId;
            var typeId = confirmed ? confirmedTypeId : probableTypeId;
            int? probableFlag = confirmed ? (int?)null : 1;
            var now = DateTime.UtcNow;

            var hasProbableColumn = await _db.Database
                .SqlQueryRaw<int>("SELECT CASE WHEN COL_LENGTH('PlayerGameStates', 'IsProbableStarter') IS NULL THEN 0 ELSE 1 END AS Value")
                .SingleAsync(ct) == 1;

            using var tx = await _db.Database.BeginTransactionAsync(ct);

            foreach (var pos in players.Where(p => p.PositionId.HasValue).Select(p => p.PositionId.Value).Distinct())
            {
                await _db.Database.ExecuteSqlInterpolatedAsync(
                    $"DELETE FROM PlayerGameStates WHERE GameId = {gameId} AND TeamId = {teamId} AND PositionId = {pos} AND PlayerGameStateTypeId IN ({probableTypeId}, {confirmedTypeId})", ct);
            }

            foreach (var p in players)
            {
                await _db.Database.ExecuteSqlInterpolatedAsync(
                    $"DELETE FROM PlayerGameStates WHERE GameId = {gameId} AND PlayerId = {p.PlayerId}", ct);
                if (hasProbableColumn)
                    await _db.Database.ExecuteSqlInterpolatedAsync(
                        $"INSERT INTO PlayerGameStates (GameId, PlayerId, TeamId, PositionId, PlayerGameStateTypeId, DateAdded, Details, IsProbableStarter) VALUES ({gameId}, {p.PlayerId}, {teamId}, {p.PositionId}, {typeId}, {now}, {details}, {probableFlag})", ct);
                else
                    await _db.Database.ExecuteSqlInterpolatedAsync(
                        $"INSERT INTO PlayerGameStates (GameId, PlayerId, TeamId, PositionId, PlayerGameStateTypeId, DateAdded, Details) VALUES ({gameId}, {p.PlayerId}, {teamId}, {p.PositionId}, {typeId}, {now}, {details})", ct);
            }

            await tx.CommitAsync(ct);

            return $"Posted {(confirmed ? "confirmed" : "probable")} lineup, {players.Count} players";
        }
    }
}
