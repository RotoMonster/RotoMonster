using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RotoMonster.Core;
using RotoMonster.Data;
using RotoMonsterUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RotoMonster.Pages
{
    public class ScheduleGridModel : RMPageModel
    {
        public const string GridId = "rm-schedule-grid";

        public string GridHtml { get; set; } = "";
        public bool UsesQualityGames { get; set; }
        public int QualityLimit { get; set; }
        public string SportTitle { get; set; } = "";
        public string ExpandedCsv { get; set; } = "";

        public ScheduleGridModel(IConfiguration config, IRMData db, IRMSharedData sharedDb, UserManager<ApplicationUser> userManager, IHttpContextAccessor contextAccessor, ILogger<PageModel> logger)
            : base(config, db, sharedDb, userManager, contextAccessor, logger)
        {
        }

        public async Task OnGetAsync(int? l)
        {
            await BuildAsync(l, null, new List<int>());
        }

        public async Task OnPostAsync(int? l)
        {
            var form = Request.Form.ToDictionary(k => k.Key, v => v.Value.ToString());
            var result = new ScheduleGridService().Process(GridId, form);

            var expanded = new List<int>();
            if (form.TryGetValue(GridId + "-expanded", out var csv))
            {
                foreach (var part in csv.Split(',', StringSplitOptions.RemoveEmptyEntries))
                    if (int.TryParse(part, out var n)) expanded.Add(n);
            }
            if (result.ToggledPeriodNumber.HasValue)
            {
                var p = result.ToggledPeriodNumber.Value;
                if (result.IsCollapseRequested) expanded.Remove(p);
                else if (!expanded.Contains(p)) expanded.Add(p);
            }
            else if (result.IsCollapseRequested)
            {
                expanded.Clear();
            }

            await BuildAsync(l, result, expanded);
        }

        private async Task BuildAsync(int? l, ScheduleGridResult state, List<int> expanded)
        {
            InitGet("Schedule Grid");

            if (UserId != null)
            {
                if (l != null)
                    SelectedUserLeagueId = l.GetValueOrDefault();
                else if (SelectedUserLeagueId == 0 && SelectedUserLeagues != null && SelectedUserLeagues.Count > 0)
                    SelectedUserLeagueId = SelectedUserLeagues[0].Id;
                ViewData["UserLeagueList"] = new SelectList(await db.GetTrackedUserLeaguesAsync(UserId), "Id", "ListDisplayTitle");
            }

            UserLeague userLeague = await db.SelectUserLeagueAsync(UserId, await db.GetUserLeagueAsync(UserId, SelectedUserLeagueId));
            if (userLeague == null)
                userLeague = await db.GetDefaultUserLeagueAsync();

            var sport = db.Sport;
            SportTitle = sport.Title;
            var season = db.GetDefaultSeason();

            var first = season.StartDate.Date;
            var weekStart = sport.IsNFL ? DayOfWeek.Tuesday : sport.StartDayOfWeek;
            while (first.DayOfWeek != weekStart)
                first = first.AddDays(-1);

            var periods = new List<ScheduleGridPeriod>();
            for (var d = first; d <= season.EndDate.Date; d = d.AddDays(7))
                periods.Add(new ScheduleGridPeriod { PeriodNumber = periods.Count + 1, StartDate = d, NumWeeks = 1 });

            var games = db.GetGames(season, first, season.EndDate.Date)
                .Where(g => g.AwayTeam != null && g.HomeTeam != null && g.IsPostponed != true)
                .ToList();

            var teams = games.SelectMany(g => new[] { g.AwayTeam, g.HomeTeam })
                .GroupBy(t => t.Id)
                .Select(g => g.First())
                .OrderBy(t => t.Code)
                .ToList();

            UsesQualityGames = !sport.IsNFL;
            QualityLimit = QualityGameLimit(userLeague, teams.Count);
            var gamesPerDay = games.GroupBy(g => g.GameDate.Date).ToDictionary(g => g.Key, g => g.Count());

            var opponentEase = OpponentEase(userLeague, season);
            var easeMean = opponentEase.Values.DefaultIfEmpty(0).Average();
            var maxAbs = opponentEase.Values.Select(v => Math.Abs(v - easeMean)).DefaultIfEmpty(0).Max();
            double Scale(double v) => maxAbs > 0 ? (v - easeMean) / maxAbs : 0;

            int PeriodFor(DateTime date) => (int)((date.Date - first).TotalDays / 7) + 1;

            var gridTeams = new List<ScheduleGridTeam>();
            foreach (var team in teams)
            {
                var gridTeam = new ScheduleGridTeam
                {
                    TeamCode = team.Code,
                    TeamColor = "var(--team-" + sport.Title.Trim().ToLower() + "-" + (team.Code ?? "").Trim().ToLower() + ", var(--brand-primary))"
                };

                foreach (var game in games.Where(g => g.AwayTeamId == team.Id || g.HomeTeamId == team.Id).OrderBy(g => g.GameDate))
                {
                    var period = PeriodFor(game.GameDate);
                    if (!gridTeam.Periods.TryGetValue(period, out var cell))
                    {
                        cell = new ScheduleGridPeriodCell();
                        gridTeam.Periods[period] = cell;
                    }

                    var opponent = game.GetOpponent(team);
                    var ease = opponentEase.TryGetValue(opponent.Id, out var e) ? Scale(e) : 0;
                    var quality = UsesQualityGames && IsQualityGame(gamesPerDay, game.GameDate.Date, QualityLimit);

                    cell.Games++;
                    if (quality) cell.QualityGames++;
                    cell.Days.Add(new ScheduleGridDay
                    {
                        Date = game.GameDate.Date,
                        Opponent = opponent.Code,
                        IsAwayGame = game.AwayTeamId == team.Id,
                        IsQualityGame = quality,
                        EaseColor = EaseColor(ease)
                    });
                    cell.Ease += ease;
                }

                foreach (var cell in gridTeam.Periods.Values)
                {
                    if (cell.Games > 0) cell.Ease /= cell.Games;
                    cell.EaseColor = EaseColor(cell.Ease);
                }

                gridTeams.Add(gridTeam);
            }

            foreach (var period in periods)
            {
                var max = gridTeams.Select(t => t.Periods.TryGetValue(period.PeriodNumber, out var c) ? c.Games : 0).DefaultIfEmpty(0).Max();
                foreach (var t in gridTeams)
                    if (t.Periods.TryGetValue(period.PeriodNumber, out var c) && max > 0 && c.Games == max)
                        c.IsMaxWeek = true;
            }

            var today = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, Game.DisplayZone).Date;
            var currentPeriod = Math.Clamp(PeriodFor(today), 1, Math.Max(1, periods.Count));
            var start = Math.Clamp(state?.StartSelectedPeriod ?? currentPeriod, 1, Math.Max(1, periods.Count));
            var end = Math.Clamp(state?.EndSelectedPeriod ?? periods.Count, start, Math.Max(1, periods.Count));

            foreach (var t in gridTeams)
            {
                var inRange = t.Periods.Where(kv => kv.Key >= start && kv.Key <= end).Select(kv => kv.Value).ToList();
                var played = inRange.Where(c => c.Games > 0).ToList();
                t.Summary = new ScheduleGridTeamSummary
                {
                    Games = inRange.Sum(c => c.Games),
                    MaxWeeks = inRange.Count(c => c.IsMaxWeek),
                    QualityGames = inRange.Sum(c => c.QualityGames),
                    AvgEase = played.Count > 0 ? played.Average(c => c.Ease) : 0
                };
            }

            var maxAvgEase = gridTeams.Select(t => Math.Abs(t.Summary.AvgEase)).DefaultIfEmpty(0).Max();
            foreach (var t in gridTeams)
            {
                var scaled = maxAvgEase > 0 ? t.Summary.AvgEase / maxAvgEase : 0;
                var alpha = (int)Math.Round(Math.Abs(scaled) * 0x59);
                t.Summary.AvgEaseColor = Math.Abs(scaled) < 0.1 ? null : (scaled > 0 ? "22c55e" : "ef4444") + alpha.ToString("x2");
            }

            ExpandedCsv = string.Join(",", expanded);

            var input = new ScheduleGridInput
            {
                Id = GridId,
                Periods = periods,
                Teams = gridTeams,
                SelectedDate = today,
                StartSelectedPeriod = start,
                EndSelectedPeriod = end,
                ColorType = state?.ColorType ?? ScheduleGridColorType.Ease,
                SortBy = state?.SortBy ?? ScheduleGridSortBy.Team,
                Sport = sport.IsNBA ? GameSport.Basketball : GameSport.Baseball,
                UseQualityGames = UsesQualityGames,
                ShowQualityGames = state?.ShowQualityGames ?? true,
                ExpandedPeriodNumbers = expanded
            };

            GridHtml = new RotoMonsterUI.ScheduleGrid(input).Render();
        }

        private static int QualityGameLimit(UserLeague userLeague, int teamCount)
        {
            if (userLeague != null && userLeague.QualityGamesLimit > 0)
                return userLeague.QualityGamesLimit;
            return 5;
        }

        private static bool IsQualityGame(Dictionary<DateTime, int> gamesPerDay, DateTime date, int limit)
        {
            return gamesPerDay.TryGetValue(date, out var count) && count <= limit;
        }

        private Dictionary<int, double> OpponentEase(UserLeague userLeague, Season season)
        {
            var sums = new Dictionary<int, (double Total, int Count)>();
            try
            {
                var end = DateTime.UtcNow.Date < season.EndDate.Date ? DateTime.UtcNow.Date : season.EndDate.Date;
                foreach (var playerType in db.GetPlayerTypes())
                {
                    var values = db.GetTeamEaseValuePlayers(playerType, season, season.StartDate.Date, end,
                        userLeague.GetCategorySettings(playerType), userLeague.ScoringSystem);
                    foreach (var vp in values)
                    {
                        if (vp?.StatPlayer?.Player == null) continue;
                        var id = vp.Player.Id;
                        var cur = sums.TryGetValue(id, out var s) ? s : (0d, 0);
                        sums[id] = (cur.Item1 + vp.LeagueValue, cur.Item2 + 1);
                    }
                }
            }
            catch (Exception)
            {
                sums.Clear();
            }
            return sums.ToDictionary(kv => kv.Key, kv => kv.Value.Count > 0 ? kv.Value.Total / kv.Value.Count : 0);
        }

        private static string EaseColor(double ease)
        {
            var e = (float)Math.Max(-1, Math.Min(1, ease));
            return e >= 0
                ? ColorHelper.GetGreenColorCode(e, 0, 1, true)
                : ColorHelper.GetRedColorCode(-e, 0, 1, true);
        }
    }
}
