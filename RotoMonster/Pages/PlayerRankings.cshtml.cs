using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NuGet.Packaging.Signing;
using RotoMonster.Core;
using RotoMonster.Core.Libs;
using RotoMonster.Core.PartialViewModels;
using RotoMonster.Data;
using RotoMonster.NFL.Migrations.Migrations;
using RotoMonster.Pages.Shared;

namespace RotoMonster.Pages
{
    public class PositionFilter
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool Selected { get; set; }
        public Position Position { get; set; }
    }

    [IgnoreAntiforgeryToken]
    public class PlayerRankingsModel : RMPageModel
    {
        public PlayerTableModel PlayerTableModel = new PlayerTableModel();

        public List<PlayerType> ShowPlayerTypes { get; set; }
        public List<DisplayActiveRosterSpot> DisplayActiveRosterSpots { get; set; }

        [BindProperty] public int SelectedUserLeagueId { get; set; }
        [BindProperty] public int SelectedPerValueId { get; set; }
        [BindProperty] public int SelectedTeamId { get; set; }
        [BindProperty] public DateTime SelectedStartDate { get; set; }
        [BindProperty] public DateTime SelectedEndDate { get; set; }
        [BindProperty] public long SelectedFilterId { get; set; }
        [BindProperty] public int SelectedProjectionSourceId { get; set; }
        [BindProperty] public PositionFilter[] PositionFilters { get; set; }

        [BindProperty]
        [Display(Name = "Show League Analysis")]
        public bool ShowLeagueAnalysis { get; set; } = false;

        //[BindProperty]
        //[Display(Name = "MonsterBar Game")]
        //public bool ShowMonsterBarGame { get; set; }

        //[BindProperty]
        //[Display(Name = "MonsterBar Total")]
        //public bool ShowMonsterBarTotal { get; set; }

        [BindProperty]
        [Display(Name = "Color Stats")]
        public bool ColorStats { get; set; }

        [BindProperty]
        [Display(Name = "Show Draft")]
        public bool ShowDraft { get; set; }

        [BindProperty]
        [Display(Name = "Include Live")]
        public bool IncludeLive { get; set; }

        [BindProperty]
        public string SelectedDisplayActiveRosterSpotId { get; set; }

        [BindProperty]
        [Display(Name = "Draft ID")]
        public string ProviderDraftId { get; set; } = "";

        [BindProperty]
        [Display(Name = "Hide Drafted")]
        public bool HideDrafted { get; set; } = true;

        [BindProperty]
        public string SelectedDraftTeamId { get; set; }

        [BindProperty]
        public string DefaultSort { get; set; } = "";

        [BindProperty]
        public DateSelect[] DateSelects { get; set; }

        [BindProperty]
        public DateSelect[] StreamSelects { get; set; } = null;

        [BindProperty][Display(Name = "Analysis Start")] public DateTime AnalysisStartDate { get; set; }
        [BindProperty][Display(Name = "Analysis End")] public DateTime AnalysisEndDate { get; set; }

        [BindProperty][Display(Name = "# of Recent ADP Leagues")] public int AnalysisAdpLeagueCount { get; set; } = 50;

        [Display(Name = "Players Drafted")]
        public int DraftedCount { get; set; } = 0;
        public string UpcomingDraftText { get; set; } = "";
        public int AdpLeagueCount { get; set; } = 0;
        public int DefaultAdpLeagueCount { get; set; } = 0;
        public int OwnershipLeagueCount { get; set; } = 0;
        public string PicksString { get; set; } = "";
        public List<int> PickList { get; set; } = new List<int>();
        public int NextPick { get; set; } = 0;
        public string ButtonMessage { get; set; } = "";
        public List<string> ExtraDisplayColumns = new List<string>();
        public List<ValuePlayer> ExtraValuePlayers = new List<ValuePlayer>();
        public PositionBoostHashModel PositionBoostHashModel { get; set; } = null;
        public List<UserLeagueTeamAnalysis> UserLeagueTeamAnalyses { get; set; } = null;
        public List<MonsterBar> TeamMonsterBars { get; set; } = null;

        public bool ShowMonsterBarGame { get; set; } = false;
        public bool ShowMonsterBarTotal { get; set; } = false;

        public PlayerRankingsModel(IConfiguration config, IRMData db, IRMSharedData sharedDb, UserManager<ApplicationUser> userManager, IHttpContextAccessor contextAccessor, ILogger<PageModel> logger)
            : base(config, db, sharedDb, userManager, contextAccessor, logger)
        {
        }

        public async Task<IActionResult> OnPostAsync(string sort,
            string draftmodebutton,
            string livebutton,
            string myteambutton,
            string seasonbutton,
            string onedaybutton,
            string oneweekbutton,
            string twoweeksbutton,
            string onemonthbutton,
            string freeagentsbutton,
            string waiversbutton,
            string rostersbutton,
            string analysis1week,
            string analysisnextweek,
            string analysis2weeks,
            string depthbutton)
        {
            string b1 = "";

            if (!string.IsNullOrEmpty(rostersbutton) && SelectedUserLeagueId > 0)
                RefreshRosters(await db.GetUserLeagueAsync(SelectedUserLeagueId));

            if (!string.IsNullOrEmpty(freeagentsbutton))
            {
                sort = "OWN";
                SelectedFilterId = 5;
            }
            else if (!string.IsNullOrEmpty(waiversbutton))
            {
                sort = "TREND";
                SelectedFilterId = 3;
            }
            else if (!string.IsNullOrEmpty(myteambutton))
            {
                sort = "OWN";
                SelectedFilterId = 4;
            }
            else if (!string.IsNullOrEmpty(livebutton))
            {
                if (db.Sport.IsNFL)
                {
                    SelectedStartDate = db.GetActivePeriodStartDate(db.GetDefaultSeason());
                    SelectedEndDate = SelectedStartDate.AddDays(6);
                }
                else
                {
                    SelectedStartDate = db.GetCurrentGameDate(db.GetDefaultSeason());
                    SelectedEndDate = SelectedStartDate;
                }
                IncludeLive = true;
            }
            else if (!string.IsNullOrEmpty(draftmodebutton))
            {
                b1 = "draftmode";
                sort = "ADP";
                ShowDraft = true;
                HideDrafted = true;
                SelectedFilterId = 1;
                ProviderDraftId = (await db.GetUserLeagueAsync(SelectedUserLeagueId)).ProviderLeagueId;
            }
            else if (!string.IsNullOrEmpty(seasonbutton))
            {
                SelectedStartDate = db.GetDefaultSeason().StartDate;
                SelectedEndDate = db.GetDefaultSeason().UpdatedDate;
            }
            else if (!string.IsNullOrEmpty(onedaybutton))
            {
                SelectedStartDate = db.GetDefaultSeason().UpdatedDate;
                SelectedEndDate = SelectedStartDate;
            }
            else if (!string.IsNullOrEmpty(oneweekbutton))
            {
                SelectedEndDate = db.GetDefaultSeason().UpdatedDate;
                SelectedStartDate = SelectedEndDate.AddDays(-6);
            }
            else if (!string.IsNullOrEmpty(twoweeksbutton))
            {
                SelectedEndDate = db.GetDefaultSeason().UpdatedDate;
                SelectedStartDate = SelectedEndDate.AddDays(-13);
            }
            else if (!string.IsNullOrEmpty(onemonthbutton))
            {
                SelectedEndDate = db.GetDefaultSeason().UpdatedDate;
                SelectedStartDate = SelectedEndDate.AddMonths(-1);
            }
            else if (!string.IsNullOrEmpty(depthbutton))
            {
                b1 = "depthmode";
                DefaultSort = "DEPTH";
                SelectedFilterId = 2;
            }
            else if (!string.IsNullOrEmpty(analysis1week))
            {
                AnalysisStartDate = GetToday();
                AnalysisEndDate = db.GetPeriod(db.GetDefaultSeason(), 0).AddDays(6);
            }
            else if (!string.IsNullOrEmpty(analysisnextweek))
            {
                AnalysisStartDate = db.GetPeriod(db.GetDefaultSeason()).AddDays(7);
                AnalysisEndDate = AnalysisStartDate.AddDays(6);
            }
            else if (!string.IsNullOrEmpty(analysis2weeks))
            {
                AnalysisStartDate = GetToday();
                AnalysisEndDate = db.GetPeriod(db.GetDefaultSeason(), 0).AddDays(13);
            }

            string ds = "";
            for (int i = 0; i <= 13; i++)
            {
                if (DateSelects.Length > i && DateSelects[i].Selected)
                    ds += i.ToString() + "_";
            }
            if (ds.Length > 0)
                ds = ds.Substring(0, ds.Length - 1);

            string ss = "";
            for (int i = 0; i <= 13; i++)
            {
                if (StreamSelects.Length > i && StreamSelects[i].Selected)
                    ss += i.ToString() + "_";
            }
            if (ss.Length > 0)
                ss = ss.Substring(0, ss.Length - 1);

            if (sort != null)
                DefaultSort = sort;

            return RedirectToPage("./PlayerRankings", new
            {
                l = SelectedUserLeagueId,
                p = SelectedPerValueId,
                t = SelectedTeamId,
                f = SelectedFilterId,
                sd = SelectedStartDate,
                ed = SelectedEndDate,
                asd = AnalysisStartDate,
                aed = AnalysisEndDate,
                s = DefaultSort,
                cs = ColorStats,
                live = IncludeLive,
                dr = ShowDraft,
                ars = SelectedDisplayActiveRosterSpotId,
                did = ProviderDraftId,
                hd = HideDrafted,
                b = b1,
                ds = ds,
                ss = ss,
                ps = SelectedProjectionSourceId,
                dt = SelectedDraftTeamId,
                la = ShowLeagueAnalysis,
                adps = AnalysisAdpLeagueCount
            });
        }

        public async Task<IActionResult> OnGetAsync(
            string testuser,
            int? l,
            int? p,
            int? t,
            long? f,
            int? st,
            DateTime? sd,
            DateTime? ed,
            DateTime? asd,
            DateTime? aed,
            string s,
            bool? sv,
            bool? cs,
            bool? wk1,
            bool? wk2,
            bool? live,
            bool? dr,
            string ars,
            string did,
            bool? hd,
            string dt,
            string ds,
            string ss,
            string b,
            int? ps,
            bool? la,
            bool? mbg,
            bool? mbt,
            int? hid,
            int? adps)
        {
            InitGet("Player Rankings", testuser);

            //if (UserId != null)
            //    logger.LogWarning("Player Rankings: " + userManager.GetUserName(contextAccessor.HttpContext.User));

            if (hid.GetValueOrDefault(0) > 0)
                ViewData["Helper"] = db.GetHelper(hid.GetValueOrDefault(0));

            if (UserId != null)
            {
                SelectedUserLeagueId = l.GetValueOrDefault();
                ViewData["UserLeagueList"] = new SelectList(await db.GetTrackedUserLeaguesAsync(UserId), "Id", "ListDisplayTitle");
            }
            UserLeague userLeague = await db.SelectUserLeagueAsync(UserId, await db.GetUserLeagueAsync(UserId, SelectedUserLeagueId));
            if (userLeague == null)
                userLeague = await db.GetDefaultUserLeagueAsync();

            if (false)
            {
                if (userLeague != null && UserId != null)
                {
                    if (userLeague.SeasonId == db.GetDefaultSeason().Id)
                    {
                        SelectedUserLeagueId = userLeague.Id;
                        if (userLeague.TimeToRefresh())
                            userLeague = RefreshRosters(userLeague);
                    }
                }
            }

            if (!IsLoggedIn)
                userLeague = await db.GetDefaultUserLeagueAsync();

            PlayerTableModel.UserDisplayColumns = await db.GetUserDisplayColumns(UserId);
            PlayerTableModel.FillUserDefaultShows();

            var fantasyProvider = (userLeague != null ? userLeague.FantasyProvider : db.GetDefaultFantasyProvider());
            var positionSource = db.GetPositionSource(fantasyProvider);
            var showActiveRosterSpots = (userLeague != null ? userLeague.UserLeagueActiveRosterSpots : await db.GetDefaultUserLeagueActiveRosterSpots());
            DisplayActiveRosterSpots = db.GetDisplayActiveRosterSpots(showActiveRosterSpots, db.GetPositionSourcePositions(positionSource));
            if (ars == null)
            {
                ars = (from rs in DisplayActiveRosterSpots where rs.IsDefault select rs.Id).FirstOrDefault();
                if (ars == null)
                    ars = DisplayActiveRosterSpots[0].Id;
            }
            SelectedDisplayActiveRosterSpotId = ars;
            var selectedDisplayActiveRosterSpot = (from rs in DisplayActiveRosterSpots where rs.Id == ars select rs).FirstOrDefault();
            if (selectedDisplayActiveRosterSpot == null && DisplayActiveRosterSpots.Count > 0)
            {
                selectedDisplayActiveRosterSpot = DisplayActiveRosterSpots.First();
                SelectedDisplayActiveRosterSpotId = selectedDisplayActiveRosterSpot.Id;
            }

            ColorLib colorLib = new ColorLib();
            ValuePlayerLib lib = new ValuePlayerLib();
            SelectedTeamId = (t != null ? t.GetValueOrDefault() : -1);
            var season = db.GetDefaultSeason();
            if (!season.HasStarted)
                season = db.GetPreviousSeason(season.Year.GetValueOrDefault() - 1);

            var displaySeason = db.GetDefaultSeason();

            var selectedPlayerType = (selectedDisplayActiveRosterSpot != null ? selectedDisplayActiveRosterSpot.PlayerType : db.GetDefaultPlayerType());
            ShowPlayerTypes = (from pt1 in db.GetPlayerTypes() where !pt1.IsDisabled select pt1).ToList();
            PlayerTableModel.AllPositions = await db.GetActualPositionsAsync(selectedPlayerType);

            var seasonPlayers = db.GetSeasonPlayers(displaySeason, selectedPlayerType);

            ViewData["PerValues"] = new SelectList(db.GetPerValuesSelectItems(selectedPlayerType), "Value", "Text");
            ViewData["TeamList"] = new SelectList(db.GetTeamsSelectItems(displaySeason), "Value", "Text");
            ViewData["FilterList"] = new SelectList(await db.GetPlayerFilterSelectItems(userLeague), "Value", "Text");
            ViewData["ProjectionSourceList"] = new SelectList(db.GetProjectionSourceSelectItems(), "Value", "Text");

            ColorStats = cs.GetValueOrDefault(true);
            ShowDraft = dr.GetValueOrDefault(false);
            IncludeLive = live.GetValueOrDefault(false);
            ShowLeagueAnalysis = la.GetValueOrDefault(false);
            SelectedProjectionSourceId = ps.GetValueOrDefault(0);

            List<CategorySetting> catSetttings = db.GetUserLeagueCategorySettings(userLeague, selectedPlayerType);

            PerValue perValue = null;
            if (p > 0)
                perValue = (from pv in db.GetPerValues(selectedPlayerType.Id) where pv.Id == p select pv).FirstOrDefault();

            if (perValue == null)
                perValue = (from pv in db.GetPerValues(selectedPlayerType.Id) where pv.IsDefaultDisplay != null && (bool)pv.IsDefaultDisplay select pv).FirstOrDefault();
            SelectedPerValueId = (perValue == null ? -1 : perValue.Id);

            if (!sd.HasValue || !ed.HasValue)
            {
                SelectedStartDate = season.StartDate;
                SelectedEndDate = season.UpdatedDate;
            }
            else
            {
                SelectedStartDate = sd.GetValueOrDefault();
                SelectedEndDate = ed.GetValueOrDefault();
            }

            var today = GetToday();
            var yesterday = db.GetStartedGameDate(displaySeason);

            string scoringSystem = db.GetUserLeagueScoringSystem(userLeague);
            int leagueSize = db.GetUserLeagueLeagueSize(userLeague, selectedPlayerType);

            List<ValuePlayer> teamEaseValuePlayers = db.GetTeamEaseValuePlayers(selectedPlayerType, season, SelectedStartDate, SelectedEndDate, catSetttings, scoringSystem);

            //DateTime filterStartDate = new DateTime();
            //DateTime filterEndDate = new DateTime();
            //List<DateTime> selectedDates = null;
            //List<DateTime> streamDates = null;
            //List<PlayerGameDate> playerGameDates = null;
            //if (PlayerTableModel.ShowCurrentWeekGame || PlayerTableModel.ShowNextWeekGames)
            //{
            //    filterStartDate = today;
            //    filterEndDate = db.GetPeriod(season).AddDays(6);
            //    if (PlayerTableModel.ShowCurrentWeekGame && !PlayerTableModel.ShowNextWeekGames)
            //    {
            //        filterEndDate = db.GetPeriod(season).AddDays(6);
            //    }
            //    else if (PlayerTableModel.ShowCurrentWeekGame && PlayerTableModel.ShowNextWeekGames)
            //    {
            //        filterEndDate = filterEndDate.AddDays(7);
            //    }
            //    else if (PlayerTableModel.ShowNextWeekGames)
            //    {
            //        filterStartDate = db.GetPeriod(season).AddDays(7);
            //        filterEndDate = filterStartDate.AddDays(6);
            //    }

            //    DateSelects = GetDateSelectDays(filterStartDate, filterEndDate, ds);
            //    selectedDates = GetSelectedDates(filterStartDate, DateSelects);

            //    if (selectedPlayerType.IsStreamable)
            //    {
            //        StreamSelects = GetDateSelectDays(filterStartDate, filterEndDate, ss);
            //        streamDates = GetSelectedDates(filterStartDate, StreamSelects);
            //    }

            //    PlayerTableModel.PlayerGameStates = db.GetPlayerGameStates(filterStartDate, filterEndDate);
            //    playerGameDates = db.GetPlayerGameDates(filterStartDate, filterEndDate, teamEaseValuePlayers);
            //}

            ValueAverages outValueAverages;
            var valuePlayersHash = new Dictionary<int, List<ValuePlayer>>();
            List<ValuePlayer> valuePlayers = null;
            foreach (var pv in db.GetPerValues(selectedPlayerType.Id))
            {
                var vps = db.GetValuePlayers(selectedPlayerType, season, SelectedStartDate, SelectedEndDate, 0, catSetttings, scoringSystem, pv, leagueSize, !IncludeLive, out outValueAverages);
                valuePlayersHash[pv.Id] = vps;
                if (pv.IsDefault.GetValueOrDefault())
                    valuePlayers = vps;
            }

            var defCode = db.GetDefaultCategoriesString(selectedPlayerType).Code;
            string leagueCategoriesCode = userLeague != null ? await db.GetUserLeagueCategoryCodeAsync(userLeague, selectedPlayerType) : defCode;

            List<OwnershipPlayer> ownershipPlayers = db.GetOwnershipPlayersWithChange(leagueCategoriesCode, DateTime.UtcNow, 24);

            var playerDefaultPositions = db.GetUserLeagueSeasonPlayerPositions(userLeague, displaySeason);
            var positionSourcePositions = (from pp in db.GetPositionSourcePositions(db.GetPositionSource(userLeague.FantasyProvider)) where pp.PlayerType.Id == selectedPlayerType.Id select pp).ToList();
            GetPositionValuePlayersResult getPositionValuePlayersResult = null;
            if (PlayerTableModel.UserDisplayColumns.IsSelected("PositionValue"))
            {
                getPositionValuePlayersResult = db.GetPositionValuePlayers(selectedPlayerType,
                    valuePlayersHash[db.GetPerGamePerValue(selectedPlayerType.Id).Id],
                    userLeague,
                    positionSourcePositions,
                    playerDefaultPositions,
                    ownershipPlayers);
                if (getPositionValuePlayersResult != null)
                {
                    PositionBoostHashModel = new PositionBoostHashModel();
                    PositionBoostHashModel.PositionBoostHash = getPositionValuePlayersResult.PositionBoostHash;
                    PositionBoostHashModel.ActiveRosterSpotBoostHash = getPositionValuePlayersResult.ActiveRosterSpotBoostHash;
                    PositionBoostHashModel.Positions = db.GetPositionSourcePositions(db.GetPositionSource(userLeague.FantasyProvider));
                    PositionBoostHashModel.ActiveRosterSpots = db.GetActiveRosterSpots();
                }
            }

            ShowMonsterBarGame = PlayerTableModel.UserDisplayColumns.IsSelected("Monster Bar (Game Value)");
            ShowMonsterBarTotal = PlayerTableModel.UserDisplayColumns.IsSelected("Monster Bar (Total Value)");

            if (ShowMonsterBarGame)
                PlayerTableModel.MonsterBarGame = db.GetMonsterBar(selectedPlayerType, season, catSetttings, scoringSystem, db.GetPerGamePerValue(selectedPlayerType.Id), leagueSize, userLeague.ActiveSize(selectedPlayerType));
            if (ShowMonsterBarTotal)
                PlayerTableModel.MonsterBarTotal = db.GetMonsterBar(selectedPlayerType, season, catSetttings, scoringSystem, db.GetTotalPerValue(selectedPlayerType.Id), leagueSize, userLeague.ActiveSize(selectedPlayerType));

            List<Game> games = null;
            List<Game> nextGames = null;
            List<ValuePlayer> extraValuePlayers1 = null;
            List<ValuePlayer> extraValuePlayers2 = null;
            List<ValuePlayer> extraValuePlayers3 = null;
            var perGamePerValue = db.GetPerGamePerValue(selectedPlayerType.Id);

            if (db.Sport.IsNFL)
            {
                PlayerTableModel.ExtraColumnTitle1 = "Curr"; // current week game
                PlayerTableModel.ExtraColumnDescription1 = "Their value in the current week";
                PlayerTableModel.ExtraColumnTitle2 = "Prev"; // previous week game
                PlayerTableModel.ExtraColumnDescription2 = "Their value in the previous week";

                var currentPeriodStart = db.GetActivePeriodStartDate(displaySeason, 0);
                extraValuePlayers1 = db.GetValuePlayers(selectedPlayerType, season, currentPeriodStart, currentPeriodStart.AddDays(6), 0, catSetttings, scoringSystem, perGamePerValue, leagueSize, true, out outValueAverages);
                var previousPeriodStart = currentPeriodStart.AddDays(-7);
                extraValuePlayers2 = db.GetValuePlayers(selectedPlayerType, season, previousPeriodStart, previousPeriodStart.AddDays(6), 0, catSetttings, scoringSystem, perGamePerValue, leagueSize, true, out outValueAverages);

                var nflGameDate = db.GetPeriod(displaySeason, 0);
                games = db.GetGames(displaySeason, nflGameDate, nflGameDate.AddDays(6));
                nextGames = db.GetGames(displaySeason, nflGameDate.AddDays(7), nflGameDate.AddDays(6 + 7));
            }

            else if (db.Sport.IsNHL || db.Sport.IsNBA || db.Sport.IsMLB)
            {
                PlayerTableModel.NextColumnTitle = today.AddDays(1).DayOfWeek.ToString().Substring(0, 3);
                if (displaySeason.HasStarted)
                {
                    if (PlayerTableModel.UserDisplayColumns.IsSelected("2 Week Value"))
                    {
                        PlayerTableModel.ExtraColumnTitle1 = "2W";
                        extraValuePlayers1 = db.GetValuePlayers(selectedPlayerType, season, yesterday.AddDays(-14), yesterday, 0, catSetttings, scoringSystem, perGamePerValue, leagueSize, true, out outValueAverages);
                    }
                    if (PlayerTableModel.UserDisplayColumns.IsSelected("1 Week Value"))
                    {
                        PlayerTableModel.ExtraColumnTitle2 = "1W"; // 1 week
                        extraValuePlayers2 = db.GetValuePlayers(selectedPlayerType, season, yesterday.AddDays(-7), yesterday, 0, catSetttings, scoringSystem, perGamePerValue, leagueSize, true, out outValueAverages);
                    }
                    if (PlayerTableModel.UserDisplayColumns.IsSelected("Past Day Value"))
                    {
                        PlayerTableModel.ExtraColumnTitle3 = yesterday.DayOfWeek.ToString().Substring(0, 3); // yesterday
                        extraValuePlayers3 = db.GetValuePlayers(selectedPlayerType, season, yesterday, yesterday, 0, catSetttings, scoringSystem, perGamePerValue, leagueSize, true, out outValueAverages);
                    }
                }

                games = db.GetGames(displaySeason, db.GetCurrentGameDate(displaySeason), db.GetCurrentGameDate(displaySeason));
                nextGames = db.GetGames(displaySeason, db.GetCurrentGameDate(displaySeason).AddDays(1), db.GetCurrentGameDate(displaySeason).AddDays(1));

                PlayerTableModel.RemainingWeekStartDate = today;
                PlayerTableModel.RemainingWeekEndDate = db.GetPeriod(season).AddDays(6);
                PlayerTableModel.NextWeekStartDate = db.GetPeriod(season).AddDays(7);
                PlayerTableModel.NextWeekEndDate = PlayerTableModel.NextWeekStartDate.AddDays(6);

                ViewData["DayOfWeek"] = new SelectList(db.GetDayOfWeekSelectItems(today, today.AddDays(6)), "Value", "Text");
            }

            SelectedFilterId = (t == null ? 1 : f.GetValueOrDefault());

            if (ownershipPlayers != null && ownershipPlayers.Count > 0)
                OwnershipLeagueCount = (from a in ownershipPlayers select a.LeagueCount).Max();

            if (OwnershipLeagueCount < 10)
            {
                //leagueCategoriesCode = defCode;
                //ownershipPlayers = db.GetOwnershipPlayersWithChange(leagueCategoriesCode, userLeague.LineupFrequency, DateTime.UtcNow, 24);
                //OwnershipLeagueCount = (from a in ownershipPlayers select a.LeagueCount).Max();
            }

            PlayerTableModel.ShowTrending = displaySeason.HasStarted;
            List<OwnershipPlayer> trendingPlayers = PlayerTableModel.ShowTrending ? db.GetTrendingPlayers() : null;

            var waiverPlayers = await db.GetUserLeagueWaiverPlayersAsync(userLeague);

            int pastAdps = 50;
            var adpPlayers = db.GetAdpPlayers(displaySeason, leagueCategoriesCode, adps.GetValueOrDefault(0) > 0 ? adps.GetValueOrDefault(0) : pastAdps, DateTime.Today.AddDays(-90));
            if (adpPlayers.Count > 0)
                AdpLeagueCount = (from a in adpPlayers select a.DraftCount).Max();

            AnalysisAdpLeagueCount = Math.Max(AdpLeagueCount, adps.GetValueOrDefault(0));
            AnalysisAdpLeagueCount = Math.Min(AnalysisAdpLeagueCount, AdpLeagueCount);

            var allAdpPlayers = db.GetAdpPlayers(displaySeason, leagueCategoriesCode, pastAdps, DateTime.Today.AddDays(-90));
            if (allAdpPlayers.Count > 0)
                if ((from a in allAdpPlayers select a.DraftCount).Max() == AdpLeagueCount)
                    allAdpPlayers = null;

            List<AdpPlayer> defaultAdpPlayers = null;
            if (leagueCategoriesCode != defCode)
            {
                defaultAdpPlayers = db.GetAdpPlayers(displaySeason, defCode, AnalysisAdpLeagueCount, DateTime.Today.AddDays(-30));
                if (defaultAdpPlayers.Count > 0)
                    DefaultAdpLeagueCount = (from a in defaultAdpPlayers select a.DraftCount).Max();
            }

            var injuries = await db.GetPlayerInjuriesAsync();
            var playerStatuses = await db.GetActivePlayerStatusesAsync();

            var depthPlayers = db.GetDepthPlayers(selectedPlayerType, leagueCategoriesCode, DateTime.UtcNow, false);

            Draft draft = null;
            ProviderDraftId = did;

            HideDrafted = hd.GetValueOrDefault(true);
            if (ProviderDraftId != null && ProviderDraftId.Length > 0)
            {
                if (await db.IsDraftFinished(userLeague.FantasyProvider, ProviderDraftId))
                {
                    draft = await db.GetDraft(userLeague.FantasyProvider, ProviderDraftId);
                }
                else
                {
                    int myDraftOrder = 0;

                    if (userLeague.FantasyProvider.Id == 1)
                    {
                        try
                        {
                            draft = sharedDb.ImportDraft(
                                RefreshYahoo(sharedDb.GetUserAuth(UserId)),
                                displaySeason,
                                ProviderDraftId,
                                db.GetFantasyProviderPlayers(userLeague.FantasyProvider),
                                logger);
                            if (draft == null)
                                throw new Exception();

                            if (draft.IsFinished)
                            {
                                draft.FantasyProviderId = fantasyProvider.Id;
                                draft.SeasonId = displaySeason.Id;
                                draft.ImportUserLeague(userLeague);
                                await db.AddDraftAsync(draft);
                            }
                            List<UserLeagueTeam> pickUserLeagueTeams = null;

                            XmlDocument xml = null;
                            string teamXmlFilename = "C:\\rm_debug\\yteams_" + UserId + "_" + ProviderDraftId + ".xml";
                            if (draft.IsLive && System.IO.File.Exists(teamXmlFilename))
                            {
                                xml = new XmlDocument();
                                try
                                {
                                    xml.Load(teamXmlFilename);
                                }
                                catch
                                {
                                    xml = null;
                                }
                            }

                            var tmpUserLeague = new UserLeague();
                            tmpUserLeague.Season = displaySeason;
                            tmpUserLeague.ProviderLeagueId = ProviderDraftId;
                            tmpUserLeague.DraftDate = userLeague.DraftDate;
                            var missingPlayers = new List<UserLeagueMissingPlayer>();
                            pickUserLeagueTeams = sharedDb.GetUserLeagueTeams(
                                sharedDb.GetUserAuth(UserId),
                                db.GetDefaultSeason().YahooId,
                                tmpUserLeague,
                                db.GetFantasyProviderPlayers(userLeague.FantasyProvider),
                                missingPlayers,
                                logger,
                                xml);

                            var draftTeamsList = new List<SelectListItem>();
                            foreach (var pickUserLeagueTeam in pickUserLeagueTeams)
                                pickUserLeagueTeam.DraftOrder = (from dteam in draft.DraftUserLeagueTeams where dteam.ProviderId == pickUserLeagueTeam.ProviderId select dteam.DraftOrder).FirstOrDefault();
                            pickUserLeagueTeams = YahooLib.SortDraftOrderTeams(pickUserLeagueTeams);
                            foreach (var pickUserLeagueTeam in pickUserLeagueTeams)
                            {
                                string teamTitle = (pickUserLeagueTeam.DraftOrder > 0 ? pickUserLeagueTeam.DraftOrder.ToString() + " " : "") + pickUserLeagueTeam.Title;
                                draftTeamsList.Add(new SelectListItem(teamTitle, pickUserLeagueTeam.ProviderId));
                            }

                            ViewData["DraftTeamList"] = new SelectList(draftTeamsList, "Value", "Text");
                            if (dt == null)
                            {
                                SelectedDraftTeamId = tmpUserLeague.MyProviderTeamId;
                            }
                            else
                                SelectedDraftTeamId = dt;
                            DraftedCount = draft.DraftPlayers.Count();
                            myDraftOrder = (from t1 in pickUserLeagueTeams where t1.ProviderId == SelectedDraftTeamId select t1).FirstOrDefault().DraftOrder;
                        }
                        catch (Exception ex)
                        {
                            draft = null;
                        }
                    }

                    if (userLeague.FantasyProviderId == 4)
                    {
                        try
                        {
                            var tmpUserLeague = new UserLeague();
                            tmpUserLeague.FantasyProviderId = fantasyProvider.Id;
                            tmpUserLeague.ProviderLeagueId = ProviderDraftId;
                            tmpUserLeague.MyProviderTeamId = userLeague.MyProviderTeamId;
                            tmpUserLeague.DraftDate = userLeague.DraftDate;

                            draft = FanTraxLib.ImportDraft(sharedDb.GetUserAuth(UserId), tmpUserLeague, db.GetFantasyProviderPlayers(userLeague.FantasyProvider));
                            List<UserLeagueTeam> pickUserLeagueTeams = null;

                            var missingPlayers = new List<UserLeagueMissingPlayer>();
                            pickUserLeagueTeams = FanTraxLib.GetUserLeagueTeams(sharedDb.GetUserAuth(UserId), db.Sport.Title.ToLower(), tmpUserLeague, db.GetFantasyProviderPlayers(userLeague.FantasyProvider), missingPlayers);

                            var draftTeamsList = new List<SelectListItem>();
                            foreach (var pickUserLeagueTeam in pickUserLeagueTeams)
                            {
                                draftTeamsList.Add(new SelectListItem(pickUserLeagueTeam.Title, pickUserLeagueTeam.ProviderId));
                            }
                            ViewData["DraftTeamList"] = new SelectList(draftTeamsList, "Value", "Text");
                            if (dt == null)
                                SelectedDraftTeamId = tmpUserLeague.MyProviderTeamId;
                            else
                                SelectedDraftTeamId = dt;
                            if (draft != null)
                                DraftedCount = draft.DraftPlayers.Count();
                        }
                        catch
                        {
                            draft = null;
                        }
                    }

                    if (myDraftOrder > 0 && draft.NumberOfTeams > 0)
                    {
                        for (int i = 1; i < Convert.ToInt32(draft.LeagueSize / draft.NumberOfTeams) + 1; i++)
                        {
                            int pick;
                            if (i % 2 == 1)
                                pick = myDraftOrder + ((i - 1) * draft.NumberOfTeams);
                            else
                                pick = (i * draft.NumberOfTeams - myDraftOrder) + 1;
                            if (pick > DraftedCount && NextPick == 0)
                                NextPick = pick;
                            PickList.Add(pick);
                        }
                    }
                }
            }

            if (userLeague != null)
            {
                if (userLeague.DraftDate != null && userLeague.DraftDate.GetValueOrDefault() > DateTime.UtcNow)
                {
                    TimeSpan timeUntil = userLeague.DraftDate.GetValueOrDefault() - DateTime.UtcNow;
                    UpcomingDraftText = "Draft in " + String.Format("{0:####0.0}", timeUntil.TotalHours) + " hours";
                }
            }

            long filterTeamId = (SelectedFilterId > 100 ? SelectedFilterId - 100 : 0);

            var allDps = new List<DisplayPlayer>();

            PlayerTableModel.ShowAnalysisDates = PlayerTableModel.UserDisplayColumns.IsSelected("Analysis Dates");
            PlayerTableModel.AnalysisStartDate = AnalysisStartDate = asd.GetValueOrDefault(today);
            PlayerTableModel.AnalysisEndDate = AnalysisEndDate = aed.GetValueOrDefault(today);
            PlayerTableModel.PlayerGameStates = await db.GetPlayerGameStatesAsync(AnalysisStartDate, AnalysisEndDate);

            List<MonsterBotPlayer> monsterBotPlayers = null;
            if (db.Sport.IsNFL || db.Sport.IsMLB || true)
            {
                if (UserId != null && season.HasStarted)
                {
                    UserLeagueTeam userLeagueTeam;
                    if (filterTeamId == 0)
                        userLeagueTeam = (from ult in userLeague.UserLeagueTeams where ult.ProviderId == userLeague.MyProviderTeamId select ult).FirstOrDefault();
                    else
                        userLeagueTeam = (from ult in userLeague.UserLeagueTeams where ult.Id == filterTeamId select ult).FirstOrDefault();
                    if (userLeagueTeam != null)
                    {
                        MonsterBotLib monsterBotLib = new MonsterBotLib();
                        monsterBotPlayers = monsterBotLib.GetMonsterBotPlayers(userLeague.UserLeagueActiveRosterSpots, userLeagueTeam.UserLeagueTeamPlayers, selectedPlayerType, ownershipPlayers, season, seasonPlayers, playerStatuses, playerDefaultPositions, PlayerTableModel.PlayerGameStates, games);
                    }
                }
            }

            List<ProjectionPlayer> projectionPlayers = null;
            if (PlayerTableModel.ShowAnalysisDates && SelectedProjectionSourceId > 0)
            {
                DateTime projSourceEndDate = today;
                DateTime projSourceStartDate = season.StartDate;
                if (SelectedProjectionSourceId == 2)
                    projSourceStartDate = projSourceEndDate.AddMonths(-1);
                else if (SelectedProjectionSourceId == 3)
                    projSourceStartDate = projSourceEndDate.AddDays(-7 * 3);
                else if (SelectedProjectionSourceId == 4)
                    projSourceStartDate = projSourceEndDate.AddDays(-7 * 2);
                else if (SelectedProjectionSourceId == 5)
                    projSourceStartDate = projSourceEndDate.AddDays(-7);
                projectionPlayers = await db.GetProjectionPlayers(selectedPlayerType, season, projSourceStartDate, projSourceEndDate, AnalysisStartDate, AnalysisEndDate, catSetttings, scoringSystem, db.GetTotalPerValue(selectedPlayerType.Id), leagueSize);
                PlayerTableModel.ShowProjections = true;
                PlayerTableModel.ShowProjectionPercents = db.Sport.IsNBA;
            }

            List<PlayerPositionPercent> playerPositionPercents = null;
            if (PlayerTableModel.UserDisplayColumns.IsSelected("Position Percents"))
            {
                PlayerTableModel.ShowPositionPercents = true;
                playerPositionPercents = db.GetPlayerPositionPercents(season, season.StartDate, season.UpdatedDate);
            }

            // team analysis
            if (IsLoggedIn)
            {
                if (userLeague != null && userLeague.UserLeagueTeams.Count() > 1)
                {
                    var allOwnershipPlayers = db.GetAllOwnershipPlayers(userLeague, DateTime.UtcNow);
                    var allSeasonPlayers = db.GetAllSeasonPlayers(displaySeason);
                    UserLeagueTeamAnalyses = new List<UserLeagueTeamAnalysis>();
                    foreach (var userLeagueTeam in userLeague.UserLeagueTeams)
                    {
                        var userLeagueTeamAnalysis = userLeagueTeam.GetUserLeagueTeamAnalysis(db.GetPlayerTypes(), allSeasonPlayers, allOwnershipPlayers, false, true);
                        UserLeagueTeamAnalyses.Add(userLeagueTeamAnalysis);
                    }

                    UserLeagueTeamAnalyses = (from ul in UserLeagueTeamAnalyses orderby ul.AverageOwnershipPercent descending select ul).ToList();
                }
            }

            foreach (var seasonPlayer in seasonPlayers)
            {
                if (IncludeLive)
                {
                    var matchGame = (from g in games where g.HasStarted && g.IncludesTeam(seasonPlayer.TeamId) select g).FirstOrDefault();
                    if (matchGame == null)
                        continue;
                }

                var dp = new DisplayPlayer();
                dp.SeasonPlayer = seasonPlayer;
                dp.ExtraValuePlayer1 = extraValuePlayers1 != null ? (from vp in extraValuePlayers1 where vp.Player.Id == seasonPlayer.PlayerId select vp).FirstOrDefault() : null;
                dp.ExtraValuePlayer2 = extraValuePlayers2 != null ? (from vp in extraValuePlayers2 where vp.Player.Id == seasonPlayer.PlayerId select vp).FirstOrDefault() : null;
                dp.ExtraValuePlayer3 = extraValuePlayers3 != null ? (from vp in extraValuePlayers3 where vp.Player.Id == seasonPlayer.PlayerId select vp).FirstOrDefault() : null;
                dp.MonsterBarGamePlayer = PlayerTableModel.MonsterBarGame != null ? (from vp in PlayerTableModel.MonsterBarGame.MonsterBarPlayers where vp.Player.Id == seasonPlayer.PlayerId select vp).FirstOrDefault() : null;
                if (getPositionValuePlayersResult != null)
                    dp.PositionValuePlayer = (getPositionValuePlayersResult != null & getPositionValuePlayersResult.PositionValuePlayers != null) ? (from vp in getPositionValuePlayersResult.PositionValuePlayers where vp.DefaultValuePlayer.Player.Id == seasonPlayer.PlayerId select vp).FirstOrDefault() : null;
                dp.MonsterBarTotalPlayer = PlayerTableModel.MonsterBarTotal != null ? (from vp in PlayerTableModel.MonsterBarTotal.MonsterBarPlayers where vp.Player.Id == seasonPlayer.PlayerId select vp).FirstOrDefault() : null;
                dp.Positions = (from p1 in playerDefaultPositions where p1.PlayerId == seasonPlayer.Player.Id select p1.Position).ToList();
                dp.OwnershipPlayer = (from p1 in ownershipPlayers where p1.PlayerId == seasonPlayer.Player.Id select p1).FirstOrDefault();
                dp.NoWaiverOwnershipPlayer = trendingPlayers == null ? null : (from p1 in trendingPlayers where p1.PlayerId == seasonPlayer.Player.Id select p1).FirstOrDefault();
                dp.AdpPlayer = (from p1 in adpPlayers where p1.PlayerId == seasonPlayer.Player.Id select p1).FirstOrDefault();
                if (allAdpPlayers != null)
                    dp.AllAdpPlayer = (from p1 in allAdpPlayers where p1.PlayerId == seasonPlayer.Player.Id select p1).FirstOrDefault();
                dp.DepthPlayer = (from p1 in depthPlayers where p1.SeasonPlayer.PlayerId == seasonPlayer.Player.Id select p1).FirstOrDefault();
                if (projectionPlayers != null)
                    dp.ProjectionPlayer = (from p1 in projectionPlayers where p1.ValuePlayer.Player.Id == seasonPlayer.Player.Id select p1).FirstOrDefault();
                dp.IsWaiver = (waiverPlayers.Find(p => p.PlayerId == seasonPlayer.PlayerId) != null);
                dp.RecentArticles = db.GetPlayerRecentArticles(seasonPlayer.Player.Id);
                dp.PlayerPositionPercents = playerPositionPercents != null ? (from pp in playerPositionPercents where pp.Player.Id == seasonPlayer.PlayerId select pp).ToList() : null;

                if (games != null)
                {
                    dp.Game = (from g in games where g.IncludesTeam(seasonPlayer.Team.Id) select g).FirstOrDefault();
                    if (dp.Game != null && PlayerTableModel.PlayerGameStates != null)
                        dp.PlayerGameState = (from gs in PlayerTableModel.PlayerGameStates where gs.GameId == dp.Game.Id && gs.PlayerId == dp.SeasonPlayer.PlayerId select gs).FirstOrDefault();
                }
                if (nextGames != null)
                    dp.NextGame = (from g in nextGames where g.IncludesTeam(seasonPlayer.Team.Id) select g).FirstOrDefault();
                if (defaultAdpPlayers != null)
                    dp.DefaultAdpPlayer = (from p1 in defaultAdpPlayers where p1.PlayerId == seasonPlayer.Player.Id select p1).FirstOrDefault();
                if (monsterBotPlayers != null)
                    dp.MonsterBotPlayer = (from mbp in monsterBotPlayers where mbp.DisplayPlayer.SeasonPlayer.PlayerId == seasonPlayer.PlayerId select mbp).FirstOrDefault();

                if (dp.DepthPlayer != null)
                    dp.HigherDepthInjuredDisplayPlayers = dp.DepthPlayer.GetHigherDepthInjuredDisplayPlayers(playerStatuses);

                if (seasonPlayer != null)
                {
                    dp.TeamGames = db.GetGames(season, seasonPlayer.Team);
                    dp.RemainingWeekGames = (from g in dp.TeamGames where g.GameDate >= PlayerTableModel.RemainingWeekStartDate && g.GameDate <= PlayerTableModel.RemainingWeekEndDate select g).ToList();
                    dp.NextWeekGames = (from g in dp.TeamGames where g.GameDate >= PlayerTableModel.NextWeekStartDate && g.GameDate <= PlayerTableModel.NextWeekEndDate select g).ToList();
                }

                dp.PlayerInjury = (from p1 in injuries where p1.PlayerId == seasonPlayer.Player.Id select p1).FirstOrDefault();
                dp.PlayerStatus = (from p1 in playerStatuses where p1.PlayerId == seasonPlayer.Player.Id select p1).FirstOrDefault();
                if (draft != null)
                    dp.DraftPlayer = (from d in draft.DraftPlayers where d.PlayerId == seasonPlayer.Player.Id select d).FirstOrDefault();

                // dp.PlayerGameDates = playerGameDates == null ? null : (from pgd in playerGameDates where pgd.SeasonPlayer.PlayerId == seasonPlayer.PlayerId select pgd).ToList();

                var activeRosterSpot = db.GetEaseActiveRosterSpot(dp.SeasonPlayer.Player.DefaultPosition);
                if (teamEaseValuePlayers != null && dp.Positions.Count > 0 && dp.Game != null)
                    dp.OpposingTeamValuePlayer = lib.GetTeamValuePlayer(teamEaseValuePlayers, dp.Game.GetOpponent(dp.SeasonPlayer.Team), activeRosterSpot);
                if (teamEaseValuePlayers != null && dp.Positions.Count > 0 && dp.NextGame != null)
                    dp.NextOpposingTeamValuePlayer = lib.GetTeamValuePlayer(teamEaseValuePlayers, dp.NextGame.GetOpponent(dp.SeasonPlayer.Team), activeRosterSpot);

                if (valuePlayers != null)
                {
                    foreach (var vp in (from v in valuePlayers where v.Player.Id == seasonPlayer.Player.Id select v))
                    {
                        dp.StatPlayer = vp.StatPlayer;
                        dp.ValuePlayer = vp;
                        foreach (var pv in db.GetPerValues(selectedPlayerType.Id))
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
                    }
                }
                allDps.Add(dp);
            }

            foreach (var displayPlayer in allDps)
            {
                displayPlayer.PlayerAnalysisGames = new List<PlayerAnalysisGame>();
                foreach (var analysisDate in PlayerTableModel.AnalysisDates)
                {
                    var analysisGames = (from g in db.GetGames(season, analysisDate, analysisDate) where g.IncludesTeam(displayPlayer.SeasonPlayer.TeamId) select g).ToList();
                    foreach (var analysisGame in analysisGames)
                    {
                        displayPlayer.TeamGameCount++;
                        var playerStatus = (from ps1 in playerStatuses where ps1.PlayerId == displayPlayer.SeasonPlayer.PlayerId && ps1.EstimatedReturnDate != null select ps1).FirstOrDefault();
                        if (playerStatus == null || playerStatus.EstimatedReturnDate <= analysisGame.GameDate)
                            displayPlayer.PlayerGameCount++;
                        var playerAnalysisGame = new PlayerAnalysisGame();
                        playerAnalysisGame.SeasonPlayer = displayPlayer.SeasonPlayer;
                        playerAnalysisGame.Game = analysisGame;
                        playerAnalysisGame.PlayerGameStates = (from gs in PlayerTableModel.PlayerGameStates where gs.PlayerId == displayPlayer.SeasonPlayer.PlayerId && gs.GameId == analysisGame.Id select gs).ToList();
                        var activeRosterSpot = db.GetEaseActiveRosterSpot(playerAnalysisGame.SeasonPlayer.Player.DefaultPosition);
                        if (teamEaseValuePlayers != null && displayPlayer.Positions.Count > 0)
                            playerAnalysisGame.OpponentEasePlayer = lib.GetTeamValuePlayer(teamEaseValuePlayers, analysisGame.GetOpponent(displayPlayer.SeasonPlayer.Team), activeRosterSpot);
                        displayPlayer.PlayerAnalysisGames.Add(playerAnalysisGame);
                    }
                }
            }

            allDps = (from dp in allDps
                      orderby (dp.ValuePlayer != null ? dp.ValuePlayer.LeagueValue : -1000 - dp.SeasonPlayer.PlayerId) descending
                      select dp).ToList();

            if (s != null)
            {
                DefaultSort = s;

                List<DisplayPlayer> newDisplayPlayers = null;

                if (s.Length > 2 && s.Substring(0, 2) == "PV")
                {
                    try
                    {
                        int sortPerValueId = Convert.ToInt32(s.Substring(3));
                        var sortPerValue = (from spv in db.GetPerValues(selectedPlayerType.Id) where spv.Id == sortPerValueId && spv.PlayerTypeId == selectedPlayerType.Id select spv).FirstOrDefault();
                        if (sortPerValue != null)
                        {
                            newDisplayPlayers = (from dp
                                                 in allDps
                                                 orderby (dp.DisplayValuePlayers.Where(a => a.PerValue.Id == sortPerValueId).FirstOrDefault() != null
                                                     ? dp.DisplayValuePlayers.Where(a => a.PerValue.Id == sortPerValueId).FirstOrDefault().ValuePlayer.LeagueValue
                                                    : int.MinValue)
                                                 descending
                                                 select dp).ToList();
                        }
                    }
                    catch { }
                }
                else if (s == "EXTRAVALUE1")
                {
                    newDisplayPlayers = (from dp in allDps
                                         orderby (dp.ExtraValuePlayer1 != null ? dp.ExtraValuePlayer1.LeagueValue : double.MinValue)
                                         descending,
                                         (dp.ValuePlayer != null ? dp.ValuePlayer.Rank : int.MinValue)
                                         descending
                                         select dp).ToList();
                }
                else if (s == "EXTRAVALUE2")
                {
                    newDisplayPlayers = (from dp in allDps
                                         orderby (dp.ExtraValuePlayer2 != null ? dp.ExtraValuePlayer2.LeagueValue : double.MinValue)
                                         descending,
                                         (dp.ValuePlayer != null ? dp.ValuePlayer.Rank : int.MinValue)
                                         descending
                                         select dp).ToList();
                }
                else if (s == "EXTRAVALUE3")
                {
                    newDisplayPlayers = (from dp in allDps
                                         orderby (dp.ExtraValuePlayer3 != null ? dp.ExtraValuePlayer3.LeagueValue : double.MinValue)
                                         descending,
                                         (dp.ValuePlayer != null ? dp.ValuePlayer.Rank : int.MinValue)
                                         descending
                                         select dp).ToList();
                }

                else if (s == "POSVALUE")
                {
                    newDisplayPlayers = (from dp in allDps
                                         orderby (dp.PositionValuePlayer != null ? dp.PositionValuePlayer.PositionValue : double.MinValue) descending,
                                         (dp.ValuePlayer != null ? dp.ValuePlayer.Rank : int.MinValue) descending
                                         select dp).ToList();
                }

                else if (s == "PROJV")
                {
                    newDisplayPlayers = (from dp in allDps
                                         orderby (dp.ProjectionPlayer != null ? dp.ProjectionPlayer.ValuePlayer.LeagueValue : double.MinValue) descending,
                                         (dp.ValuePlayer != null ? dp.ValuePlayer.Rank : int.MinValue) descending
                                         select dp).ToList();
                }
                else if (s == "PROJVINFO")
                {
                    newDisplayPlayers = (from dp in allDps
                                         orderby (dp.ProjectionPlayer != null ? dp.ProjectionPlayer.EstimatedUpside : double.MinValue) descending,
                                         (dp.ValuePlayer != null ? dp.ValuePlayer.Rank : int.MinValue) descending
                                         select dp).ToList();
                }

                else if (s == "OWN")
                {
                    newDisplayPlayers = (from dp in allDps
                                         orderby (dp.OwnershipPlayer != null ? dp.OwnershipPlayer.OwnershipPercent : 0)
                                         descending,
                                         (dp.OwnershipPlayer != null ? dp.OwnershipPlayer.ActivePercent : 0)
                                         descending
                                         select dp).ToList();
                }
                else if (s == "ACTIVE")
                {
                    newDisplayPlayers = (from dp in allDps
                                         orderby (dp.OwnershipPlayer != null ? dp.OwnershipPlayer.ActivePercent : 0) descending,
                                         (dp.OwnershipPlayer != null ? dp.OwnershipPlayer.OwnershipPercent : 0)
                                         descending
                                         select dp).ToList();
                }
                else if (s == "OWNCHANGE")
                {
                    newDisplayPlayers = (from dp in allDps
                                         orderby (dp.OwnershipPlayer != null ? dp.OwnershipPlayer.PercentOwnershipChange : 0) descending
                                         select dp).ToList();
                }
                else if (s == "TREND")
                {
                    newDisplayPlayers = (from dp in allDps
                                         orderby (dp.NoWaiverOwnershipPlayer != null ?
                                         (Math.Round(dp.NoWaiverOwnershipPlayer.PercentOwnershipChange, 0) == 0 ? int.MinValue : dp.NoWaiverOwnershipPlayer.PercentOwnershipChange)
                                         : int.MinValue) descending,
                                         (dp.OwnershipPlayer != null ? dp.OwnershipPlayer.PercentOwnershipChange : 0) descending
                                         select dp).ToList();
                }
                else if (s == "ACTIVECHANGE")
                {
                    newDisplayPlayers = (from dp in allDps
                                         orderby (dp.OwnershipPlayer != null ? dp.OwnershipPlayer.PercentActiveChange : 0)
                                         descending
                                         select dp).ToList();
                }
                else if (s == "ADP")
                {
                    newDisplayPlayers = (from dp in allDps
                                         orderby (dp.AdpPlayer != null && dp.AdpPlayer.Adp != 0 ? dp.AdpPlayer.Adp
                                            : (dp.ValuePlayer != null ? 1000 + dp.ValuePlayer.Rank : 1000 + dp.SeasonPlayer.Player.Id))
                                         ascending
                                         select dp).ToList();
                }
                else if (s == "ALLADP")
                {
                    newDisplayPlayers = (from dp in allDps
                                         orderby (dp.AllAdpPlayer != null && dp.AllAdpPlayer.Adp != 0 ? dp.AllAdpPlayer.Adp
                                            : (dp.ValuePlayer != null ? 1000 + dp.ValuePlayer.Rank : 1000 + dp.SeasonPlayer.Player.Id))
                                         ascending
                                         select dp).ToList();
                }
                else if (s == "DADP")
                {
                    newDisplayPlayers = (from dp in allDps
                                         orderby (dp.DefaultAdpPlayer != null && dp.DefaultAdpPlayer.Adp != 0 ? dp.DefaultAdpPlayer.Adp
                                            : (dp.ValuePlayer != null ? 1000 + dp.ValuePlayer.Rank : 1000 + dp.SeasonPlayer.Player.Id))
                                         ascending
                                         select dp).ToList();
                }
                else if (s == "DEPTH")
                {
                    newDisplayPlayers = (from dp in allDps
                                         orderby dp.SeasonPlayer.Team.Code ascending,
                                         (dp.DepthPlayer != null && dp.DepthPlayer.Position != null ? dp.DepthPlayer.Position.DisplayOrder : 1000) ascending,
                                         (dp.DepthPlayer != null && dp.DepthPlayer.Depth > 0 ? dp.DepthPlayer.Depth : 1000) ascending
                                         select dp).ToList();
                }
                else if (s == "INJ")
                {
                    newDisplayPlayers = (from dp in allDps
                                         orderby dp.PlayerInjury != null ? dp.PlayerInjury.UpdateDate : DateTime.MinValue descending
                                         select dp).ToList();
                }
                else if (s == "PS")
                {
                    newDisplayPlayers = (from dp in allDps
                                         orderby dp.PlayerStatus != null ? dp.PlayerStatus.DateAdded : DateTime.MinValue descending
                                         select dp).ToList();
                }

                else if (s == "RemainingWeek")
                {
                    newDisplayPlayers = (from dp in allDps
                                         orderby dp.RemainingWeekGames != null ? dp.RemainingWeekGames.Count() : int.MinValue descending,
                                         (dp.OwnershipPlayer != null ? dp.OwnershipPlayer.OwnershipPercent : 0) descending
                                         select dp).ToList();
                }

                else if (s == "NextWeek")
                {
                    newDisplayPlayers = (from dp in allDps
                                         orderby dp.NextWeekGames != null ? dp.NextWeekGames.Count() : int.MinValue descending,
                                         (dp.OwnershipPlayer != null ? dp.OwnershipPlayer.OwnershipPercent : 0) descending
                                         select dp).ToList();
                }

                else if (s == "AGAMES")
                {
                    newDisplayPlayers = (from dp in allDps
                                         orderby dp.TeamGameCount descending,
                                                             (dp.ValuePlayer != null ? dp.ValuePlayer.Rank : 0) ascending
                                         select dp).ToList();
                }

                else
                {
                    try
                    {
                        int sortCategoryId = Convert.ToInt32(s);
                        newDisplayPlayers = (from dp in allDps
                                             orderby (dp.StatPlayer != null ? dp.StatPlayer.Get(perValue, sortCategoryId) : int.MinValue)
                                             descending
                                             select dp).ToList();
                    }
                    catch { }
                }

                if (newDisplayPlayers != null)
                    allDps = newDisplayPlayers;
            }

            await db.FillDisplayPlayerUserLeagueTeamsAsync(userLeague, allDps);

            PlayerTableModel.SelectedUserLeague = userLeague;
            PlayerTableModel.ScoringSystem = userLeague.ScoringSystem;
            PlayerTableModel.UserId = UserId;
            PlayerTableModel.Sport = db.Sport;
            PlayerTableModel.CategorySettings = catSetttings;
            PlayerTableModel.PlayerType = selectedPlayerType;
            PlayerTableModel.DisplayPerValue = perValue;
            PlayerTableModel.UserDisplayCategories = await db.GetUserDisplayCategoriesAsync(UserId, userLeague, selectedPlayerType);
            PlayerTableModel.GamesCategoryId = db.GetGamesCategory(selectedPlayerType.Id).Id;
            PlayerTableModel.BeforeCategories = db.GetBeforeDisplayCategories(selectedPlayerType);
            PlayerTableModel.AfterCategories = db.GetAfterDisplayCategories(selectedPlayerType);
            PlayerTableModel.DisplayPlayers = new List<DisplayPlayer>();
            PlayerTableModel.ValuePerValues = db.GetPerValues(selectedPlayerType.Id);
            PlayerTableModel.ColorStats = ColorStats;
            PlayerTableModel.ShowDraft = ShowDraft;
            PlayerTableModel.ShowAdp = adpPlayers.Count > 0;
            PlayerTableModel.ShowAllAdp = allAdpPlayers != null;
            PlayerTableModel.ShowDefaultAdp = defaultAdpPlayers != null && defaultAdpPlayers.Count > 0;
            PlayerTableModel.NextPick = NextPick;
            PlayerTableModel.Draft = draft;
            PlayerTableModel.ShowMonsterBot = monsterBotPlayers != null;
            PlayerTableModel.ShowEase = db.Sport.IsNFL;
            PlayerTableModel.DateSelects = DateSelects;
            PlayerTableModel.StreamSelects = StreamSelects;
            PlayerTableModel.PositionBoostHashModel = PositionBoostHashModel;
            PlayerTableModel.ShowTeam = false;
            PlayerTableModel.ShowPositions = false;
            PlayerTableModel.ShowInjuries = false;

            //if (selectedDates != null && selectedDates.Count > 0)
            //{
            //    var matchDps = new List<DisplayPlayer>();
            //    foreach (var dp in allDps)
            //    {
            //        bool matchesAllDates = true;
            //        foreach (var gameDate in selectedDates)
            //        {
            //            if (!matchesAllDates)
            //                break;

            //            bool matchesCurrentDate = false;
            //            var dateGames = db.GetGames(season, gameDate, gameDate);
            //            foreach (var dateGame in dateGames)
            //            {
            //                if (dateGame.IncludesTeam(dp.SeasonPlayer.TeamId))
            //                    matchesCurrentDate = true;
            //            }
            //            if (!matchesCurrentDate)
            //                matchesAllDates = false;
            //        }

            //        if (matchesAllDates)
            //            matchDps.Add(dp);
            //    }
            //    allDps = matchDps;
            //}

            //if (streamDates != null && streamDates.Count > 0)
            //{
            //    var matchDps = new List<DisplayPlayer>();
            //    foreach (var dp in allDps)
            //    {
            //        if (dp.PlayerGameDates == null || dp.PlayerGameDates.Count == 0)
            //            continue;

            //        foreach (var streamDate in streamDates)
            //        {
            //            bool match = false;
            //            foreach (PlayerGameDate playerGameDate in dp.PlayerGameDates)
            //            {
            //                if (playerGameDate.PlayerGameState != null && playerGameDate.PlayerGameState.Game.GameDate == streamDate)
            //                {
            //                    match = true;
            //                    break;
            //                }
            //            }
            //            if (match)
            //            {
            //                matchDps.Add(dp);
            //                break;
            //            }
            //        }
            //    }
            //    allDps = matchDps;
            //}

            foreach (var dp in allDps)
            {
                if (t > 0 && dp.SeasonPlayer.Team.Id != t)
                    continue;
                if (SelectedFilterId == 3 && dp.UserLeagueTeam != null)
                    continue;
                if (SelectedFilterId == 4 && (dp.UserLeagueTeam == null || dp.UserLeagueTeam.ProviderId != userLeague.MyProviderTeamId))
                    continue;
                if (SelectedFilterId == 5 && dp.UserLeagueTeam != null && dp.UserLeagueTeam.ProviderId != userLeague.MyProviderTeamId)
                    continue;
                else if (filterTeamId > 0 && (dp.UserLeagueTeam == null || dp.UserLeagueTeam.Id != filterTeamId))
                    continue;
                else if (SelectedFilterId != 2 && PlayerTableModel.DisplayPlayers.Count > (leagueSize + 30))
                    continue;
                else if (HideDrafted && dp.DraftPlayer != null)
                    continue;
                else if (b == "depthmode" && (dp.StatPlayer == null || dp.StatPlayer.Games == 0))
                    continue;

                bool positonMatch = false;
                foreach (var pos in dp.Positions)
                {
                    if ((from pos2 in selectedDisplayActiveRosterSpot.Positions where pos2.Id == pos.Id select pos2).FirstOrDefault() != null)
                        positonMatch = true;
                }
                if (!positonMatch)
                    continue;

                PlayerTableModel.DisplayPlayers.Add(dp);
            }

            var myLastPlayer = (from dp1 in PlayerTableModel.DisplayPlayers where dp1.IsMyPlayer select dp1).LastOrDefault();
            if (myLastPlayer != null)
                myLastPlayer.IsMyLastPlayer = true;

            if (b == "draftmode")
            {
                if (userLeague.ProviderLeagueId != null)
                {
                    string showLeagueId = (userLeague.ProviderLeagueId.Length > 0 ? " (" + userLeague.ProviderLeagueId + ")" : "");
                    ButtonMessage = "Players are sorted by ADP for drafting.  You can optionally enter your League ID" + showLeagueId + " or a Mock Draft ID to see picks live.";
                }
            }
            else if (b == "depthmode")
                ButtonMessage = "When viewing depth, use the Team filter to select a single team.";

            return Page();
        }

        public List<DateTime> GetSelectedDates(DateTime startDate, DateSelect[] selects)
        {
            var selectedDates = new List<DateTime>();
            if (selects != null)
            {
                for (int day = 0; day < selects.Length; day++)
                {
                    var dateSelect = selects[day];
                    if (dateSelect.Selected)
                        selectedDates.Add(startDate.AddDays(day));
                }
            }

            return selectedDates;
        }

        public DateTime GetToday()
        {
            return db.GetCurrentGameDate(db.GetDefaultSeason());
        }

        public DateSelect[] GetDateSelectDays(DateTime startDate, DateTime endDate, string selectedIndexes)
        {
            int days = Convert.ToInt32((endDate - startDate).TotalDays) + 1;

            var selects = new DateSelect[days];

            DateTime currentDate = startDate;
            for (int i = 0; i < days; i++)
            {
                var dateSelect = new DateSelect();
                dateSelect.Id = i;
                dateSelect.Name = currentDate.DayOfWeek.ToString().Substring(0, 3);
                selects[i] = dateSelect;
                currentDate = currentDate.AddDays(1);
            }

            if (selectedIndexes != null)
            {
                foreach (string selectedIndex in selectedIndexes.Split("_"))
                {
                    selects[Convert.ToInt32(selectedIndex)].Selected = true;
                }
            }

            return selects;
        }

    }
}
