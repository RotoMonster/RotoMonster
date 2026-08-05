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
using RotoMonster.Models.Shared;
using RotoMonster.Data;
using RotoMonster.Pages;
using RotoMonster.Pages.Shared;

namespace RotoMonster
{
    public class DetailsModel : RMPageModel
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

        public DetailsModel(IConfiguration config, IRMData db, IRMSharedData sharedDb, UserManager<ApplicationUser> userManager, IHttpContextAccessor contextAccessor, ILogger<PageModel> logger)
            : base(config, db, sharedDb, userManager, contextAccessor, logger)
        {
        }

        public async Task<IActionResult> OnGetAsync(int playerId)
        {
            SelectedUserLeague = await db.SelectUserLeagueAsync(UserId, null);

            SeasonPlayer = db.GetSeasonPlayer(playerId);
            if (SeasonPlayer == null)
            {
                return RedirectToPage("./NotFound");
            }
            else
            {
                Player = SeasonPlayer.Player;
                var ops = db.GetOwnershipPlayersWithChange(SelectedUserLeague.GetCategoriesString(SeasonPlayer.PlayerType).Code, DateTime.UtcNow, 24);
                OwnershipPlayer = (from op in ops where op.PlayerId == SeasonPlayer.PlayerId select op).FirstOrDefault();

                HistoryTableModel.CategorySettings = db.GetUserLeagueCategorySettings(SelectedUserLeague, SeasonPlayer.PlayerType);
                HistoryTableModel.UserDisplayCategories = await db.GetUserDisplayCategoriesAsync(UserId, SelectedUserLeague, SeasonPlayer.PlayerType);
                HistoryTableModel.DisplayPerValue = db.GetDefaultPerValue(SeasonPlayer.PlayerTypeId);
                HistoryTableModel.GamesCategoryId = db.GetGamesCategory(SeasonPlayer.PlayerTypeId).Id;
                HistoryTableModel.BeforeCategories = db.GetBeforeDisplayCategories(SeasonPlayer.PlayerType);
                HistoryTableModel.AfterCategories = db.GetAfterDisplayCategories(SeasonPlayer.PlayerType);
                HistoryTableModel.DisplayPlayers = new List<DisplayPlayer>();
                HistoryTableModel.IsPlayerHistory = true;
                HistoryTableModel.ShowInjuries = false;
                HistoryTableModel.PlayerHeaderTitle = "Data Set";
                HistoryTableModel.ValuePerValues = db.GetPerValues(SeasonPlayer.PlayerType.Id);
                HistoryTableModel.ShowAdp = false;

                var season = db.GetDefaultSeason();
                PositionSourcePlayers = (from p in db.GetUserLeagueSeasonPlayerPositions(SelectedUserLeague, season) where p.PlayerId == SeasonPlayer.PlayerId select p).ToList();

                DisplayPlayer fullSeasonDisplayPlayer = AddDisplayPlayer(null, season.Title, false, season, season.StartDate, season.UpdatedDate);
                if (fullSeasonDisplayPlayer != null)
                {
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

                //GameLogPerValue = (from pv in db.GetPerValues(SeasonPlayer.PlayerTypeId) where pv.CategoryId == null select pv).FirstOrDefault();
                //GameLogGames = db.GetPlayerStatPlayerGameLog(SelectedUserLeague, SeasonPlayer.Player, SeasonPlayer.PlayerType, GameLogPerValue, valueAverages, season);
                //if (pastSeason1 != null)
                //{
                //    var gl = db.GetPlayerStatPlayerGameLog(SelectedUserLeague, SeasonPlayer.Player, SeasonPlayer.PlayerType, GameLogPerValue, valueAverages, pastSeason1);
                //    if (gl.Count > 0)
                //    {
                //        GameLogGames.Add(new GameLogGame() { IsBreak = true });
                //        foreach (var lg in gl)
                //            GameLogGames.Add(lg);
                //    }
                //}
                //if (pastSeason2 != null)
                //{
                //    var gl = db.GetPlayerStatPlayerGameLog(SelectedUserLeague, SeasonPlayer.Player, SeasonPlayer.PlayerType, GameLogPerValue, valueAverages, pastSeason2);
                //    if (gl.Count > 0)
                //    {
                //        GameLogGames.Add(new GameLogGame() { IsBreak = true });
                //        foreach (var lg in gl)
                //            GameLogGames.Add(lg);
                //    }
                //}

                if (GameLogGames != null && GameLogGames.Count > 0 && GameLogGames.First().IsBreak)
                    GameLogGames.RemoveAt(0);

                return Page();
            }
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
                HistoryTableModel.DisplayPlayers.Add(dp);

                addedDisplayPlayer = dp;
            }

            return addedDisplayPlayer;
        }

    }
}
