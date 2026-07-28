using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RotoMonster.Core;
using RotoMonster.Core.Libs;
using RotoMonster.Data;
using System.IO;

namespace RotoMonster.Pages.UserLeagues
{

    public class EditModel : PageModel
    {
        private readonly RotoMonster.Data.RMDBContext _context;
        private readonly IRMData db;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IHttpContextAccessor contextAccessor;

        public string UserId { get; private set; }

        public EditModel(RotoMonster.Data.RMDBContext context, IRMData db, UserManager<ApplicationUser> userManager, IHttpContextAccessor contextAccessor)
        {
            _context = context;
            this.db = db;
            this.userManager = userManager;
            this.contextAccessor = contextAccessor;

            UserId = userManager.GetUserId(contextAccessor.HttpContext.User);
        }

        [BindProperty]
        public UserLeague UserLeague { get; set; }

        [BindProperty]
        public List<CategorySelect> CategorySelects { get; set; }

        [BindProperty]
        public List<PointsValue> PointsValues { get; set; }

        [BindProperty]
        public List<ActiveRosterSpotValue> ActiveRosterSpotValues { get; set; }

        [BindProperty]
        public string SelectedTeamId { get; set; }
        public UserLeagueTeam SelectedUserLeagueTeam { get; set; }


        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            //Stream stream = await "https://fantasy.espn.com/basketball/draft?leagueId=50604361&seasonId=2022&teamId=10&memberId={EA381B6B-B5AC-443D-B164-B27D3ECAF8C9}".GetStreamAsync();

            UserLeague = db.GetUserLeague(UserId, id.GetValueOrDefault(0));

            ViewData["TeamSelectList"] = new SelectList(UserLeague.UserLeagueTeams, "ProviderId", "Title");
            if (UserLeague.MyProviderTeamId.Length > 0)
                SelectedUserLeagueTeam = (from ult in UserLeague.UserLeagueTeams where ult.ProviderId == UserLeague.MyProviderTeamId select ult).FirstOrDefault();
            if (SelectedUserLeagueTeam != null)
                SelectedTeamId = SelectedUserLeagueTeam.ProviderId;

            UserLeagueLib userLeagueLib = new UserLeagueLib();
            ViewData["ScoringSystemList"] = userLeagueLib.ScoringSystemList;
            ViewData["LeagueTypeList"] = userLeagueLib.LeagueTypeList;
            ViewData["LineupFrequencyList"] = userLeagueLib.LineupFrequencyList;
            ViewData["CategorySelectionList"] = userLeagueLib.CategorySelectionList;
            ViewData["PlayerTypes"] = db.GetPlayerTypes();

            CategorySelects = new List<CategorySelect>();
            foreach (var cat in db.GetValueCategories())
            {
                CategorySelect select = new CategorySelect();
                select.Title = cat.DisplayTitle;
                var catSetting = (from c in UserLeague.UserLeagueCategories where c.CategoryId == cat.Id select c).FirstOrDefault();
                if (catSetting == null)
                    select.Value = "Off";
                else if (catSetting.IsActive)
                    select.Value = "On";
                else
                    select.Value = "Punt";
                select.Id = "Cat" + cat.Id.ToString();
                select.Name = select.Id;
                CategorySelects.Add(select);
            }

            PointsValues = new List<PointsValue>();
            foreach (var cat in db.GetPointCategories())
            {
                PointsValue pv = new PointsValue();
                pv.Title = cat.DisplayTitle;
                pv.Id = "PCat" + cat.Id.ToString();
                pv.Name = pv.Id;
                var catSetting = (from c in UserLeague.UserLeagueCategories where c.CategoryId == cat.Id select c).FirstOrDefault();
                if (catSetting != null && catSetting.PointsPerStat != 0)
                {
                    pv.Value = catSetting.PointsPerStat;
                }
                PointsValues.Add(pv);
            }

