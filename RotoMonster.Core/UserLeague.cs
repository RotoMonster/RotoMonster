using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;

namespace RotoMonster.Core
{
    public class UserLeague
    {
        public UserLeague()
        {
            UserLeagueActiveRosterSpots = new List<UserLeagueActiveRosterSpot>();
            UserLeagueCategories = new List<UserLeagueCategory>();
            UserLeagueTeams = new List<UserLeagueTeam>();
            UserLeaguePlayerTypes = new List<UserLeaguePlayerType>();
            UserLeagueImportErrors = new List<UserLeagueImportError>();
            UserLeagueWaiverPlayers = new List<UserLeagueWaiverPlayer>();
            MyProviderTeamId = "";
            MyTeamTitle = "";
            StartWeekday = Convert.ToInt32(DayOfWeek.Monday);
        }

        public int Id { get; set; }
        public int SeasonId { get; set; }

        [Required, StringLength(450)]
        public string UserId { get; set; }

        [Required, StringLength(250)]
        public string Title { get; set; }

        [Required]
        [StringLength(250)]
        [Display(Name = "Title")]
        public string DisplayTitle { get; set; }

        [Required]
        [Display(Name = "Fantasy Provider")]
        public int FantasyProviderId { get; set; }

        [StringLength(100)]
        [Display(Name = "League Id")]
        public string ProviderLeagueId { get; set; }

        [StringLength(100)]
        [Display(Name = "Your Team Id")]
        public string MyProviderTeamId { get; set; } = "";

        [Display(Name = "Track league")]
        public bool TrackLeague { get; set; }

        [StringLength(250)]
        [Display(Name = "Your Team")]
        public string MyTeamTitle { get; set; } = "";

        [Required, StringLength(10)]
        [Display(Name = "Scoring System")]
        public string ScoringSystem { get; set; }

        [Display(Name = "League Type")]
        [Required, StringLength(10)]
        public string LeagueType { get; set; }

        [Required, StringLength(10)]
        [Display(Name = "Lineup Frequency")]
        public string LineupFrequency { get; set; }

        [Display(Name = "Number of Teams")]
        public int NumberOfTeams { get; set; }

        [Display(Name = "Players per Team")]
        public int PlayersPerTeam { get; set; }

        [Display(Name = "IR Spots")]
        public int IRSpots { get; set; }

        [Display(Name = "Start Weekday")]
        public int StartWeekday { get; set; }

        [Display(Name = "Quality Games Limit")]
        public int QualityGamesLimit { get; set; }

        [Display(Name = "Same Day Transactions")]
        public bool SameDayTransactions { get; set; }

        [Display(Name = "Is Auction")]
        public bool IsAuction { get; set; }
        [Display(Name = "Money League")]
        public bool IsMoney { get; set; }
        [Display(Name = "Pro League")]
        public bool IsProLeague { get; set; }
        [Display(Name = "Dynasty")]
        public bool IsDynasty { get; set; }

        [Display(Name = "Draft Date")]
        public DateTime? DraftDate { get; set; }

        [Display(Name = "End Season when Playoffs end")]
        public bool AutoEndDate { get; set; }

        [Display(Name = "Game Limit")]
        public int GameLimit { get; set; }

        [Display(Name = "Auto Update Rosters")]
        public bool AutoUpdate { get; set; }

        [Display(Name = "Continuous Waivers")]
        public bool ContinuousWaivers { get; set; }
        public bool HasDrafted { get; set; }

        [Display(Name = "Entry Fee")]
        public int EntryFee { get; set; }

        public string WaiverType { get; set; } = "";
        public string WaiverRule { get; set; } = "";

        public bool IsDefault { get; set; }    // the default settings league

        public int DisplayOrder { get; set; }
        public DateTime? LastSelectedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public DateTime? RostersUpdatedDate { get; set; }
        public DateTime? CreatedDate { get; set; }
        public bool? DailyTransactionsAllowed { get; set; }

        public FantasyProvider FantasyProvider { get; set; }
        public Season Season { get; set; }

        [NotMapped]
        public List<UserLeagueActiveRosterSpot> UserLeagueActiveRosterSpots { get; set; }

        [NotMapped]
        public List<UserLeagueCategory> UserLeagueCategories { get; set; }

        [NotMapped]
        public List<UserLeagueTeam> UserLeagueTeams { get; set; }

