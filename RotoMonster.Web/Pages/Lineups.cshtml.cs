using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RotoMonster.Core;
using RotoMonster.Data;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace RotoMonster.Pages
{
    public class LineupRow
    {
        public string Slot { get; set; } = "";
        public Player Player { get; set; }
        public Position Position { get; set; }
        public string Hand { get; set; } = "";
        public PlayerStatus Status { get; set; }
        public bool IsOwned { get; set; }
        public bool IsActive { get; set; }
        public bool IsFeatured { get; set; }
        public string Group { get; set; } = "";
        public OwnershipPlayer Ownership { get; set; }
    }

    public class LineupRowView
    {
        public LineupRow Row { get; set; }
        public bool IsMLB { get; set; }
        public bool IsBench { get; set; }
        public int LeagueId { get; set; }
        public string Label { get; set; } = "";
    }

    public class LineupTeam
    {
        public Team Team { get; set; }
        public string Color { get; set; } = "";
        public string Implied { get; set; } = "";
        public string Line { get; set; } = "";
        public string Context { get; set; } = "";
        public string SlotHead { get; set; } = "";
        public string RightHead { get; set; } = "";
        public List<LineupRow> Rows { get; set; } = new List<LineupRow>();
        public List<LineupRow> Bench { get; set; } = new List<LineupRow>();
        public string BenchTitle { get; set; } = "Bench";
        public string Footer { get; set; } = "";
        public string FooterKind { get; set; } = "none";
    }

    public class LineupGame
    {
        public Game Game { get; set; }
        public string StatusText { get; set; } = "";
        public string State { get; set; } = "upcoming";
        public bool HasOwned { get; set; }
        public List<LineupTeam> Teams { get; set; } = new List<LineupTeam>();
    }

    public class LineupsModel : RMPageModel
    {
        private const int ProbableTypeId = 1;
        private const int ConfirmedTypeId = 2;
        private const int BenchTypeId = 3;

        [BindProperty] public DateTime SelectedDate { get; set; }
        public Sport Sport { get; set; }
        public string SportKey { get; set; } = "";
        public List<LineupGame> Games { get; set; } = new List<LineupGame>();

        public LineupsModel(IConfiguration config, IRMData db, IRMSharedData sharedDb, UserManager<ApplicationUser> userManager, IHttpContextAccessor contextAccessor, ILogger<PageModel> logger)
            : base(config, db, sharedDb, userManager, contextAccessor, logger)
        {
        }

        public IActionResult OnPost(string backbutton, string nextbutton)
        {
            if (!string.IsNullOrEmpty(backbutton))
                SelectedDate = SelectedDate.AddDays(-1);
            else if (!string.IsNullOrEmpty(nextbutton))
                SelectedDate = SelectedDate.AddDays(1);

            return RedirectToPage("./Lineups", new { l = SelectedUserLeagueId, date = SelectedDate.ToString("yyyy-MM-dd") });
        }

        public async Task OnGetAsync(DateTime date, int? l)
        {
            InitGet("Lineups");

            if (UserId != null)
            {
                if (l != null)
                    SelectedUserLeagueId = l.GetValueOrDefault();
                else if (SelectedUserLeagues != null && SelectedUserLeagues.Count > 0)
                    SelectedUserLeagueId = SelectedUserLeagues[0].Id;
                ViewData["UserLeagueList"] = new SelectList(await db.GetTrackedUserLeaguesAsync(UserId), "Id", "ListDisplayTitle");
            }

            Sport = db.Sport;
            SportKey = (Sport.Title ?? "").ToLowerInvariant();
            var season = db.GetDefaultSeason();
            SelectedDate = date.Ticks == 0 ? TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, Game.DisplayZone).Date : date.Date;

            var games = db.GetGames(season, SelectedDate, SelectedDate);
            var states = await db.GetPlayerGameStatesAsync(SelectedDate, SelectedDate);
            var seasonPlayers = db.GetAllSeasonPlayers(season);
            var statuses = await db.GetActivePlayerStatusesAsync();
            var positions = db.GetPositions().ToDictionary(p => p.Id);

            var owned = new Dictionary<int, bool>();
            foreach (var ul in SelectedUserLeagues ?? new List<UserLeague>())
            {
                var mine = ul.MyUserLeagueTeam;
                if (mine == null || mine.UserLeagueTeamPlayers == null) continue;
                foreach (var tp in mine.UserLeagueTeamPlayers)
                    owned[tp.PlayerId] = (owned.TryGetValue(tp.PlayerId, out var a) && a) || tp.IsActive;
            }

            var statusByPlayer = statuses.GroupBy(s => s.PlayerId).ToDictionary(gr => gr.Key, gr => gr.OrderByDescending(s => s.DateAdded).First());

            var ownershipByPlayer = new Dictionary<int, OwnershipPlayer>();
            try
            {
                var ownLeague = (SelectedUserLeagues ?? new List<UserLeague>()).FirstOrDefault(u => u.Id == SelectedUserLeagueId);
                foreach (var playerType in db.GetPlayerTypes())
                {
                    var code = ownLeague != null
                        ? await db.GetUserLeagueCategoryCodeAsync(ownLeague, playerType)
                        : db.GetDefaultCategoriesString(playerType).Code;
                    if (string.IsNullOrEmpty(code)) continue;
                    foreach (var op in db.GetOwnershipPlayersWithChange(code, DateTime.UtcNow, 24))
                        ownershipByPlayer.TryAdd(op.PlayerId, op);
                }
            }
            catch (Exception)
            {
                ownershipByPlayer.Clear();
            }

            foreach (var game in games)
            {
                var lg = new LineupGame { Game = game };
                lg.State = game.IsFinished ? "final" : (game.HasStarted ? "live" : "upcoming");
                lg.StatusText = game.IsFinished ? "Final"
                    : game.HasStarted ? (game.PercentComplete > 0 ? "Live · " + game.PercentComplete + "%" : "Live")
                    : game.EasternTime.ToString("h:mm tt", CultureInfo.InvariantCulture);

                var gameStates = states.Where(s => s.GameId == game.Id).ToList();
                var sps = gameStates.Where(s => s.PlayerGameStateTypeId != BenchTypeId && IsCode(Pos(s, positions), "SP")).ToList();

                foreach (var team in game.GetTeams())
                {
                    var lt = new LineupTeam { Team = team };
                    lt.Color = "var(--team-" + SportKey + "-" + (team.Code ?? "").ToLowerInvariant() + ", var(--lu-sport))";
                    FillOdds(lt, game, team);
                    if (ownershipByPlayer.Count > 0) lt.RightHead = "Own \u00b7 Act";

                    var teamStates = gameStates.Where(s => s.TeamId == team.Id).ToList();
                    var starters = teamStates.Where(s => s.PlayerGameStateTypeId == ProbableTypeId || s.PlayerGameStateTypeId == ConfirmedTypeId).ToList();
                    var benchStates = teamStates.Where(s => s.PlayerGameStateTypeId == BenchTypeId).ToList();

                    Func<int, Position, string, bool, LineupRow> row = (playerId, pos, slot, featured) =>
                    {
                        var sp = seasonPlayers.FirstOrDefault(x => x.PlayerId == playerId);
                        var player = sp != null ? sp.Player : null;
                        if (player == null)
                        {
                            var st = teamStates.FirstOrDefault(x => x.PlayerId == playerId);
                            player = st != null ? st.Player : null;
                        }
                        var r = new LineupRow
                        {
                            Slot = slot,
                            Player = player,
                            Position = pos ?? (player != null ? player.DefaultPosition : null),
                            Status = statusByPlayer.TryGetValue(playerId, out var ps) ? ps : null,
                            IsOwned = owned.ContainsKey(playerId),
                            IsActive = owned.TryGetValue(playerId, out var act) && act,
                            IsFeatured = featured,
                            Ownership = ownershipByPlayer.TryGetValue(playerId, out var own) ? own : null
                        };
                        if (Sport.IsMLB && player != null)
                            r.Hand = (r.Position != null && IsCode(r.Position, "SP")) ? (player.Throws ?? "") : (player.Bats ?? "");
                        return r;
                    };

                    if (Sport.IsMLB)
                    {
                        lt.SlotHead = "#";
                        lt.RightHead = ownershipByPlayer.Count > 0 ? "Own \u00b7 Act \u00b7 Pos" : "POS";
                        var oppSp = sps.FirstOrDefault(s => s.TeamId != team.Id);
                        if (oppSp != null && oppSp.Player != null)
                            lt.Context = "vs " + ((oppSp.Player.Throws ?? "").StartsWith("L", StringComparison.OrdinalIgnoreCase) ? "LHP " : "RHP ") + oppSp.Player.ForwardName;
                        else
                            lt.Context = "Lineup";

                        foreach (var s in starters.Where(s => !IsCode(Pos(s, positions), "SP"))
                                                  .OrderBy(s => int.TryParse(s.Details, out var n) ? n : 99))
                            lt.Rows.Add(row(s.PlayerId, Pos(s, positions), (s.Details ?? "").Trim(), false));
                        foreach (var s in starters.Where(s => IsCode(Pos(s, positions), "SP")))
                            lt.Rows.Add(row(s.PlayerId, Pos(s, positions), "SP", true));

                        foreach (var s in benchStates)
                            lt.Bench.Add(row(s.PlayerId, Pos(s, positions), "", false));
                        lt.Bench = lt.Bench.OrderBy(r => r.Position != null ? r.Position.DisplayOrder : 99).ThenBy(r => r.Player != null ? r.Player.LastName : "").ToList();

                        var batters = starters.Where(s => !IsCode(Pos(s, positions), "SP")).ToList();
                        SetLineupFooter(lt, batters);
                    }
                    else if (Sport.IsNHL)
                    {
                        lt.SlotHead = "POS";
                        lt.Context = "Lines";
                        var goalies = starters.Where(s => IsCode(Pos(s, positions), "G")).ToList();
                        var skaters = starters.Where(s => !IsCode(Pos(s, positions), "G"))
                                              .OrderBy(s => NhlSlotOrder(s.Details)).ToList();

                        foreach (var s in skaters)
                        {
                            var r = row(s.PlayerId, Pos(s, positions), "", false);
                            r.Group = NhlGroup(s.Details);
                            lt.Rows.Add(r);
                        }
                        foreach (var s in goalies)
                            lt.Rows.Add(row(s.PlayerId, Pos(s, positions), "G", true));

                        var inLineup = new HashSet<int>(starters.Select(s => s.PlayerId));
                        lt.Bench = Roster(seasonPlayers, team, inLineup, row);
                        lt.BenchTitle = skaters.Count > 0 ? "Not in lineup" : "Roster";

                        if (skaters.Count > 0 && skaters.All(s => s.PlayerGameStateTypeId == ConfirmedTypeId)) { lt.Footer = "Confirmed lines"; lt.FooterKind = "ok"; }
                        else if (skaters.Count > 0) { lt.Footer = "Projected lines"; lt.FooterKind = "proj"; }
                        else { lt.Footer = "Lines not posted"; lt.FooterKind = "none"; }
                    }
                    else
                    {
                        lt.SlotHead = "POS";
                        lt.Context = "Starters";
                        foreach (var s in starters.OrderBy(s => Pos(s, positions) != null ? Pos(s, positions).DisplayOrder : 99))
                        {
                            var pos = Pos(s, positions);
                            lt.Rows.Add(row(s.PlayerId, pos, pos != null ? pos.Abbreviation.Trim() : "", false));
                        }

                        var inLineup = new HashSet<int>(starters.Select(s => s.PlayerId));
                        lt.Bench = Roster(seasonPlayers, team, inLineup, row);
                        SetLineupFooter(lt, starters);
                    }

                    lg.HasOwned |= lt.Rows.Concat(lt.Bench).Any(r => r.IsOwned);
                    lg.Teams.Add(lt);
                }

                Games.Add(lg);
            }
        }

        private static List<LineupRow> Roster(List<SeasonPlayer> seasonPlayers, Team team, HashSet<int> exclude, Func<int, Position, string, bool, LineupRow> row)
        {
            return seasonPlayers
                .Where(sp => sp.TeamId == team.Id && !exclude.Contains(sp.PlayerId) && sp.Player != null)
                .Select(sp =>
                {
                    var pos = sp.Player.DefaultPosition;
                    return row(sp.PlayerId, pos, pos != null ? pos.Abbreviation.Trim() : "", false);
                })
                .OrderBy(r => r.Position != null ? r.Position.DisplayOrder : 99)
                .ThenBy(r => r.Player.LastName)
                .ToList();
        }

        private static void SetLineupFooter(LineupTeam lt, List<PlayerGameState> rows)
        {
            if (rows.Count == 0) { lt.Footer = "Lineup not posted"; lt.FooterKind = "none"; }
            else if (rows.All(s => s.PlayerGameStateTypeId == ConfirmedTypeId)) { lt.Footer = "Confirmed lineup"; lt.FooterKind = "ok"; }
            else { lt.Footer = "Projected lineup"; lt.FooterKind = "proj"; }
        }

        private static int NhlSlotOrder(string details)
        {
            var d = (details ?? "").Trim().ToUpperInvariant();
            if (d.Length < 4 || d[2] != '-' || !char.IsDigit(d[1])) return 999;
            var group = d[0] == 'L' ? 0 : d[0] == 'D' ? 1 : 2;
            var side = d.Substring(3);
            var order = side == "LW" || side == "L" ? 0 : side == "C" ? 1 : 2;
            return group * 100 + (d[1] - '0') * 10 + order;
        }

        private static string NhlGroup(string details)
        {
            var d = (details ?? "").Trim().ToUpperInvariant();
            if (d.Length < 2 || !char.IsDigit(d[1])) return "";
            if (d[0] == 'L') return "Line " + d[1];
            if (d[0] == 'D') return "D pair " + d[1];
            return "";
        }

        private void FillOdds(LineupTeam lt, Game game, Team team)
        {
            if ((Sport.IsNHL || Sport.IsMLB) && game.OverUnder > 0 && game.HomeProjectedPoints == 0 && game.AwayProjectedPoints == 0)
                game.MoneyLineToWinsAndPoints();

            var isHome = game.HomeTeam != null && team.Id == game.HomeTeam.Id;
            double? implied = null;
            var projected = isHome ? game.HomeProjectedPoints : game.AwayProjectedPoints;
            if (projected > 0)
                implied = projected;
            else if (game.OverUnder > 0)
                implied = game.OverUnder / 2 + (isHome ? -game.HomeSpread : game.HomeSpread) / 2;

            var unit = Sport.IsMLB ? "runs" : Sport.IsNHL ? "goals" : "pts";
            lt.Implied = implied.HasValue ? implied.Value.ToString("0.0", CultureInfo.InvariantCulture) + " " + unit : "";

            var parts = new List<string>();
            var spread = isHome ? game.HomeSpread : -game.HomeSpread;
            if (spread != 0)
                parts.Add((spread > 0 ? "+" : "") + spread.ToString("0.0", CultureInfo.InvariantCulture));
            if (game.OverUnder > 0)
                parts.Add("o/u " + game.OverUnder.ToString("0.0", CultureInfo.InvariantCulture));
            var ml = isHome ? game.HomeMoneyLine : game.AwayMoneyLine;
            if (ml.HasValue && ml.Value != 0)
                parts.Add("ML " + (ml.Value > 0 ? "+" : "") + ml.Value);
            lt.Line = string.Join(" · ", parts);
        }

        private static Position Pos(PlayerGameState s, Dictionary<int, Position> positions)
        {
            return s.PositionId.HasValue && positions.TryGetValue(s.PositionId.Value, out var p) ? p : null;
        }

        private static bool IsCode(Position p, string code)
        {
            return p != null && string.Equals((p.Abbreviation ?? "").Trim(), code, StringComparison.OrdinalIgnoreCase);
        }
    }
}
