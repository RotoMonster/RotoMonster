using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RotoMonster.Core;
using RotoMonster.Models.Shared;
using RotoMonster.Data;

namespace RotoMonster.Pages
{
    public class PlayersModel : RMPageModel
    {
        public Player Player { get; set; }
        public SeasonPlayer SeasonPlayer { get; set; }
        public List<PositionSourcePlayer> PositionSourcePlayers { get; set; }
        public OwnershipPlayer OwnershipPlayer { get; set; }
        public OwnershipPlayerChange OwnershipPlayerChange { get; set; } = null;
        public PlayerTableModel HistoryTableModel { get; set; } = new PlayerTableModel();
        public UserLeague SelectedUserLeague { get; set; }
        public List<GameLogGame> GameLogGames { get; set; } = null;
        public PerValue GameLogPerValue { get; set; }
        public PlayerStatus PlayerStatus { get; set; }
        public List<UserLeague> OwnedInUserLeagues = null;
        public List<UserLeague> AvailableInUserLeagues = null;
        public List<RotoMonster.Services.FeedTweet> PlayerTweets { get; set; } = new List<RotoMonster.Services.FeedTweet>();
        public List<UpcomingGame> UpcomingGames { get; set; } = new List<UpcomingGame>();
        public List<RecentGame> RecentGames { get; set; } = new List<RecentGame>();
        public PlayerNewsColumnModel StatusNews { get; set; } = new PlayerNewsColumnModel { Title = null };

        public class RecentGame
        {
            public string When { get; set; }
            public string Opponent { get; set; }
            public bool Missed { get; set; }
            public string Points { get; set; }
            public string Color { get; set; }
            public string Line { get; set; }
        }

        public class UpcomingGame
        {
            public string When { get; set; }
            public string Opponent { get; set; }
            public string EaseColor { get; set; }
            public bool IsBye { get; set; }
        }

        [BindProperty(SupportsGet = true)]
        public string SearchTerm { get; set; }

        public PlayersModel(IConfiguration config, IRMData db, IRMSharedData sharedDb, UserManager<ApplicationUser> userManager, IHttpContextAccessor contextAccessor, ILogger<PageModel> logger)
            : base(config, db, sharedDb, userManager, contextAccessor, logger)
        {
        }

        private async Task FillPlayerExtrasAsync(Season season)
        {
            try
            {
                var feed = HttpContext.RequestServices.GetService(typeof(RotoMonster.Services.TwitterFeedService)) as RotoMonster.Services.TwitterFeedService;
                if (feed != null && feed.Enabled && Player != null)
                {
                    var tweets = await feed.GetTweetsAsync();
                    PlayerTweets = tweets
                        .Where(t => t.Players != null && t.Players.Any(p => p.PlayerId == Player.Id))
                        .OrderByDescending(t => t.CreatedDate)
                        .Take(5)
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                logger?.LogWarning(ex, "Player tweets failed");
            }

            try
            {
                var team = SeasonPlayer?.Team;
                if (team == null || season == null) return;
                var sport = db.Sport;
                var today = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, Game.DisplayZone).Date;
                var ease = OpponentEaseScaled(season);
                var games = db.GetGames(season, today, today.AddDays(sport.IsNFL ? 42 : 14))
                    .Where(g => g.IsPostponed != true && g.AwayTeam != null && g.HomeTeam != null && (g.AwayTeamId == team.Id || g.HomeTeamId == team.Id))
                    .OrderBy(g => g.GameDate)
                    .ToList();

                if (sport.IsNFL)
                {
                    var start = today;
                    while (start.DayOfWeek != DayOfWeek.Tuesday)
                        start = start.AddDays(-1);
                    for (int w = 0; w < 5; w++)
                    {
                        var ws = start.AddDays(7 * w);
                        if (ws > season.EndDate.Date) break;
                        var we = ws.AddDays(7);
                        var g = games.FirstOrDefault(x => x.GameDate.Date >= ws && x.GameDate.Date < we);
                        UpcomingGames.Add(g == null
                            ? new UpcomingGame { When = ws.AddDays(5).ToString("MMM d"), Opponent = "BYE", IsBye = true }
                            : MakeUpcoming(g, team, ease));
                    }
                }
                else
                {
                    UpcomingGames = games.Take(6).Select(g => MakeUpcoming(g, team, ease)).ToList();
                }
            }
            catch (Exception ex)
            {
                logger?.LogWarning(ex, "Player schedule failed");
            }
        }