        [NotMapped]
        public List<UserLeaguePlayerType> UserLeaguePlayerTypes { get; set; }

        [NotMapped]
        public List<UserLeagueImportError> UserLeagueImportErrors { get; set; }

        [NotMapped]
        public List<UserLeagueWaiverPlayer> UserLeagueWaiverPlayers { get; set; }

        [NotMapped]
        public List<PlayerStatus> PlayerStatuses { get; set; }


        [NotMapped]
        public string ListDisplayTitle
        {
            get
            {
                string o = DisplayTitle;
                if (FantasyProvider != null)
                    o += " (" + FantasyProvider.Name + ")";

                return o;
            }
        }

        public CategoriesString GetCategoriesString(PlayerType playerType)
        {
            var stringId = (from pt in UserLeaguePlayerTypes where pt.PlayerTypeId == playerType.Id select pt.CategoriesString).FirstOrDefault();

            return stringId;
        }

        public bool IsNoWaiver
        {
            get
            {
                return WaiverRule == "all";
            }
        }

        public bool IsQualifiedNoWaiver(Sport sport)
        {
            if (IsNoWaiver && HasDrafted)
            {
                if (sport.IsNBA)
                    if (Size >= 150)
                        return true;

                if (sport.IsNHL)
                    if (Size >= 150)
                        return true;

                if (sport.IsMLB)
                    return true;

            }

            return false;
        }

        public int Size
        {
            get
            {
                return NumberOfTeams * PlayersPerTeam;
            }
        }

        public int ActivePlayersPerTeam
        {
            get
            {
                int total = 0;
                foreach (var ars in UserLeagueActiveRosterSpots)
                    total += ars.NumberOfPlayers;

                return total;
            }
        }

        public int BenchPlayersPerTeam
        {
            get
            {
                if (PlayersPerTeam > ActivePlayersPerTeam)
                    return PlayersPerTeam - ActivePlayersPerTeam;
                else
                    return 0;
            }
        }

        [NotMapped]
        public string ProviderURL
        {
            get
            {
                if (FantasyProvider != null && FantasyProvider.LeagueURL != null)
                {
                    string u = FantasyProvider.LeagueURL;
                    u = u.Replace("{id}", ProviderLeagueId);
                    u = u.Replace("{teamid}", MyProviderTeamId);

                    return u;
                }
                else
                    return "";
            }
        }

        public List<CategorySetting> GetCategorySettings(PlayerType playerType)
        {
            List<CategorySetting> catSetttings = new List<CategorySetting>();
            foreach (var cs in UserLeagueCategories)
            {
                if (cs.Category.PlayerType.Id == playerType.Id)
                {
                    CategorySetting catSetting = new CategorySetting();
                    catSetting.Category = cs.Category;
                    catSetting.PointsPerStat = cs.PointsPerStat;
                    catSetting.IsActive = cs.IsActive;
                    catSetttings.Add(catSetting);
                }
            }

            return catSetttings;
        }

        public bool AddError(string message)
        {
            if (UserLeagueImportErrors.Find(e => e.Error == message) == null)
            {
                UserLeagueImportErrors.Add(new UserLeagueImportError() { Error = message });

                return true;
            }

            return false;
        }

        public void FillUserLeagueCategoriesCode(List<Category> categories)
        {
            UserLeaguePlayerTypes.Clear();

            var playerTypes = new List<PlayerType>();
            foreach (var lc in UserLeagueCategories)
            {
                var playerType = (from c in categories where c.Id == lc.CategoryId select c.PlayerType).FirstOrDefault();
                if (playerTypes.Find(pt => pt.Id == playerType.Id) == null)
                    playerTypes.Add(playerType);
            }

            foreach (var pt in playerTypes)
            {
                var ulpt = new UserLeaguePlayerType();
                ulpt.PlayerTypeId = pt.Id;
                ulpt.CategoriesCode1 = "";
                foreach (var cs in (from cs2 in UserLeagueCategories orderby cs2.CategoryId select cs2))
                {
                    var playerType = (from c in categories where c.Id == cs.CategoryId select c.PlayerType).FirstOrDefault();
                    if (playerType.Id == pt.Id)
                    {
                        ulpt.CategoriesCode1 += ":" + cs.CategoryId.ToString();
                        if (cs.PointsPerStat != 0)
                            ulpt.CategoriesCode1 += "p" + cs.PointsPerStat.ToString();
                    }
                }
                if (ulpt.CategoriesCode1.Length > 0 && ulpt.CategoriesCode1.Substring(0, 1) == ":")
                    ulpt.CategoriesCode1 = ulpt.CategoriesCode1.Substring(1);
                UserLeaguePlayerTypes.Add(ulpt);
            }

        }

