using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RotoMonster.Core;
using RotoMonster.Core.Libs;
using RotoMonster.Data;
using System.Web.Services;
using RotoMonster.Models.Shared;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace RotoMonster.Pages
{
    public class IndexModel : RMPageModel
    {
        public Sport Sport { get; set; }
        public List<UserLeagueView> UserLeagueViews { get; set; }
        public List<DisplayPlayer> InjuryDisplayPlayers { get; set; }
        public List<DisplayPlayer> TrendingDisplayPlayers { get; set; } = new List<DisplayPlayer>();
        public List<Game> CurrentGames { get; set; }
        public List<Game> ScheduleGames { get; set; }
        public List<GameUserLeagueTeamPlayer> ActiveGameUserLeagueTeamPlayers { get; set; }
        public List<GameUserLeagueTeamPlayer> OwnedGameUserLeagueTeamPlayers { get; set; }
        public Game NextGame { get; set; }
        public List<DisplayPlayer> GameScoringAlertDisplayPlayers { get; set; } = new List<DisplayPlayer>();
        public DateTime ScheduleStartDate { get; set; }
        public DateTime ScheduleEndDate { get; set; }
        public List<Article> RecentArticles { get; set; }

        public Season season;
        public List<SeasonPlayer> seasonPlayers;
        public DateTime liveStartDate;
        public DateTime liveEndDate;
        public DateTime liveUpcomingStartDate;
        public DateTime liveUpcomingEndDate;

        public IndexModel(IConfiguration config, IRMData db, IRMSharedData sharedDb, UserManager<ApplicationUser> userManager, IHttpContextAccessor contextAccessor, ILogger<PageModel> logger)
            : base(config, db, sharedDb, userManager, contextAccessor, logger)
        {
        }

        public async Task OnGetAsync(DateTime date, [FromQuery(Name = "l")] int? leagueId)
        {
            InitGet("");

            //if (userManager.GetUserName(contextAccessor.HttpContext.User) != null && UserId.Length > 0)
            //      ("Front Page: " + userManager.GetUserName(contextAccessor.HttpContext.User));

            ViewData["sport"] = db.Sport.Title;
            ViewData["pagetitle"] = "RotoMonster" + db.Sport.Title + " - Automated Fantasy " + db.Sport.SportType + " Rankings and Tools";
            ViewData["tag"] = "Automated Fantasy " + db.Sport.SportType + " Rankings and Tools";
            ViewData["metadescription"] = "Automated Fantasy " + db.Sport.SportType + " rankings and tools (Yahoo!, ESPN, FanTrax).";

            Sport = db.Sport;

            if (false)
            {
                if (!db.GetDefaultSeason().IsFinished)
                {
                    var autoRefreshLeagues = (from l in SelectedUserLeagues where l.SeasonId == db.GetDefaultSeason().Id && l.TrackLeague && l.TimeToRefresh() select l).ToList();
                    Parallel.ForEach(autoRefreshLeagues, l =>
                    {
                        RefreshRosters(l);
                    }
                    );
                }
            }

            RecentArticles = (from a in db.GetRecentArticles(1) where !a.IsAutomated select a).ToList();

            SelectedUserLeagues = await db.GetTrackedUserLeaguesAsync(UserId);

            // The front page renders a card per league, so switching does not
            // change what is shown here. The dropdown lives in _Layout though,
            // so this page still has to show the CURRENT league and persist a
            // change for the pages that do act on it.
            UserLeague selectedUserLeague = leagueId.HasValue
                ? await db.SelectUserLeagueAsync(UserId, await db.GetUserLeagueAsync(UserId, leagueId.Value))
                : await db.SelectUserLeagueAsync(UserId, null);

            // Only bind to a league the dropdown actually offers - GetUserLeague
            // can return one that is not the user's.
            if (selectedUserLeague != null && SelectedUserLeagues != null
                && SelectedUserLeagues.Any(x => x.Id == selectedUserLeague.Id))
            {
                SelectedUserLeagueId = selectedUserLeague.Id;
            }

            List<OwnershipPlayer> ownershipPlayers = null;
            UserLeague userLeague = null;
            if (SelectedUserLeagues.Count > 0)
            {
                userLeague = SelectedUserLeagues.First();
                ownershipPlayers = db.GetAllDefaultOwnershipPlayers(DateTime.UtcNow);
            }
            else
                userLeague = await db.GetDefaultUserLeagueAsync();

            var playerStatuses = await db.GetActivePlayerStatusesAsync();
            MonsterBotLib monsterBotLib = new MonsterBotLib();

            FillFrontGameScoreValues();

            NextGame = db.NextGame(season);

            List<PositionSourcePlayer> playerPositions = (userLeague == null) ? db.GetPlayerSeasonPositions(db.GetDefaultFantasyProvider(), season) : db.GetUserLeagueSeasonPlayerPositions(userLeague, db.GetDefaultSeason());

            var depthPlayers = new List<DepthPlayer>();
            if (userLeague != null)
            {
                foreach (var playerType in db.GetPlayerTypes())
                {
                    foreach (var dp in db.GetDepthPlayers(playerType, userLeague.GetCategoriesString(playerType).Code, DateTime.Now, false))
                        depthPlayers.Add(dp);
                }
            }

            // if (season.HasStarted)
            {
                var trendingPlayers = db.GetTrendingPlayers().OrderByDescending(tp => tp.PercentOwnershipChange).Take(15).ToList();
                if (trendingPlayers.Count() > 0 && trendingPlayers.First().PercentOwnershipChange >= 3)
                {
                    foreach (var tp in trendingPlayers)
                    {
                        var displayPlayer = new DisplayPlayer();
                        displayPlayer.SeasonPlayer = db.GetSeasonPlayer(tp.PlayerId);
                        displayPlayer.AvailableInUserLeagues = new List<UserLeague>();
                        displayPlayer.OwnedInUserLeagues = new List<UserLeague>();
                        if (displayPlayer.SeasonPlayer != null)
                        {
                            //displayPlayer.Game = (from g in CurrentGames where g.AwayTeam.Id == displayPlayer.SeasonPlayer.TeamId || g.HomeTeam.Id == displayPlayer.SeasonPlayer.Team.Id select g).FirstOrDefault();
                            displayPlayer.Positions = (from p1 in playerPositions where p1.PlayerId == tp.Player.Id select p1.Position).ToList();
                            displayPlayer.NoWaiverOwnershipPlayer = tp;
                            displayPlayer.DepthPlayer = (from p1 in depthPlayers where p1.SeasonPlayer.PlayerId == displayPlayer.SeasonPlayer.PlayerId select p1).FirstOrDefault();
                            if (displayPlayer.DepthPlayer != null)
                                displayPlayer.HigherDepthInjuredDisplayPlayers = displayPlayer.DepthPlayer.GetHigherDepthInjuredDisplayPlayers(playerStatuses);
                            foreach (var ul in SelectedUserLeagues)
                            {
                                var owningUserLeagueTeam = ul.OwningUserLeagueTeam(displayPlayer.SeasonPlayer.PlayerId);
                                if (owningUserLeagueTeam == null)
                                    displayPlayer.AvailableInUserLeagues.Add(ul);
                                else if (owningUserLeagueTeam.ProviderId == ul.MyProviderTeamId)
                                    displayPlayer.OwnedInUserLeagues.Add(ul);
                            }
                            TrendingDisplayPlayers.Add(displayPlayer);
                        }
                    }
                }
                else
                {
                    trendingPlayers = null;
                }
            }

            var recentPlayerStatuses = (from ps in playerStatuses where ps.TimeSince.TotalHours < 24 orderby ps.DateAdded descending select ps).ToList();
            if (recentPlayerStatuses.Count > 0)
            {
                InjuryDisplayPlayers = new List<DisplayPlayer>();
                foreach (var ps in recentPlayerStatuses)
                {
                    var seasonPlayer = db.GetSeasonPlayer(ps.PlayerId);
                    if (seasonPlayer != null)
                    {
                        var displayPlayer = new DisplayPlayer();
                        displayPlayer.SeasonPlayer = seasonPlayer;
                        displayPlayer.PlayerStatus = ps;
                        displayPlayer.Positions = (from p1 in playerPositions where p1.PlayerId == seasonPlayer.Player.Id select p1.Position).ToList();
                        if (ownershipPlayers != null)
                            displayPlayer.OwnershipPlayer = (from op in ownershipPlayers where op.PlayerId == ps.PlayerId select op).FirstOrDefault();
                        if (ownershipPlayers == null || displayPlayer.OwnershipPlayer != null)
                            InjuryDisplayPlayers.Add(displayPlayer);
                    }
                }
            }

            var gameScoringAlerts = await db.GetGameScoringAlertsAsync(season, liveStartDate, liveEndDate);

            if (gameScoringAlerts.Count > 0)
            {
                foreach (var a in gameScoringAlerts)
                {
                    var displayPlayer = new DisplayPlayer();
                    displayPlayer.SeasonPlayer = db.GetSeasonPlayer(a.PlayerId);
                    displayPlayer.AvailableInUserLeagues = new List<UserLeague>();
                    displayPlayer.OwnedInUserLeagues = new List<UserLeague>();
                    if (displayPlayer.SeasonPlayer != null)
                    {
                        //displayPlayer.Game = (from g in CurrentGames where g.AwayTeam.Id == displayPlayer.SeasonPlayer.TeamId || g.HomeTeam.Id == displayPlayer.SeasonPlayer.Team.Id select g).FirstOrDefault();
                        displayPlayer.Positions = (from p1 in playerPositions where p1.PlayerId == a.Player.Id select p1.Position).ToList();
                        displayPlayer.GameScoringAlert = a;
                        displayPlayer.DepthPlayer = (from p1 in depthPlayers where p1.SeasonPlayer.PlayerId == displayPlayer.SeasonPlayer.PlayerId select p1).FirstOrDefault();
                        if (displayPlayer.DepthPlayer != null)
                            displayPlayer.HigherDepthInjuredDisplayPlayers = displayPlayer.DepthPlayer.GetHigherDepthInjuredDisplayPlayers(playerStatuses);
                        foreach (var ul in SelectedUserLeagues)
                        {
                            var owningUserLeagueTeam = ul.OwningUserLeagueTeam(displayPlayer.SeasonPlayer.PlayerId);
                            if (owningUserLeagueTeam == null)
                                displayPlayer.AvailableInUserLeagues.Add(ul);
                            else if (owningUserLeagueTeam.ProviderId == ul.MyProviderTeamId)
                                displayPlayer.OwnedInUserLeagues.Add(ul);
                        }
                        GameScoringAlertDisplayPlayers.Add(displayPlayer);
                    }
                }
            }

            UserLeagueViews = new List<UserLeagueView>();
            foreach (var ul in SelectedUserLeagues)
            {
                var allUserLeagueTeamPlayers = await db.GetUserLeagueTeamPlayersAsync(ul);

                var leagueDepthPlayers = new List<DepthPlayer>();
                if (userLeague != null)
                {
                    foreach (var playerType in db.GetPlayerTypes())
                    {
                        foreach (var dp in db.GetDepthPlayers(playerType, ul.GetCategoriesString(playerType).Code, DateTime.Now, false))
                            leagueDepthPlayers.Add(dp);
                    }
                }

                var yesterdayValuePlayers = new List<ValuePlayer>();
                MonsterBar monsterBar = null;

                foreach (var pt in db.GetPlayerTypes())
                {
                    var perValue = db.GetDefaultPerValue(pt.Id);
                    int leagueSize = db.GetUserLeagueLeagueSize(ul, pt);
                    var outValueAverages = new ValueAverages();
                    foreach (var valuePlayer in db.GetValuePlayers(pt, season, liveStartDate, liveEndDate, 0, ul.GetCategorySettings(pt), ul.ScoringSystem, db.GetPerGamePerValue(pt.Id), leagueSize, false, out outValueAverages))
                        yesterdayValuePlayers.Add(valuePlayer);
                    if (monsterBar == null)
                        monsterBar = db.GetMonsterBar(pt, season, ul.GetCategorySettings(pt), ul.ScoringSystem, perValue, leagueSize, ul.ActiveSize(pt));
                    else
                    {
                        foreach (var mbp in db.GetMonsterBar(pt, season, ul.GetCategorySettings(pt), ul.ScoringSystem, perValue, leagueSize, ul.ActiveSize(pt)).MonsterBarPlayers)
                            monsterBar.MonsterBarPlayers.Add(mbp);
                    }
                }

                //DateTime twoWeekEndDate = yesterday;
                //DateTime twoWeekStartDate = yesterday.AddDays(-13);
                //var twoWeekValuePlayers = new List<ValuePlayer>();
                //foreach (var pt in db.GetPlayerTypes())
                //{
                //    var outValueAverages = new ValueAverages();
                //    foreach (var valuePlayer in db.GetValuePlayers(pt, season, twoWeekStartDate, twoWeekEndDate, 0, ul.GetCategorySettings(pt), ul.ScoringSystem, db.GetPerGamePerValue(pt.Id), ul.NumberOfTeams * ul.PlayersPerTeam, out outValueAverages))
                //        twoWeekValuePlayers.Add(valuePlayer);
                //}

                var userLeagueView = new UserLeagueView();
                userLeagueView.UserLeague = ul;
                userLeagueView.Yesterday = db.GetLiveGameDate(season);
                userLeagueView.MonsterBar = monsterBar;
                UserLeagueViews.Add(userLeagueView);

                List<OwnershipPlayer> leagueOwnershipPlayers = new List<OwnershipPlayer>();

                foreach (var t in ul.UserLeagueTeams)
                {
                    if (t.ProviderId == ul.MyProviderTeamId)
                    {
                        foreach (var pt in db.GetPlayerTypes())
                        {
                            foreach (var op in db.GetOwnershipPlayersWithChange(await db.GetUserLeagueCategoryCodeAsync(ul, pt), DateTime.UtcNow, 24))
                            {
                                var seasonPlayer = (from sp in seasonPlayers where sp.PlayerId == op.PlayerId && sp.PlayerTypeId == pt.Id select sp).FirstOrDefault();
                                if (seasonPlayer != null)
                                    leagueOwnershipPlayers.Add(op);
                            }
                        }

                        userLeagueView.UserLeagueTeamAnalysis = t.GetUserLeagueTeamAnalysis(db.GetPlayerTypes(), seasonPlayers, leagueOwnershipPlayers, db.Sport.IsNFL, true);

                        foreach (var p in t.UserLeagueTeamPlayers)
                        {
                            var playerStatus = (from ps in playerStatuses where ps.PlayerId == p.PlayerId select ps).FirstOrDefault();
                            if (playerStatus != null)
                            {
                                var seasonPlayer = db.GetSeasonPlayer(p.PlayerId);
                                if (seasonPlayer != null)
                                {
                                    var displayPlayer = new DisplayPlayer();
                                    displayPlayer.UserLeagueTeam = t;
                                    displayPlayer.SeasonPlayer = seasonPlayer;
                                    displayPlayer.PlayerStatus = playerStatus;
                                    // displayPlayer.Game = (from g in CurrentGames where g.AwayTeam.Id == seasonPlayer.TeamId || g.HomeTeam.Id == seasonPlayer.Team.Id select g).FirstOrDefault();
                                    if (leagueOwnershipPlayers != null)
                                        displayPlayer.OwnershipPlayer = (from op in leagueOwnershipPlayers where op.PlayerId == seasonPlayer.PlayerId select op).FirstOrDefault();
                                    displayPlayer.IsActive = p.IsActive;
                                    displayPlayer.IsIR = p.IsIR;
                                    displayPlayer.IsMyPlayer = true;
                                    displayPlayer.Positions = (from p1 in playerPositions where p1.PlayerId == seasonPlayer.Player.Id select p1.Position).ToList();
                                    userLeagueView.PlayerStatusDisplayPlayers.Add(displayPlayer);
                                }
                            }
                        }

                        userLeagueView.PlayerStatusDisplayPlayers = (from dp in userLeagueView.PlayerStatusDisplayPlayers
                                                                     orderby dp.PlayerStatus.DateAdded descending, dp.PlayerStatus.Player.LastName ascending, dp.PlayerStatus.Player.FirstName ascending
                                                                     select dp).ToList();

                        // add yesterday players
                        foreach (var p in t.UserLeagueTeamPlayers)
                        {
                            var yesterdayPlayer = (from yp in yesterdayValuePlayers where yp.Player.Id == p.PlayerId select yp).FirstOrDefault();
                            if (yesterdayPlayer != null)
                            {
                                var displayPlayer = new DisplayPlayer();
                                displayPlayer.SeasonPlayer = (from sp in seasonPlayers where sp.PlayerId == yesterdayPlayer.Player.Id select sp).FirstOrDefault();
                                if (displayPlayer.SeasonPlayer != null)
                                {
                                    displayPlayer.Game = (from g in CurrentGames where g.AwayTeam.Id == displayPlayer.SeasonPlayer.TeamId || g.HomeTeam.Id == displayPlayer.SeasonPlayer.Team.Id select g).FirstOrDefault();
                                    displayPlayer.ValuePlayer = yesterdayPlayer;
                                    displayPlayer.Positions = (from p1 in playerPositions where p1.PlayerId == yesterdayPlayer.Player.Id select p1.Position).ToList();
                                    if (leagueOwnershipPlayers != null)
                                        displayPlayer.OwnershipPlayer = (from op in leagueOwnershipPlayers where op.PlayerId == yesterdayPlayer.Player.Id select op).FirstOrDefault();
                                    // displayPlayer.PreviousPeriodValuePlayer = (from p2 in twoWeekValuePlayers where p2.Player.Id == yesterdayPlayer.Player.Id select p2).FirstOrDefault();
                                    displayPlayer.UserLeagueTeam = t;
                                    displayPlayer.IsActive = p.IsActive;
                                    displayPlayer.IsIR = p.IsIR;
                                    displayPlayer.IsMyPlayer = true;
                                    displayPlayer.MonsterBarGamePlayer = (from mbp in monsterBar.MonsterBarPlayers where mbp.Player.Id == yesterdayPlayer.Player.Id select mbp).FirstOrDefault();
                                    userLeagueView.DayDisplayPlayers.Add(displayPlayer);
                                }
                            }
                        }
                        if (userLeagueView.DayDisplayPlayers != null)
                            userLeagueView.DayDisplayPlayers = (from ddp in userLeagueView.DayDisplayPlayers orderby ddp.ValuePlayer.LeagueValue descending select ddp).ToList();

                        // top free agents
                        foreach (var playerType in db.GetPlayerTypes())
                        {
                            foreach (var displayPlayer in monsterBotLib.GetRecommendedFreeAgents(playerType, 5, monsterBar.MonsterBarPlayers,
                                allUserLeagueTeamPlayers, t.UserLeagueTeamPlayers, seasonPlayers, playerStatuses,
                                playerPositions, leagueOwnershipPlayers))
                            {
                                displayPlayer.DepthPlayer = (from p1 in leagueDepthPlayers where p1.SeasonPlayer.PlayerId == displayPlayer.SeasonPlayer.PlayerId select p1).FirstOrDefault();
                                displayPlayer.MonsterBarGamePlayer = (from mbp in monsterBar.MonsterBarPlayers where mbp.Player.Id == displayPlayer.SeasonPlayer.PlayerId select mbp).FirstOrDefault();
                                if (displayPlayer.DepthPlayer != null)
                                    displayPlayer.HigherDepthInjuredDisplayPlayers = displayPlayer.DepthPlayer.GetHigherDepthInjuredDisplayPlayers(playerStatuses);
                                userLeagueView.FreeAgentDisplayPlayers.Add(displayPlayer);
                            }
                        }

                        // monsterbot
                        if (ul.LineupFrequency == "D")
                        {
                            var myUserLeagueTeam = ul.MyUserLeagueTeam;
                            if (myUserLeagueTeam != null && myUserLeagueTeam.UserLeagueTeamPlayers.Count() > 0)
                            {
                                var monsterBotPlayers = monsterBotLib.GetAllMonsterBotPlayers(
                                   db.GetPlayerTypes(),
                                   ul.UserLeagueActiveRosterSpots,
                                   myUserLeagueTeam.UserLeagueTeamPlayers,
                                   leagueOwnershipPlayers,
                                   season,
                                   seasonPlayers,
                                   playerStatuses,
                                   db.GetUserLeagueSeasonPlayerPositions(ul, season),
                                   await db.GetPlayerGameStatesAsync(liveUpcomingStartDate, liveUpcomingEndDate),
                                   ScheduleGames
                                   );
                                userLeagueView.MonsterBotPlayers = monsterBotLib.GetNonOKMonsterBotPlayers(monsterBotPlayers, Sport, ul);
                            }
                        }
                    }
                }
            }
        }

        public async Task<IActionResult> OnGetUpdateRostersAsync(int id)
        {
            UserLeague userLeague = await db.GetUserLeagueAsync(UserId, id);
            RefreshRosters(userLeague);

            TempData["test"] = 1;


            return RedirectToPage("./Index");
        }

        public async Task<IActionResult> OnGetUpdateAllRostersAsync()
        {
            // Yahoo takes every league key in one request, so all of them come
            // back together rather than one round trip each.
            var yahooProvider = db.GetFantasyProvider("Yahoo!");
            var handledByProvider = new HashSet<int>();
            var totalRefreshed = 0;

            if (yahooProvider != null)
            {
                var importService = new RotoMonster.Data.LeagueImportService(db, sharedDb, config);
                var refreshed = await importService.RefreshRostersAsync(UserId, "Yahoo!");

                if (!refreshed.Success && !string.IsNullOrEmpty(refreshed.ErrorMessage))
                {
                    AddErrorMessage(refreshed.ErrorMessage);
                }
                else
                {
                    // Counted rather than announced, so the total below covers
                    // every provider in one line.
                    totalRefreshed += refreshed.RefreshedCount;

                    foreach (var failed in refreshed.Leagues.Where(l => !l.Refreshed))
                        AddErrorMessage(failed.Title + ": " + failed.Message);
                }

                handledByProvider.Add(yahooProvider.Id);
            }

            // Everything else still goes one at a time, since those providers
            // have no implementation behind the layer yet.
            foreach (var ul in SelectedUserLeagues)
            {
                if (!ul.TrackLeague)
                    continue;
                if (handledByProvider.Contains(ul.FantasyProviderId))
                    continue;

                UserLeague userLeague = await db.GetUserLeagueAsync(UserId, ul.Id);

                // announce false so these do not each add their own line.
                RefreshRosters(userLeague, false);
                if (LastRefreshSucceeded)
                    totalRefreshed++;
            }

            if (totalRefreshed > 0)
            {
                AddMessage("Refreshed rosters for " + totalRefreshed
                           + (totalRefreshed == 1 ? " league." : " leagues."));
            }

            return RedirectToPage("./Index");
        }

        public void FillFrontGameScoreValues()
        {
            season = db.GetDefaultSeason();
            seasonPlayers = db.GetAllSeasonPlayers(season);

            liveStartDate = db.GetLiveStartGameDate(season);
            liveEndDate = db.GetLiveEndGameDate(season);

            liveUpcomingStartDate = db.GetUpcomingGamesStartDate(season);
            liveUpcomingEndDate = db.GetUpcomingGamesEndDate(season);

            ScheduleStartDate = liveUpcomingStartDate;
            ScheduleEndDate = liveUpcomingEndDate;
            ScheduleGames = db.GetGames(season, liveUpcomingStartDate, liveUpcomingEndDate);
            CurrentGames = db.GetGames(season, liveStartDate, liveEndDate);

            OwnedGameUserLeagueTeamPlayers = new List<GameUserLeagueTeamPlayer>();
            foreach (var ul in SelectedUserLeagues)
            {
                foreach (var tp in ul.GetGameUserLeagueTeamPlayers(ScheduleGames, seasonPlayers))
                {
                    var current = (from p in OwnedGameUserLeagueTeamPlayers where p.UserLeagueTeamPlayer.PlayerId == tp.UserLeagueTeamPlayer.PlayerId select p).FirstOrDefault();
                    if (current != null)
                    {
                        if (tp.UserLeagueTeamPlayer.IsActive)
                            current.UserLeagueTeamPlayer.IsActive = true;
                    }
                    else
                        OwnedGameUserLeagueTeamPlayers.Add(tp);
                }
            }
            ActiveGameUserLeagueTeamPlayers = (from tp in OwnedGameUserLeagueTeamPlayers where tp.UserLeagueTeamPlayer.IsActive select tp).ToList();
        }

        public IActionResult OnGetFrontGameScores()
        {
            FillFrontGameScoreValues();

            var frontGameScoresModel = new FrontGameScoresModel();
            frontGameScoresModel.Sport = db.Sport;
            frontGameScoresModel.ScheduleStartDate = ScheduleStartDate;
            frontGameScoresModel.ScheduleEndDate = ScheduleEndDate;
            frontGameScoresModel.ScheduleGames = ScheduleGames;
            frontGameScoresModel.OwnedGameUserLeagueTeamPlayers = OwnedGameUserLeagueTeamPlayers;
            frontGameScoresModel.ActiveGameUserLeagueTeamPlayers = ActiveGameUserLeagueTeamPlayers;

            // Return the partial view with the model
            return Partial("_FrontGameScores", frontGameScoresModel);
        }

    }

    public class UserLeagueView
    {
        public UserLeague UserLeague { get; set; }
        public UserLeagueTeamAnalysis UserLeagueTeamAnalysis { get; set; }
        public List<DisplayPlayer> PlayerStatusDisplayPlayers { get; set; } = new List<DisplayPlayer>();
        public List<DisplayPlayer> DayDisplayPlayers { get; set; } = new List<DisplayPlayer>();
        public List<DisplayPlayer> FreeAgentDisplayPlayers { get; set; } = new List<DisplayPlayer>();
        public List<MonsterBotPlayer> MonsterBotPlayers { get; set; }
        public MonsterBar MonsterBar { get; set; } = new MonsterBar();
        public DateTime Yesterday { get; set; }

    }


}
