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
        public const int KickerPlayerTypeId = 5;
        public const int DefensePlayerTypeId = 6;
        public const int KickerCategoriesStringId = 1433;
        public const int DefenseCategoriesStringId = 1861;

        private static int _running;
        private static RunSummary _lastRun;

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

        public class RunSummary
        {
            public DateTime StartedAt { get; set; }
            public DateTime FinishedAt { get; set; }
            public string CategoriesCode { get; set; }
            public int SeasonId { get; set; }
            public int LeaguesFound { get; set; }
            public int LeaguesRefreshed { get; set; }
            public int LeaguesSkipped { get; set; }
            public int LeaguesFailed { get; set; }
            public bool OwnershipFilled { get; set; }
            public string FillError { get; set; }
            public double DurationSeconds { get; set; }
            public List<LeagueResult> Failures { get; set; } = new List<LeagueResult>();
        }

        [HttpPost("fill")]
        public IActionResult Fill([FromBody] FillOwnershipRequest request)
        {
            if (!IsAuthorized()) return Unauthorized();

            request = request ?? new FillOwnershipRequest();

            var alreadyRunning = Interlocked.Exchange(ref _running, 1) == 1;
            var started = false;

            if (!alreadyRunning)
            {
                started = true;
                var code = string.IsNullOrEmpty(request.CategoriesCode) ? ProCategoriesCode : request.CategoriesCode;
                var wanted = Math.Max(1, request.LeagueCount);
                var pause = TimeSpan.FromSeconds(Math.Max(0, request.PauseSeconds));

                _ = Task.Run(async () =>
                {
                    try
                    {
                        _lastRun = await RunAsync(code, wanted, pause).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Ownership run failed");
                        _lastRun = new RunSummary
                        {
                            StartedAt = DateTime.UtcNow,
                            FinishedAt = DateTime.UtcNow,
                            CategoriesCode = code,
                            FillError = ex.Message
                        };
                    }
                    finally
                    {
                        Interlocked.Exchange(ref _running, 0);
                    }
                });
            }

            return Ok(Status(started));
        }

        [HttpGet("status")]
        public IActionResult GetStatus()
        {
            if (!IsAuthorized()) return Unauthorized();
            return Ok(Status(false));
        }

        private bool IsAuthorized()
        {
            var apiKey = _config["AdvancedOwnership:ApiKey"];
            if (string.IsNullOrEmpty(apiKey)) apiKey = _config["MonitorApiKey"];
            return !string.IsNullOrEmpty(apiKey) && Request.Headers["X-API-Key"] == apiKey;
        }

        private object Status(bool startedNow)
        {
            var last = _lastRun;

            return new
            {
                running = Volatile.Read(ref _running) == 1,
                startedRun = startedNow,
                lastRun = last == null ? null : new
                {
                    startedAt = last.StartedAt,
                    finishedAt = last.FinishedAt,
                    categoriesCode = last.CategoriesCode,
                    seasonId = last.SeasonId,
                    leaguesFound = last.LeaguesFound,
                    leaguesRefreshed = last.LeaguesRefreshed,
                    leaguesSkipped = last.LeaguesSkipped,
                    leaguesFailed = last.LeaguesFailed,
                    ownershipFilled = last.OwnershipFilled,
                    fillError = last.FillError,
                    durationSeconds = last.DurationSeconds,
                    failures = last.Failures
                }
            };
        }

        private async Task<RunSummary> RunAsync(string code, int wanted, TimeSpan pause)
        {
            var summary = new RunSummary { StartedAt = DateTime.UtcNow, CategoriesCode = code };

            List<UserLeague> candidates;
            Season season;

            using (var scope = _scopeFactory.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<IRMData>();
                season = db.GetDefaultSeason();

                var ids = db.GetUserLeagueIdsWithCategoriesCode(code, season);
                candidates = ids
                    .Select(id => db.GetUserLeague(id))
                    .Where(ul => ul != null && ul.FantasyProviderId == 1 && ul.IsProLeague && !string.IsNullOrEmpty(ul.ProviderLeagueId))
                    .GroupBy(ul => ul.ProviderLeagueId)
                    .Select(g => g.First())
                    .OrderBy(ul => ul.ProviderLeagueId, StringComparer.Ordinal)
                    .ToList();
            }

            summary.SeasonId = season.Id;
            summary.LeaguesFound = candidates.Count;

            var results = new List<LeagueResult>();
            var processed = new List<UserLeague>();

            foreach (var candidate in candidates)
            {
                if (processed.Count >= wanted) break;

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
                    await Task.Delay(pause).ConfigureAwait(false);
            }

            if (processed.Count > 0)
            {
                try
                {
                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var db = scope.ServiceProvider.GetRequiredService<IRMData>();
                        if (code == ProCategoriesCode)
                        {
                            db.FillOwnershipPlayers(code, processed, season.Id, new Dictionary<int, int>
                            {
                                { KickerPlayerTypeId, KickerCategoriesStringId },
                                { DefensePlayerTypeId, DefenseCategoriesStringId }
                            });
                        }
                        else
                        {
                            db.FillOwnershipPlayers(code, processed);
                        }
                        summary.OwnershipFilled = true;
                    }
                }
                catch (Exception ex)
                {
                    summary.FillError = ex.Message;
                    _logger.LogError(ex, "FillOwnershipPlayers failed");
                }
            }

            summary.LeaguesRefreshed = processed.Count;
            summary.LeaguesSkipped = results.Count(r => r.Error == "no rostered players");
            summary.LeaguesFailed = results.Count(r => r.Error != null && r.Error != "no rostered players");
            summary.Failures = results.Where(r => r.Error != null).Take(25).ToList();
            summary.FinishedAt = DateTime.UtcNow;
            summary.DurationSeconds = Math.Round((summary.FinishedAt - summary.StartedAt).TotalSeconds, 1);

            return summary;
        }
    }
}
