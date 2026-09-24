using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RotoMonster.Core;
using RotoMonster.Data;

namespace RotoMonster.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class OwnershipController : ControllerBase
    {
        public const string ProCategoriesCode = "1p6:2p6:3p0.1:4p0.1:5p4:6p0.04:7p0.5:12p-2:29p-1:31p6:32p2:33p6";

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IConfiguration _config;
        private readonly ILogger<OwnershipController> _logger;

        public OwnershipController(IServiceScopeFactory scopeFactory, IConfiguration config, ILogger<OwnershipController> logger)
        {
            _scopeFactory = scopeFactory;
            _config = config;
            _logger = logger;
        }

        public class FillOwnershipRequest
        {
            public string CategoriesCode { get; set; }
            public int LeagueCount { get; set; } = 100;
            public int PauseSeconds { get; set; } = 3;
        }

        public class LeagueResult
        {
            public int UserLeagueId { get; set; }
            public string ProviderLeagueId { get; set; }
            public int Teams { get; set; }
            public int Players { get; set; }
            public string Error { get; set; }
        }

        [HttpPost("fill")]
        public async Task<IActionResult> Fill([FromBody] FillOwnershipRequest request, CancellationToken ct)
        {
            var apiKey = _config["MonitorApiKey"];
            if (string.IsNullOrEmpty(apiKey) || Request.Headers["X-API-Key"] != apiKey)
                return Unauthorized();

            request = request ?? new FillOwnershipRequest();
            var code = string.IsNullOrEmpty(request.CategoriesCode) ? ProCategoriesCode : request.CategoriesCode;
            var wanted = Math.Max(1, request.LeagueCount);
            var pause = TimeSpan.FromSeconds(Math.Max(0, request.PauseSeconds));
            var started = DateTime.UtcNow;

            List<UserLeague> candidates;
            Season season;

            using (var scope = _scopeFactory.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<IRMData>();
                season = db.GetDefaultSeason();

                var ids = db.GetUserLeagueIdsWithCategoriesCode(code, season);
                candidates = ids
                    .Select(id => db.GetUserLeague(id))
                    .Where(ul => ul != null && ul.FantasyProviderId == 1 && !string.IsNullOrEmpty(ul.ProviderLeagueId))
                    .GroupBy(ul => ul.ProviderLeagueId)
                    .Select(g => g.First())
                    .OrderBy(ul => ul.ProviderLeagueId, StringComparer.Ordinal)
                    .ToList();
            }

            var results = new List<LeagueResult>();
            var processed = new List<UserLeague>();

            foreach (var candidate in candidates)
            {
                if (processed.Count >= wanted || ct.IsCancellationRequested)
                    break;

                var result = new LeagueResult
                {
                    UserLeagueId = candidate.Id,
                    ProviderLeagueId = candidate.ProviderLeagueId
                };

                try
                {
                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var db = scope.ServiceProvider.GetRequiredService<IRMData>();
                        var sharedDb = scope.ServiceProvider.GetRequiredService<IRMSharedData>();

                        var userLeague = db.GetUserLeague(candidate.Id);
                        var userAuth = userLeague == null ? null : sharedDb.GetUserAuth(userLeague.UserId);

                        if (userAuth == null)
                        {
                            result.Error = "no authorization";
                        }
                        else
                        {
                            var missingPlayers = new List<UserLeagueMissingPlayer>();
                            var providerPlayers = db.GetFantasyProviderPlayers(db.GetFantasyProvider("yahoo"));
                            var teams = sharedDb.GetUserLeagueTeams(userAuth, season.YahooId, userLeague, providerPlayers, missingPlayers, _logger);

                            result.Teams = teams == null ? 0 : teams.Count;

                            result.Players = teams == null ? 0 : teams.Sum(t => t.UserLeagueTeamPlayers == null ? 0 : t.UserLeagueTeamPlayers.Count);

                            if (result.Teams == 0)
                            {
                                result.Error = "no teams returned";
                            }
                            else
                            {
                                db.UpdateUserLeagueTeams(userLeague.Id, teams, missingPlayers, null);

                                if (result.Players == 0)
                                    result.Error = "no rostered players";
                                else
                                    processed.Add(db.GetUserLeague(userLeague.Id));
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    result.Error = ex.Message;
                }

                results.Add(result);

                if (pause > TimeSpan.Zero)
                    await Task.Delay(pause, ct);
            }

            var filled = false;
            string fillError = null;

            if (processed.Count > 0)
            {
                try
                {
                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var db = scope.ServiceProvider.GetRequiredService<IRMData>();
                        db.FillOwnershipPlayers(code, processed);
                        filled = true;
                    }
                }
                catch (Exception ex)
                {
                    fillError = ex.Message;
                    _logger.LogError(ex, "FillOwnershipPlayers failed");
                }
            }

            return Ok(new
            {
                categoriesCode = code,
                seasonId = season.Id,
                leaguesFound = candidates.Count,
                leaguesRefreshed = processed.Count,
                leaguesSkipped = results.Count(r => r.Error == "no rostered players"),
                leaguesFailed = results.Count(r => r.Error != null && r.Error != "no rostered players"),
                ownershipFilled = filled,
                fillError,
                durationSeconds = Math.Round((DateTime.UtcNow - started).TotalSeconds, 1),
                failures = results.Where(r => r.Error != null).Take(25)
            });
        }
    }
}