            ActiveRosterSpotValues = new List<ActiveRosterSpotValue>();
            foreach (var activeRosterSpot in db.GetActiveRosterSpots())
            {
                ActiveRosterSpotValue arsv = new ActiveRosterSpotValue();
                arsv.Title = activeRosterSpot.Title;
                arsv.Id = "Ars" + activeRosterSpot.Id.ToString();
                arsv.Name = arsv.Id;
                var arsSetting = (from ars in UserLeague.UserLeagueActiveRosterSpots where ars.ActiveRosterSpotId == activeRosterSpot.Id select ars).FirstOrDefault();
                if (arsSetting != null && arsSetting.NumberOfPlayers > 0)
                {
                    arsv.Value = arsSetting.NumberOfPlayers;
                }
                ActiveRosterSpotValues.Add(arsv);
            }

            if (UserLeague == null)
            {
                return NotFound();
            }

            ViewData["FantasyProviderId"] = new SelectList(_context.FantasyProviders, "Id", "Id");
            return Page();
        }

        public async Task<IActionResult> OnPostRefreshAsync()
        {
            TempData["message"] = "Your settings have been updated.";

            return RedirectToPage("./Edit", new { id = UserLeague.Id });
        }

        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostSaveAsync()
        {
            if (!ModelState.IsValid)
            {
                return RedirectToPage("./Edit", new { id = UserLeague.Id });
            }

            var userLeagueTeams = db.GetUserLeagueTeams(UserLeague);

            var selectedTeam = (from ult in userLeagueTeams where ult.ProviderId == SelectedTeamId select ult).FirstOrDefault();
            if (selectedTeam != null)
            {
                UserLeague.MyProviderTeamId = selectedTeam.ProviderId;
            }

            var cats = db.GetValueCategories();
            for (int i = 0; i < cats.Count; i++)
            {
                var catSelect = CategorySelects[i];
                var cat = cats[i];
                if (catSelect.Value == "On" || catSelect.Value == "Punt")
                {
                    UserLeagueCategory ulc = new UserLeagueCategory();
                    ulc.CategoryId = cat.Id;
                    ulc.IsActive = (catSelect.Value == "On");
                    UserLeague.UserLeagueCategories.Add(ulc);
                }
            }

            var pointCats = db.GetPointCategories();
            for (int i = 0; i < pointCats.Count; i++)
            {
                var cat = pointCats[i];

                var pointsVal = PointsValues[i];
                if (pointsVal.Value != 0)
                {
                    var ulc = (from u in UserLeague.UserLeagueCategories where u.CategoryId == cat.Id select u).FirstOrDefault();
                    if (ulc == null)
                    {
                        ulc = new UserLeagueCategory();
                        ulc.CategoryId = cat.Id;
                        ulc.IsActive = true;
                        UserLeague.UserLeagueCategories.Add(ulc);
                    }
                    ulc.PointsPerStat = pointsVal.Value;
                }
            }

            var activeRosterSpots = db.GetActiveRosterSpots();
            for (int i = 0; i < activeRosterSpots.Count; i++)
            {
                ActiveRosterSpot activeRosterSpot = activeRosterSpots[i];

                var arsv = ActiveRosterSpotValues[i];
                if (arsv.Value > 0)
                {
                    var ulars = new UserLeagueActiveRosterSpot();
                    ulars.ActiveRosterSpotId = activeRosterSpot.Id;
                    ulars.NumberOfPlayers = arsv.Value;
                    UserLeague.UserLeagueActiveRosterSpots.Add(ulars);
                }
            }

            db.UpdateUserLeague(UserLeague);

            TempData["message"] = "Your settings have been saved.";

            return RedirectToPage("./Edit", new { id = UserLeague.Id });
        }

        private bool UserLeagueExists(int id)
        {
            return _context.UserLeagues.Any(e => e.Id == id);
        }
    }

    public class TeamSelect
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool Selected { get; set; }
    }

}
