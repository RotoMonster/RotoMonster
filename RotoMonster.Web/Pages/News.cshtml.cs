using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RotoMonster.Core;
using RotoMonster.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RotoMonster.Pages
{
    public class NewsModel : RMPageModel
    {
        public PlayerNewsColumnModel News { get; set; } = new PlayerNewsColumnModel();
        public bool HasMine { get; set; }

        public NewsModel(IConfiguration config, IRMData db, IRMSharedData sharedDb, UserManager<ApplicationUser> userManager, IHttpContextAccessor contextAccessor, ILogger<PageModel> logger)
            : base(config, db, sharedDb, userManager, contextAccessor, logger)
        {
        }

        public async Task OnGetAsync(int? l)
        {
            InitGet("News");
            ViewData["sport"] = db.Sport.Title;
            var season = db.GetDefaultSeason();

            var leagues = UserId != null ? await db.GetTrackedUserLeaguesAsync(UserId) : new List<UserLeague>();
            var pickedLeague = UserId != null && l.HasValue ? await db.GetUserLeagueAsync(UserId, l.Value) : null;
            var selectedLeague = UserId != null ? await db.SelectUserLeagueAsync(UserId, pickedLeague) : null;
            if (selectedLeague != null)
                SelectedUserLeagueId = selectedLeague.Id;
            HasMine = selectedLeague != null;

            var userLeague = selectedLeague ?? leagues.FirstOrDefault();
            var positions = userLeague != null
                ? db.GetUserLeagueSeasonPlayerPositions(userLeague, season)
                : db.GetPlayerSeasonPositions(db.GetDefaultFantasyProvider(), season);
            var positionsByPlayer = positions.GroupBy(p => p.PlayerId).ToDictionary(g => g.Key, g => g.Select(p => p.Position).Where(p => p != null).ToList());

            var ownership = db.GetAllDefaultOwnershipPlayers(DateTime.UtcNow)
                .GroupBy(o => o.PlayerId).ToDictionary(g => g.Key, g => g.First());

            var items = new List<Tuple<PlayerStatus, DisplayPlayer, double?>>();
            var statuses = await db.GetActivePlayerStatusesAsync();
            foreach (var ps in statuses.GroupBy(s => s.PlayerId).Select(g => g.OrderByDescending(s => s.DateAdded).First()))
            {
                if (ps.PlayerStatusType == null)
                    continue;
                var sp = db.GetSeasonPlayer(ps.PlayerId);
                if (sp == null || sp.Player == null || sp.Team == null)
                    continue;
                var dp = new DisplayPlayer();
                dp.SeasonPlayer = sp;
                dp.Positions = positionsByPlayer.TryGetValue(ps.PlayerId, out var pl) ? pl : new List<Position>();
                ownership.TryGetValue(ps.PlayerId, out var op);
                items.Add(Tuple.Create(ps, dp, op != null ? (double?)op.OwnershipPercent : null));
            }

            if (selectedLeague != null && items.Count > 0)
                await db.FillDisplayPlayerUserLeagueTeamsAsync(selectedLeague, items.Select(i => i.Item2).ToList());

            var dark = Request.Cookies["monster-theme"] == "dark";
            News.Title = null;
            News.Grid = true;
            News.EmptyText = "No player news right now.";
            foreach (var i in items.OrderByDescending(x => x.Item1.DateAdded))
            {
                News.Cards.Add(PlayerNewsCards.FromStatus(i.Item1, i.Item2, db.Sport, i.Item3, selectedLeague != null && i.Item2.IsMyPlayer, dark));
                News.Groups[i.Item1.Id] = PlayerNewsCards.Group(i.Item1);
            }
        }
    }
}