        private void FillRecentGames()
        {
            try
            {
                if (GameLogGames == null) return;
                var cats = (HistoryTableModel.UserDisplayCategories ?? new List<UserDisplayCategory>())
                    .Select(u => u.Category)
                    .Where(c => c != null && c.Id != HistoryTableModel.GamesCategoryId && !string.IsNullOrWhiteSpace(c.Abbreviation))
                    .ToList();
                foreach (var g in GameLogGames.Where(x => !x.IsBreak && x.Game != null).OrderByDescending(x => x.Game.GameDate).Take(5))
                {
                    var team = g.StatPlayer != null && g.StatPlayer.Team != null ? g.StatPlayer.Team : SeasonPlayer.Team;
                    var rg = new RecentGame
                    {
                        When = g.Game.GameDate.ToString("ddd M/d"),
                        Opponent = team != null ? g.Game.GetOpponentText(team) : "",
                        Missed = g.PlayerGameMissed != null || g.StatPlayer == null
                    };
                    if (!rg.Missed)
                    {
                        if (g.ValuePlayer != null)
                        {
                            rg.Points = g.ValuePlayer.LeagueValue.ToString("0.00");
                            rg.Color = g.ValuePlayer.LeagueValueColor;
                        }
                        var parts = new List<string>();
                        foreach (var c in cats)
                        {
                            var v = g.StatPlayer.Get(HistoryTableModel.DisplayPerValue, c.Id);
                            if (Math.Abs(v) < 0.005) continue;
                            var fmt = c.CategoryPerValues != null && GameLogPerValue != null ? c.CategoryPerValues.Where(df => df.PerValueId == GameLogPerValue.Id).Select(df => df.DisplayFormat).FirstOrDefault() : null;
                            parts.Add(string.Format("{0:" + (string.IsNullOrEmpty(fmt) ? "0.##" : fmt) + "}", v).Trim() + " " + c.Abbreviation.Trim());
                            if (parts.Count == 3) break;
                        }
                        rg.Line = string.Join(" \u00b7 ", parts);
                    }
                    RecentGames.Add(rg);
                }
            }
            catch (Exception ex)
            {
                logger?.LogWarning(ex, "Player recent games failed");
            }
        }

        private async Task FillStatusNewsAsync()
        {
            try
            {
                var ctx = HttpContext.RequestServices.GetService(typeof(RMDBContext)) as RMDBContext;
                if (ctx == null || Player == null || SeasonPlayer == null) return;
                var rows = await ctx.PlayerStatuses.AsNoTracking()
                    .Include(s => s.PlayerStatusType)
                    .Where(s => s.PlayerId == Player.Id && s.DateDeleted == null)
                    .OrderByDescending(s => s.DateAdded)
                    .Take(2)
                    .ToListAsync();
                var dp = new DisplayPlayer();
                dp.SeasonPlayer = SeasonPlayer;
                dp.Positions = (PositionSourcePlayers ?? new List<PositionSourcePlayer>()).Select(x => x.Position).Where(x => x != null).ToList();
                var dark = Request.Cookies["monster-theme"] == "dark";
                StatusNews.Cards = rows.Where(s => s.PlayerStatusType != null)
                    .Select(s => PlayerNewsCards.FromStatus(s, dp, db.Sport, null, false, dark))
                    .ToList();
            }
            catch (Exception ex)
            {
                logger?.LogWarning(ex, "Player status news failed");
            }
        }

        private static UpcomingGame MakeUpcoming(Game g, Team team, Dictionary<int, double> ease)
        {
            var opp = g.GetOpponent(team);
            var e = opp != null && ease.TryGetValue(opp.Id, out var v) ? v : 0;
            var f = (float)Math.Max(-1, Math.Min(1, e));
            return new UpcomingGame
            {
                When = g.GameDate.ToString("ddd M/d"),
                Opponent = (g.AwayTeamId == team.Id ? "@" : "") + (opp?.Code ?? ""),
                EaseColor = f >= 0 ? RotoMonsterUI.ColorHelper.GetGreenColorCode(f, 0, 1, true) : RotoMonsterUI.ColorHelper.GetRedColorCode(-f, 0, 1, true)
            };
        }