        public bool TimeToRefresh(int minutes = 60 * 12)
        {
            try
            {
                TimeSpan ts = DateTime.UtcNow - UpdatedDate.GetValueOrDefault();
                if (ts.TotalMinutes > minutes)
                    return true;
            }
            catch
            {
            }

            return false;
        }

        public void FillPlayerStatuses(List<PlayerStatus> activePlayerStatuses)
        {
            if (PlayerStatuses == null)
                PlayerStatuses = new List<PlayerStatus>();
            else
                PlayerStatuses.Clear();

            foreach (var team in UserLeagueTeams)
            {
                if (team.ProviderId == MyProviderTeamId)
                {
                    foreach (var player in team.UserLeagueTeamPlayers)
                    {
                        var playerStatus = (from ps in activePlayerStatuses where ps.PlayerId == player.PlayerId select ps).FirstOrDefault();
                        if (playerStatus != null)
                            PlayerStatuses.Add(playerStatus);
                    }
                }
            }

            PlayerStatuses = (from ps in PlayerStatuses
                              orderby ps.DateAdded descending, ps.Player.LastName ascending, ps.Player.FirstName ascending
                              select ps).ToList();
        }

        public UserLeagueTeam MyUserLeagueTeam
        {
            get
            {
                var userLeagueTeam = (from ult in UserLeagueTeams where ult.ProviderId == MyProviderTeamId select ult).FirstOrDefault();

                return userLeagueTeam;
            }
        }

        public string MyUserLeagueTeamTitle
        {
            get
            {
                if (MyUserLeagueTeam != null)
                    return MyUserLeagueTeam.Title;

                return "";
            }
        }

        public int ActiveSize(PlayerType playerType)
        {
            int cnt = 0;

            if (UserLeagueActiveRosterSpots != null)
                foreach (var userActiveRosterSpot in UserLeagueActiveRosterSpots)
                    foreach (var activeRosterSpotPosition in userActiveRosterSpot.ActiveRosterSpot.ActiveRosterSpotPositions)
                        if (activeRosterSpotPosition.Position.PlayerType.Id == playerType.Id)
                        {
                            cnt += userActiveRosterSpot.NumberOfPlayers;
                            break;
                        }

            return cnt * NumberOfTeams;
        }

        public UserLeagueTeam OwningUserLeagueTeam(int playerId)
        {
            foreach (var ult in UserLeagueTeams)
            {
                var userLeagueTeamPlayer = (from ultp in ult.UserLeagueTeamPlayers where ultp.PlayerId == playerId select ultp).FirstOrDefault();
                if (userLeagueTeamPlayer != null)
                    return ult;
            }

            return null;
        }

        public List<GameUserLeagueTeamPlayer> GetGameUserLeagueTeamPlayers(List<Game> games, List<SeasonPlayer> seasonPlayers)
        {
            var teamPlayers = new List<GameUserLeagueTeamPlayer>();

            if (MyUserLeagueTeam != null)
            {
                foreach (var tp in MyUserLeagueTeam.UserLeagueTeamPlayers)
                {
                    var seasonPlayer = (from sp in seasonPlayers where sp.PlayerId == tp.PlayerId select sp).FirstOrDefault();
                    if (seasonPlayer != null)
                    {
                        foreach (var g in games)
                        {
                            if (g.IncludesTeam(seasonPlayer.TeamId))
                            {
                                var newTp = new GameUserLeagueTeamPlayer();
                                newTp.Game = g;
                                newTp.UserLeagueTeamPlayer = tp;
                                teamPlayers.Add(newTp);
                                break;
                            }
                        }
                    }
                }
            }

            return teamPlayers;
        }

        [NotMapped]
        public List<SelectListItem> UserLeagueTeamsSelectList
        {
            get
            {
                var list = new List<SelectListItem>();
                foreach (var team in UserLeagueTeams)
                {
                    var select = new SelectListItem();
                    select.Value = team.ProviderId;
                    select.Text = team.Title;
                    list.Add(select);
                }

                return list;
            }
        }


    }
}
