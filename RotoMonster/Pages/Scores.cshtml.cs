using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RotoMonster.Core;
using RotoMonster.Core.PartialViewModels;
using RotoMonster.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace RotoMonster.Pages
{
    public class ScoresModel : RMPageModel
    {

        [BindProperty] public int SelectedUserLeagueId { get; set; }
        [BindProperty] [Display(Name = "Show Top Players That Missed Games")] public bool ShowTopToo { get; set; } = true;

        [BindProperty] public DateTime SelectedDate { get; set; }
        public List<Game> ScheduleGames { get; set; }
        public Sport Sport { get; set; }
        public BoxScoreModel BoxScoreModel { get; set; } = new BoxScoreModel();

        public ScoresModel(IConfiguration config, IRMData db, IRMSharedData sharedDb, UserManager<ApplicationUser> userManager, IHttpContextAccessor contextAccessor, ILogger<PageModel> logger)
            : base(config, db, sharedDb, userManager, contextAccessor, logger)
        {
        }

        public IActionResult OnPost(string backbutton, string nextbutton, string viewgame, int? newgameid, int? prevgameid, string prevdate)
        {
            if (newgameid == null && prevgameid != null)
                newgameid = prevgameid;

            if (!string.IsNullOrEmpty(backbutton))
            {
                SelectedDate = SelectedDate.AddDays(-1);
                newgameid = null;
            }
            else if (!string.IsNullOrEmpty(nextbutton))
            {
                SelectedDate = SelectedDate.AddDays(1);
                newgameid = null;
            }
            else
            {
                try
                {
                    DateTime prevDate = Convert.ToDateTime(prevdate);
                    if (prevDate != SelectedDate)
                        newgameid = null;
                }
                catch
                {
                }
            }

            return RedirectToPage("./Scores", new
            {
                l = SelectedUserLeagueId,
                date = SelectedDate,
                g = newgameid,
                top = ShowTopToo
            });
        }

        public async Task OnGetAsync(DateTime date, int? g, int? l, bool? top, int? hid)
        {
            InitGet("Scores");

            if (UserId != null)
            {
                if (l != null)
                    SelectedUserLeagueId = l.GetValueOrDefault();
                else if (SelectedUserLeagues != null && SelectedUserLeagues.Count > 0)
                    SelectedUserLeagueId = SelectedUserLeagues[0].Id;
                ViewData["UserLeagueList"] = new SelectList(await db.GetTrackedUserLeaguesAsync(UserId), "Id", "ListDisplayTitle");
            }

            if (hid.GetValueOrDefault(0) > 0)
                ViewData["Helper"] = db.GetHelper(hid.GetValueOrDefault(0));

            ShowTopToo = top.GetValueOrDefault(true);
            Sport = db.Sport;
            var season = db.GetDefaultSeason();
            SelectedDate = date.Ticks == 0 ? db.GetStartedGameDate(season) : date;

            int gameId = g.GetValueOrDefault(0);
            if (gameId > 0)
            {
                var game = db.GetGame(gameId);
                if (game != null)
                    SelectedDate = game.GameDate;
                else
                    gameId = 0;
            }

            UserLeague userLeague = await db.SelectUserLeagueAsync(UserId, await db.GetUserLeagueAsync(UserId, SelectedUserLeagueId));
            if (userLeague == null)
                userLeague = await db.GetDefaultUserLeagueAsync();

            var playerDefaultPositions = db.GetUserLeagueSeasonPlayerPositions(userLeague, season);
            List<OwnershipPlayer> trendingPlayers = db.GetTrendingPlayers();
            var waiverPlayers = await db.GetUserLeagueWaiverPlayersAsync(userLeague);
            var injuries = await db.GetPlayerInjuriesAsync();
            var playerStatuses = await db.GetActivePlayerStatusesAsync();

            ScheduleGames = db.GetGames(season, SelectedDate, SelectedDate);

            ValueAverages outValueAverages;
            var valuePlayersHash = new Dictionary<int, List<ValuePlayer>>();
            List<ValuePlayer> valuePlayers = new List<ValuePlayer>();
            foreach (var playerType in db.GetPlayerTypes())
            {
                var pv = db.GetPerGamePerValue(playerType.Id);
                foreach (var valuePlayer in db.GetValuePlayers(playerType, season, SelectedDate, SelectedDate, 0,
                    userLeague.GetCategorySettings(playerType), userLeague.ScoringSystem, pv, userLeague.Size, false, out outValueAverages))
                {
                    valuePlayers.Add(valuePlayer);
                }
            }

            Dictionary<int, MonsterBar> monsterBars = new Dictionary<int, MonsterBar>();
            Dictionary<int, List<OwnershipPlayer>> ownershipPlayerLists = new Dictionary<int, List<OwnershipPlayer>>();
            foreach (var playerType in db.GetPlayerTypes())
            {
                monsterBars[playerType.Id] = db.GetMonsterBar(playerType, season, userLeague.GetCategorySettings(playerType), userLeague.ScoringSystem, db.GetPerGamePerValue(playerType.Id), db.GetUserLeagueLeagueSize(userLeague, playerType), userLeague.ActiveSize(playerType));
                ownershipPlayerLists[playerType.Id] = db.GetOwnershipPlayersWithChange(await db.GetUserLeagueCategoryCodeAsync(userLeague, playerType), DateTime.UtcNow, 24);
            }

            if (gameId > 0)
            {
                Game game = db.GetGame(gameId);
                var boxScoreModel = new BoxScoreModel();
                boxScoreModel.PlayerTypes = db.GetPlayerTypes();
                boxScoreModel.Game = game;
                boxScoreModel.Articles = db.GetGameArticles(game);
                boxScoreModel.BoxScorePlayers = db.GetBoxScorePlayers(season, game, !ShowTopToo);
                foreach (var boxScorePlayer in boxScoreModel.BoxScorePlayers)
                    boxScorePlayer.ValuePlayer = (from vp in valuePlayers where vp.Player.Id == boxScorePlayer.SeasonPlayer.PlayerId select vp).FirstOrDefault();
                boxScoreModel.BoxScorePlayers = (from p in boxScoreModel.BoxScorePlayers
                                                 orderby p.Team.Id == game.AwayTeamId ? 0 : 1,
                                                 p.ValuePlayer != null ? p.ValuePlayer.LeagueValue : double.MinValue descending,
                                                 p.SeasonPlayer.Player.LastName, p.SeasonPlayer.Player.FirstName descending
                                                 select p).ToList();

                foreach (var team in game.GetTeams())
                {
                    boxScoreModel.TeamPlayerTableModels[team] = new Dictionary<PlayerType, PlayerTableModel>();
                    foreach (var playerType in db.GetPlayerTypes())
                    {
                        var depthPlayers = db.GetDepthPlayers(playerType, await db.GetUserLeagueCategoryCodeAsync(userLeague, playerType), DateTime.UtcNow, false);
                        var playerTableModel = new PlayerTableModel();
                        playerTableModel.DisplayPerValue = db.GetTotalPerValue(playerType.Id);
                        playerTableModel.ShowTrending = true;
                        playerTableModel.ShowRank = false;
                        playerTableModel.SelectedUserLeague = userLeague;
                        playerTableModel.ScoringSystem = userLeague.ScoringSystem;
                        playerTableModel.UserId = UserId;
                        playerTableModel.Sport = db.Sport;
                        playerTableModel.CategorySettings = userLeague.GetCategorySettings(playerType);
                        playerTableModel.PlayerType = playerType;
                        playerTableModel.DisplayPerValue = db.GetTotalPerValue(playerType.Id);
                        playerTableModel.UserDisplayCategories = await db.GetUserDisplayCategoriesAsync(UserId, userLeague, playerType);
                        playerTableModel.GamesCategoryId = db.GetGamesCategory(playerType.Id).Id;
                        playerTableModel.BeforeCategories = db.GetBeforeDisplayCategories(playerType);
                        playerTableModel.AfterCategories = db.GetAfterDisplayCategories(playerType);
                        playerTableModel.DisplayPlayers = new List<DisplayPlayer>();
                        playerTableModel.ValuePerValues.Add(db.GetPerGamePerValue(playerType.Id));
                        playerTableModel.ColorStats = true;
                        playerTableModel.ShowMonsterBot = false;
                        playerTableModel.ShowPositions = false;
                        playerTableModel.ShowInjuries = false;
                        playerTableModel.ShowCurrentGame = false;
                        playerTableModel.ShowPositionalValue = false;
                        playerTableModel.ShowCategoryValues = true;
                        playerTableModel.ShowGames = false;
                        playerTableModel.ShowTeam = false;

                        boxScoreModel.TeamPlayerTableModels[team][playerType] = playerTableModel;
                        playerTableModel.MonsterBarGame = monsterBars[playerType.Id];
                        var ownershipPlayers = ownershipPlayerLists[playerType.Id];
                        foreach (var boxScorePlayer in boxScoreModel.BoxScorePlayers)
                        {
                            if (boxScorePlayer.Team.Id != team.Id)
                                continue;

                            if (boxScorePlayer.SeasonPlayer.PlayerTypeId != playerType.Id)
                                continue;

                            int playerId = boxScorePlayer.SeasonPlayer.PlayerId;
                            var displayPlayer = new DisplayPlayer();
                            playerTableModel.DisplayPlayers.Add(displayPlayer);
                            displayPlayer.SeasonPlayer = db.GetSeasonPlayer(playerId);
                            displayPlayer.ValuePlayer = boxScorePlayer.ValuePlayer;

                            var displayValuePlayer = new DisplayValuePlayer();
                            displayValuePlayer.PerValue = db.GetPerGamePerValue(playerType.Id);
                            displayValuePlayer.Title = "Value";
                            displayValuePlayer.ValuePlayer = boxScorePlayer.ValuePlayer;
                            displayPlayer.DisplayValuePlayers.Add(displayValuePlayer);

                            displayPlayer.StatPlayer = boxScorePlayer.ValuePlayer != null ? boxScorePlayer.ValuePlayer.StatPlayer : null;
                            displayPlayer.MonsterBarGamePlayer = playerTableModel.MonsterBarGame != null ? (from vp in playerTableModel.MonsterBarGame.MonsterBarPlayers where vp.Player.Id == playerId select vp).FirstOrDefault() : null;
                            displayPlayer.Positions = (from p1 in playerDefaultPositions where p1.PlayerId == playerId select p1.Position).ToList();
                            displayPlayer.OwnershipPlayer = (from p1 in ownershipPlayers where p1.PlayerId == playerId select p1).FirstOrDefault();
                            displayPlayer.NoWaiverOwnershipPlayer = trendingPlayers == null ? null : (from p1 in trendingPlayers where p1.PlayerId == playerId select p1).FirstOrDefault();
                            displayPlayer.DepthPlayer = (from p1 in depthPlayers where p1.SeasonPlayer.PlayerId == playerId select p1).FirstOrDefault();
                            if (displayPlayer.DepthPlayer != null)
                                displayPlayer.HigherDepthInjuredDisplayPlayers = displayPlayer.DepthPlayer.GetHigherDepthInjuredDisplayPlayers(playerStatuses);

                            displayPlayer.IsWaiver = (waiverPlayers.Find(p => p.PlayerId == playerId) != null);
                            // displayPlayer.RecentArticles = db.GetPlayerRecentArticles(playerId);
                            displayPlayer.PlayerInjury = (from p1 in injuries where p1.PlayerId == playerId select p1).FirstOrDefault();
                            displayPlayer.PlayerStatus = (from p1 in playerStatuses where p1.PlayerId == playerId select p1).FirstOrDefault();
                        }

                        var filtered = (from dp in playerTableModel.DisplayPlayers where dp.ValuePlayer != null || dp.OwnershipPlayer != null select dp).ToList();
                        var sorted = (from dp in filtered
                                      orderby dp.ValuePlayer != null ? dp.ValuePlayer.LeagueValue : double.MinValue descending,
                                                   dp.OwnershipPlayer != null ? dp.OwnershipPlayer.OwnershipPercent : 0 descending,
                                                   dp.SeasonPlayer.Player.LastName, dp.SeasonPlayer.Player.FirstName descending
                                      select dp).ToList();
                        playerTableModel.DisplayPlayers = sorted;

                        await db.FillDisplayPlayerUserLeagueTeamsAsync(userLeague, playerTableModel.DisplayPlayers);
                    }
                }

                BoxScoreModel = boxScoreModel;
            }

        }

    }
}