        private Dictionary<int, double> OpponentEaseScaled(Season season)
        {
            var sums = new Dictionary<int, (double Total, int Count)>();
            try
            {
                var end = DateTime.UtcNow.Date < season.EndDate.Date ? DateTime.UtcNow.Date : season.EndDate.Date;
                foreach (var playerType in db.GetPlayerTypes())
                {
                    var values = db.GetTeamEaseValuePlayers(playerType, season, season.StartDate.Date, end,
                        SelectedUserLeague.GetCategorySettings(playerType), SelectedUserLeague.ScoringSystem);
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
            var raw = sums.ToDictionary(kv => kv.Key, kv => kv.Value.Count > 0 ? kv.Value.Total / kv.Value.Count : 0);
            var mean = raw.Values.DefaultIfEmpty(0).Average();
            var maxAbs = raw.Values.Select(x => Math.Abs(x - mean)).DefaultIfEmpty(0).Max();
            return raw.ToDictionary(kv => kv.Key, kv => maxAbs > 0 ? (kv.Value - mean) / maxAbs : 0);
        }

        public IActionResult OnPost(
            int? playerId,
            string refresh)
        {
            return RedirectToPage("./Players", new
            {
                playerId = playerId,
                l = SelectedUserLeagueId
            });
        }

        public async Task<IActionResult> OnGetAsync(int playerId, int? l)
        {
            try
            {
                InitGet("Players");

                if (UserId != null)
                {
                    SelectedUserLeagueId = l.GetValueOrDefault();
                    ViewData["UserLeagueList"] = new SelectList(await db.GetTrackedUserLeaguesAsync(UserId), "Id", "ListDisplayTitle");

                    if (SearchTerm != null && SearchTerm.Length > 0)
                    {
                        IEnumerable<Player> players = db.GetPlayerByName(SearchTerm);
                        if (players.Count() == 1)
                            playerId = players.First().Id;
                    }

                    if (playerId == 0)
                    {
                        if (SearchTerm != null && SearchTerm.Length > 0)
                            AddErrorMessage("Could not find a single match, or there were multiple matches for " + SearchTerm);
                        return Page();
                    }

                    OwnedInUserLeagues = new List<UserLeague>();
                    AvailableInUserLeagues = new List<UserLeague>();
                    foreach (var userLeague in await db.GetTrackedUserLeaguesAsync(UserId))
                    {
                        if (userLeague.TrackLeague)
                        {
                            bool owned = false;
                            foreach (var teamPlayer in await db.GetUserLeagueTeamPlayersAsync(userLeague))
                            {
                                if (teamPlayer.PlayerId == playerId)
                                {
                                    if (teamPlayer.UserLeagueTeam.ProviderId == userLeague.MyProviderTeamId)
                                        OwnedInUserLeagues.Add(userLeague);
                                    owned = true;
                                    break;
                                }
                            }
                            if (!owned)
                                AvailableInUserLeagues.Add(userLeague);
                        }
                    }
                }

                SelectedUserLeague = await db.SelectUserLeagueAsync(UserId, l == null ? await db.GetDefaultUserLeagueAsync() : await db.GetUserLeagueAsync(l.GetValueOrDefault()));
                SelectedUserLeagueId = SelectedUserLeague.Id;

                SeasonPlayer = db.GetSeasonPlayer(playerId);
                if (SeasonPlayer == null)
                {
                    AddErrorMessage("Player ID " + playerId.ToString() + " was not found in the current season.");
                    return Page();
                }
                else
                {
                    var season = db.GetDefaultSeason();

                    ViewData["pagetitle"] = "RotoMonster " + db.Sport.Title + " - " + SeasonPlayer.Player.ForwardName;

                    CategoriesString categoryString = SelectedUserLeague.GetCategoriesString(SeasonPlayer.PlayerType);
                    if (categoryString == null)
                        categoryString=  (await db.GetDefaultUserLeagueAsync()).GetCategoriesString(SeasonPlayer.PlayerType);

                    if (categoryString == null)
                        return Page();

                    Player = SeasonPlayer.Player;
                    var ops = db.GetOwnershipPlayersWithChange(categoryString.Code, DateTime.UtcNow, 24);
                    OwnershipPlayer = (from op in ops where op.PlayerId == SeasonPlayer.PlayerId select op).FirstOrDefault();
                    PlayerStatus = await db.GetPlayerActivePlayerStatusAsync(Player.Id);
                    HistoryTableModel.Sport = db.Sport;
                    HistoryTableModel.PlayerType = SeasonPlayer.PlayerType;
                    HistoryTableModel.CategorySettings = db.GetUserLeagueCategorySettings(SelectedUserLeague, SeasonPlayer.PlayerType);
                    HistoryTableModel.UserDisplayCategories = await db.GetUserDisplayCategoriesAsync(UserId, SelectedUserLeague, SeasonPlayer.PlayerType);
                    HistoryTableModel.DisplayPerValue = db.GetDefaultDisplayPerValue(SeasonPlayer.PlayerTypeId);
                    HistoryTableModel.GamesCategoryId = db.GetGamesCategory(SeasonPlayer.PlayerTypeId).Id;
                    HistoryTableModel.BeforeCategories = db.GetBeforeDisplayCategories(SeasonPlayer.PlayerType);
                    HistoryTableModel.AfterCategories = db.GetAfterDisplayCategories(SeasonPlayer.PlayerType);
                    HistoryTableModel.DisplayPlayers = new List<DisplayPlayer>();
                    HistoryTableModel.IsPlayerHistory = true;
                    HistoryTableModel.ColorStats = false;
                    HistoryTableModel.ShowInjuries = false;
                    HistoryTableModel.PlayerHeaderTitle = "Data Set";
                    HistoryTableModel.ValuePerValues = db.GetPerValues(SeasonPlayer.PlayerType.Id);
                    HistoryTableModel.ShowAdp = false;
                    HistoryTableModel.UserDisplayColumns = new UserDisplayColumns();
                    HistoryTableModel.UserDisplayColumns.DisplayAll = true;
                    HistoryTableModel.MonsterBarGame = db.GetMonsterBar(SeasonPlayer.PlayerType, season, SelectedUserLeague.GetCategorySettings(SeasonPlayer.PlayerType), SelectedUserLeague.ScoringSystem, db.GetPerGamePerValue(SeasonPlayer.PlayerTypeId), db.GetUserLeagueLeagueSize(SelectedUserLeague, SeasonPlayer.PlayerType), SelectedUserLeague.ActiveSize(SeasonPlayer.PlayerType));

                    await FillPlayerExtrasAsync(season);
                    PositionSourcePlayers = (from p in db.GetUserLeagueSeasonPlayerPositions(SelectedUserLeague, season) where p.PlayerId == SeasonPlayer.PlayerId select p).ToList();

                    DisplayPlayer fullSeasonDisplayPlayer = AddDisplayPlayer(null, season.Title, false, season, season.StartDate, season.UpdatedDate);
                    if (fullSeasonDisplayPlayer != null)
                    {
                        var weekPlayer = AddDisplayPlayer(fullSeasonDisplayPlayer, "1 Week", true, season, season.UpdatedDate.AddDays(-6), season.UpdatedDate);
                        var threeWeekPlayer = AddDisplayPlayer(fullSeasonDisplayPlayer, "3 Weeks", true, season, season.UpdatedDate.AddDays(-20), season.UpdatedDate);

                        var monthDisplayPlayer = AddDisplayPlayer(fullSeasonDisplayPlayer, "Past Month", true, season, season.UpdatedDate.AddMonths(-1), season.UpdatedDate);
                        var prevDisplayPlayer = (monthDisplayPlayer != null ? monthDisplayPlayer : fullSeasonDisplayPlayer);
                        AddDisplayPlayer(prevDisplayPlayer, "Past 2 Months", true, season, season.UpdatedDate.AddMonths(-2), season.UpdatedDate);
                    }

                    if (HistoryTableModel.DisplayPlayers.Count() > 0)
                    {
                        var dp = (DisplayPlayer)HistoryTableModel.DisplayPlayers[HistoryTableModel.DisplayPlayers.Count() - 1];
                        dp.ShowBottomBorder = true;
                    }

                    var pastSeason1 = db.GetPreviousSeason(season.Year.GetValueOrDefault() - 1);
                    if (pastSeason1 != null)
                        AddDisplayPlayer(null, pastSeason1.Title, false, pastSeason1, pastSeason1.StartDate, pastSeason1.UpdatedDate);

                    var pastSeason2 = db.GetPreviousSeason(season.Year.GetValueOrDefault() - 2);
                    if (pastSeason2 != null)
                        AddDisplayPlayer(null, pastSeason2.Title, false, pastSeason2, pastSeason2.StartDate, pastSeason2.UpdatedDate);

                    var pastSeason3 = db.GetPreviousSeason(season.Year.GetValueOrDefault() - 3);
                    if (pastSeason3 != null)
                        AddDisplayPlayer(null, pastSeason3.Title, false, pastSeason3, pastSeason3.StartDate, pastSeason3.UpdatedDate);

                    // game log
                    var gamePerValue = (from pv in db.GetPerValues(SeasonPlayer.PlayerTypeId) where pv.CategoryId == db.GetGamesCategory(SeasonPlayer.PlayerTypeId).Id select pv).FirstOrDefault();
                    var valueAverages = new ValueAverages();
                    var lastSeasonValuePlayers = db.GetValuePlayers(
                        SeasonPlayer.PlayerType,
                        pastSeason1,
                        pastSeason1.StartDate,
                        pastSeason1.EndDate,
                        0,
                        SelectedUserLeague.GetCategorySettings(SeasonPlayer.PlayerType),
                        SelectedUserLeague.ScoringSystem,
                        gamePerValue,
                        SelectedUserLeague.NumberOfTeams * SelectedUserLeague.PlayersPerTeam,
                        true,
                        out valueAverages
                        );

                    GameLogPerValue = (from pv in db.GetPerValues(SeasonPlayer.PlayerTypeId) where pv.CategoryId == null select pv).FirstOrDefault();

                    List<ValuePlayer> teamEaseValuePlayers = db.GetTeamEaseValuePlayers(SeasonPlayer.PlayerType, season, season.StartDate, season.UpdatedDate, SelectedUserLeague.GetCategorySettings(SeasonPlayer.PlayerType), SelectedUserLeague.ScoringSystem);
                    GameLogGames = db.GetPlayerStatPlayerGameLog(SelectedUserLeague, SeasonPlayer.Player, SeasonPlayer.PlayerType, GameLogPerValue, valueAverages, season, teamEaseValuePlayers);

                    //if (pastSeason1 != null)
                    //{
                    //    teamEaseValuePlayers = db.GetTeamEaseValuePlayers(SeasonPlayer.PlayerType, pastSeason1, pastSeason1.StartDate, pastSeason1.UpdatedDate, SelectedUserLeague.GetCategorySettings(SeasonPlayer.PlayerType), SelectedUserLeague.ScoringSystem);
                    //    var gl = db.GetPlayerStatPlayerGameLog(SelectedUserLeague, SeasonPlayer.Player, SeasonPlayer.PlayerType, GameLogPerValue, valueAverages, pastSeason1, teamEaseValuePlayers);
                    //    if (gl.Count > 0)
                    //    {
                    //        GameLogGames.Add(new GameLogGame() { IsBreak = true });
                    //        foreach (var lg in gl)
                    //            GameLogGames.Add(lg);
                    //    }
                    //}
                    //if (pastSeason2 != null)
                    //{
                    //    teamEaseValuePlayers = db.GetTeamEaseValuePlayers(SeasonPlayer.PlayerType, pastSeason2, pastSeason2.StartDate, pastSeason2.UpdatedDate, SelectedUserLeague.GetCategorySettings(SeasonPlayer.PlayerType), SelectedUserLeague.ScoringSystem);
                    //    var gl = db.GetPlayerStatPlayerGameLog(SelectedUserLeague, SeasonPlayer.Player, SeasonPlayer.PlayerType, GameLogPerValue, valueAverages, pastSeason2, teamEaseValuePlayers);
                    //    if (gl.Count > 0)
                    //    {
                    //        GameLogGames.Add(new GameLogGame() { IsBreak = true });
                    //        foreach (var lg in gl)
                    //            GameLogGames.Add(lg);
                    //    }
                    //}

                    if (GameLogGames.Count > 0 && GameLogGames.First().IsBreak)
                        GameLogGames.RemoveAt(0);

                    FillRecentGames();
                    await FillStatusNewsAsync();

                    return Page();
                }
            }
            catch (Exception ex)
            {
                logger.LogError("Players: User " + (UserId != null ? UserId : "[no user]") + " " + playerId.ToString() + " " + ex.Message);
            }

            return Page();
        }

        public DisplayPlayer AddDisplayPlayer(
            DisplayPlayer previousDisplayPlayer,    // don't add new player if games count matches previous player, set to null if it doesn't matter
            string title,
            bool indentTitle,
            Season season,
            DateTime startDate,
            DateTime endDate)
        {
            DisplayPlayer addedDisplayPlayer = null;

            var valuePlayersHash = new Dictionary<int, List<ValuePlayer>>();
            List<ValuePlayer> valuePlayers = null;
            foreach (var pv in db.GetPerValues(SeasonPlayer.PlayerType.Id))
            {
                ValueAverages outValueAverages = null;
                var vps = db.GetValuePlayers(
                    SeasonPlayer.PlayerType,
                    season,
                    startDate,
                    endDate,
                    0,
                    db.GetUserLeagueCategorySettings(SelectedUserLeague, SeasonPlayer.PlayerType),
                    db.GetUserLeagueScoringSystem(SelectedUserLeague),
                    pv,
                    db.GetUserLeagueLeagueSize(SelectedUserLeague, SeasonPlayer.PlayerType),
                    true,
                    out outValueAverages);
                valuePlayersHash[pv.Id] = vps;
                if (pv.IsDefault.GetValueOrDefault())
                    valuePlayers = vps;
            }

            var gamesCat = db.GetGamesCategory(SeasonPlayer.PlayerTypeId);

            var vp = (from v in valuePlayers where v.StatPlayer.Player.Id == SeasonPlayer.PlayerId select v).FirstOrDefault();
            var sp = (from s in db.GetSeasonPlayers(season, SeasonPlayer.PlayerType) where s.PlayerId == SeasonPlayer.PlayerId select s).FirstOrDefault();

            if (vp == null || (previousDisplayPlayer != null && previousDisplayPlayer.StatPlayer.Get(gamesCat.Id) == vp.StatPlayer.Get(gamesCat.Id)))
                return null;

            if (vp != null && sp != null)
            {
                DisplayPlayer dp = new DisplayPlayer();
                dp.SeasonPlayer = sp;
                dp.DisplayTitle = title;
                dp.IndentDisplayTitle = indentTitle;
                dp.StatPlayer = vp.StatPlayer;
                dp.ValuePlayer = vp;
                foreach (var pv in db.GetPerValues(SeasonPlayer.PlayerType.Id))
                {
                    var vps = valuePlayersHash[pv.Id];
                    var pvp = (from v in vps where v.Player.Id == vp.Player.Id select v).FirstOrDefault();
                    if (pvp != null)
                    {
                        var dvp = new DisplayValuePlayer();
                        dvp.PerValue = pv;
                        dvp.Title = pv.Title;
                        dvp.ValuePlayer = pvp;
                        dp.DisplayValuePlayers.Add(dvp);
                    }
                }
                dp.Positions = (from p1 in PositionSourcePlayers where p1.PlayerId == vp.StatPlayer.Player.Id select p1.Position).ToList();

                if (season.StartDate == startDate && season.UpdatedDate == endDate)
                {
                    var monsterBar = db.GetMonsterBar(SeasonPlayer.PlayerType, season, SelectedUserLeague.GetCategorySettings(SeasonPlayer.PlayerType), SelectedUserLeague.ScoringSystem, db.GetPerGamePerValue(SeasonPlayer.PlayerTypeId), db.GetUserLeagueLeagueSize(SelectedUserLeague, SeasonPlayer.PlayerType), SelectedUserLeague.ActiveSize(SeasonPlayer.PlayerType));
                    dp.MonsterBarGamePlayer = (from p in monsterBar.MonsterBarPlayers where p.Player.Id == SeasonPlayer.PlayerId select p).FirstOrDefault();
                }

                HistoryTableModel.DisplayPlayers.Add(dp);

                addedDisplayPlayer = dp;
            }

            return addedDisplayPlayer;
        }

    }
}
