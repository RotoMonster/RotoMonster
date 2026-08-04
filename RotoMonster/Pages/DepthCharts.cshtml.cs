using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Intrinsics.Arm;
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
    public class DepthChartsModel : RMPageModel
    {
        public DepthChartsModel(IConfiguration config, IRMData db, IRMSharedData sharedDb, UserManager<ApplicationUser> userManager, IHttpContextAccessor contextAccessor, ILogger<PageModel> logger)
            : base(config, db, sharedDb, userManager, contextAccessor, logger)
        {
        }

        [BindProperty] public int SelectedUserLeagueId { get; set; }
        [BindProperty] public int SelectedTeamId { get; set; }
        [BindProperty][Display(Name = "Hide 0% Players")] public bool HideZeroPlayers { get; set; } = true;
        [BindProperty][Display(Name = "Show Game Monster Bar")] public bool ShowGameMonsterBar { get; set; } = true;
        [BindProperty][Display(Name = "Show Total Monster Bar")] public bool ShowTotalMonsterBar { get; set; } = false;

        public List<DepthChartTeamModel> DepthChartTeamModels { get; set; } = new List<DepthChartTeamModel>();
        public MonsterBar MonsterBar { get; set; } = null;
        public MonsterBar TotalMonsterBar { get; set; } = null;
        public List<Position> AllPositions { get; set; }
        public bool ShowPositionPercents { get; set; } = false;

        public async Task OnGetAsync(int? t, int? hid, int? l, bool? z, bool? gmb, bool? tmb)
        {
            InitGet("Depth Charts");
            if (hid.GetValueOrDefault(0) > 0)
                ViewData["Helper"] = db.GetHelper(hid.GetValueOrDefault(0));

            HideZeroPlayers=z.GetValueOrDefault(true);
            ShowGameMonsterBar=gmb.GetValueOrDefault(true);
            ShowTotalMonsterBar=tmb.GetValueOrDefault(false);

            if (UserId != null)
            {
                SelectedUserLeagueId = l.GetValueOrDefault();
                ViewData["UserLeagueList"] = new SelectList(db.GetTrackedUserLeagues(UserId), "Id", "ListDisplayTitle");
            }
            UserLeague userLeague = db.SelectUserLeague(UserId, db.GetUserLeague(UserId, SelectedUserLeagueId));
            if (userLeague == null)
                userLeague = db.GetDefaultUserLeague();

            var fantasyProvider = (userLeague != null ? userLeague.FantasyProvider : db.GetDefaultFantasyProvider());
            var positionSource = db.GetPositionSource(fantasyProvider);

            ColorLib colorLib = new ColorLib();
            ValuePlayerLib lib = new ValuePlayerLib();
            SelectedTeamId = (t != null ? t.GetValueOrDefault() : -1);
            var season = db.GetDefaultSeason();
            if (!season.HasStarted)
                season = db.GetPreviousSeason(season.Year.GetValueOrDefault() - 1);
            var displaySeason = db.GetDefaultSeason();
            ViewData["TeamList"] = new SelectList(db.GetTeamsSelectItems(displaySeason), "Value", "Text");

            foreach (var playerType in db.GetPlayerTypes())
            {
                var seasonPlayers = db.GetSeasonPlayers(displaySeason, playerType);
                List<CategorySetting> catSetttings = db.GetUserLeagueCategorySettings(userLeague, playerType);

                string scoringSystem = db.GetUserLeagueScoringSystem(userLeague);
                int leagueSize = db.GetUserLeagueLeagueSize(userLeague, playerType);
                var defCode = db.GetDefaultCategoriesString(playerType).Code;
                string leagueCategoriesCode = userLeague != null ? db.GetUserLeagueCategoryCode(userLeague, playerType) : defCode;

                List<OwnershipPlayer> ownershipPlayers = db.GetOwnershipPlayersWithChange(leagueCategoriesCode, DateTime.UtcNow, 24);

                if (ShowGameMonsterBar) 
                    MonsterBar = db.GetMonsterBar(playerType, season, catSetttings, scoringSystem, db.GetPerGamePerValue(playerType.Id), leagueSize, userLeague.ActiveSize(playerType));
                if(ShowTotalMonsterBar)
                    TotalMonsterBar=db.GetMonsterBar(playerType, season, catSetttings, scoringSystem, db.GetTotalPerValue(playerType.Id), leagueSize, userLeague.ActiveSize(playerType));

                var injuries = await db.GetPlayerInjuriesAsync();
                var playerStatuses = db.GetActivePlayerStatuses();
                var depthPlayers = db.GetDepthPlayers(playerType, leagueCategoriesCode, DateTime.UtcNow, false);
                var playerDefaultPositions = db.GetUserLeagueSeasonPlayerPositions(userLeague, displaySeason);
                var positionSourcePositions = (from pp in db.GetPositionSourcePositions(db.GetPositionSource(userLeague.FantasyProvider)) where pp.PlayerType.Id == playerType.Id select pp).ToList();
                var playerPositionPercents = db.GetPlayerPositionPercents(season, season.StartDate, season.UpdatedDate);
                ShowPositionPercents=(playerPositionPercents.Count>0);

                AllPositions = db.GetActualPositions(playerType);

                foreach (var seasonTeam in (from tm in displaySeason.SeasonTeams where tm.Team.Code!="FA" orderby tm.Team.Name select tm))
                {
                    if (t == null || t.GetValueOrDefault(0)== seasonTeam.Team.Id || t.GetValueOrDefault(0)==-1)
                    {
                        var depthChartTeamModel = (from dcm in DepthChartTeamModels where dcm.Team.Id==seasonTeam.TeamId select dcm).FirstOrDefault();
                        if (depthChartTeamModel==null)
                        {
                            depthChartTeamModel = new DepthChartTeamModel() { Team = seasonTeam.Team };
                            DepthChartTeamModels.Add(depthChartTeamModel);
                        }

                        var teamSeasonPlayers = (from sp in seasonPlayers where sp.Team.Id == seasonTeam.Team.Id select sp).ToList();
                        foreach (var seasonPlayer in teamSeasonPlayers)
                        {
                            var ownershipPlayer = (from op in ownershipPlayers where op.Player.Id==seasonPlayer.PlayerId select op).FirstOrDefault();

                            if (HideZeroPlayers&&ownershipPlayer==null)
                                continue;

                            var displayPlayer = new DisplayPlayer();
                            displayPlayer.SeasonPlayer=seasonPlayer;
                            displayPlayer.OwnershipPlayer=ownershipPlayer;
                            displayPlayer.DepthPlayer=(from dp in depthPlayers where dp.SeasonPlayer.PlayerId==seasonPlayer.PlayerId select dp).FirstOrDefault();
                            if(MonsterBar!=null)
                                displayPlayer.MonsterBarGamePlayer=(from mb in MonsterBar.MonsterBarPlayers where mb.Player.Id==seasonPlayer.PlayerId select mb).FirstOrDefault();
                            if (TotalMonsterBar!=null)
                                displayPlayer.MonsterBarTotalPlayer=(from mb in TotalMonsterBar.MonsterBarPlayers where mb.Player.Id==seasonPlayer.PlayerId select mb).FirstOrDefault();
                            displayPlayer.Positions=(from p1 in playerDefaultPositions where p1.PlayerId == seasonPlayer.Player.Id select p1.Position).ToList();
                            displayPlayer.PlayerPositionPercents = playerPositionPercents != null ? (from pp in playerPositionPercents where pp.Player.Id == seasonPlayer.PlayerId select pp).ToList() : null;
                            displayPlayer.PlayerInjury = (from p1 in injuries where p1.PlayerId == seasonPlayer.Player.Id select p1).FirstOrDefault();
                            displayPlayer.PlayerStatus = (from p1 in playerStatuses where p1.PlayerId == seasonPlayer.Player.Id select p1).FirstOrDefault();
                            if (displayPlayer.DepthPlayer != null)
                                displayPlayer.HigherDepthInjuredDisplayPlayers = displayPlayer.DepthPlayer.GetHigherDepthInjuredDisplayPlayers(playerStatuses);

                            if (displayPlayer.OwnershipPlayer!=null || (displayPlayer.MonsterBarGamePlayer!=null && displayPlayer.MonsterBarGamePlayer.MonsterBarValuePlayers.First()!=null))
                                depthChartTeamModel.DisplayPlayers.Add(displayPlayer);
                        }

                        db.FillDisplayPlayerUserLeagueTeams(userLeague, depthChartTeamModel.DisplayPlayers);

                        depthChartTeamModel.DisplayPlayers=(from dp in depthChartTeamModel.DisplayPlayers
                                                            orderby
                                                            (dp.SeasonPlayer.PlayerType.DisplayOrder),
                                                            (dp.DepthPlayer != null && dp.DepthPlayer.Position != null ? dp.DepthPlayer.Position.DisplayOrder : 1000) ascending,
                                                            (dp.DepthPlayer != null && dp.DepthPlayer.Depth > 0 ? dp.DepthPlayer.Depth : 1000) ascending,
                                                            dp.SeasonPlayer.Player.DefaultPosition!=null ? dp.SeasonPlayer.Player.DefaultPosition.DisplayOrder : int.MaxValue ascending
                                                            select dp).ToList();
                    }
                }
            }
        }

        public IActionResult OnPostRefresh()
        {
            return RedirectToPage("./DepthCharts", new
            {
                l = SelectedUserLeagueId,
                t = SelectedTeamId,
                z = HideZeroPlayers,
                gmb = ShowGameMonsterBar,
                tmb = ShowTotalMonsterBar
            });
        }

    }
}
