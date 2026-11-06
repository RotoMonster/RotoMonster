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
    public class InjuryRow
    {
        public DisplayPlayer DisplayPlayer { get; set; }
        public PlayerStatus PlayerStatus { get; set; }
        public string Name { get; set; }
        public string TeamCode { get; set; }
        public string PositionCodes { get; set; }
        public string Group { get; set; }
        public string Injury { get; set; }
        public double MinutesAgo { get; set; }
        public double? Own { get; set; }
        public double? Start { get; set; }
        public int Mine { get; set; }
    }

    public class InjuriesModel : RMPageModel
    {
        public Sport Sport { get; set; }
        public List<InjuryRow> Rows { get; set; } = new List<InjuryRow>();
        public bool HasMine { get; set; }

        public InjuriesModel(IConfiguration config, IRMData db, IRMSharedData sharedDb, UserManager<ApplicationUser> userManager, IHttpContextAccessor contextAccessor, ILogger<PageModel> logger)
            : base(config, db, sharedDb, userManager, contextAccessor, logger)
        {
        }

        public async Task OnGetAsync(int? l)
        {
            InitGet("Injuries");
            ViewData["sport"] = db.Sport.Title;
            Sport = db.Sport;
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

                string badge = (ps.PlayerStatusType.TextFormat ?? "").Replace("{t}", "").Trim().ToUpperInvariant();
                string group = ps.IsOut || badge == "OUT" || badge.StartsWith("IR") ? "out"
                    : badge.StartsWith("D") ? "doubtful"
                    : badge.StartsWith("Q") ? "questionable"
                    : "other";

                ownership.TryGetValue(ps.PlayerId, out var op);

                Rows.Add(new InjuryRow
                {
                    DisplayPlayer = dp,
                    PlayerStatus = ps,
                    Name = (sp.Player.FirstName + " " + sp.Player.LastName).Trim(),
                    TeamCode = sp.Team.Code,
                    PositionCodes = string.Join(" ", dp.Positions.Select(p => (p.Abbreviation ?? "").Trim().ToLowerInvariant())),
                    Group = group,
                    Injury = CleanInjury(ps.Subject),
                    MinutesAgo = ps.TimeSince.TotalMinutes,
                    Own = op != null ? op.OwnershipPercent : (double?)null,
                    Start = op != null ? op.ActivePercent : (double?)null,
                    Mine = 0
                });
            }

            if (selectedLeague != null && Rows.Count > 0)
            {
                await db.FillDisplayPlayerUserLeagueTeamsAsync(selectedLeague, Rows.Select(r => r.DisplayPlayer).ToList());
                foreach (var r in Rows)
                    r.Mine = r.DisplayPlayer.IsMyPlayer ? (r.DisplayPlayer.IsActive ? 2 : 1) : 0;
            }

            Rows = Rows.OrderByDescending(r => r.Own ?? -1).ThenBy(r => r.MinutesAgo).ToList();
        }

        private static string CleanInjury(string subject)
        {
            var s = (subject ?? "").Trim();
            if (s.Length == 0 || s.Equals("unknown", StringComparison.OrdinalIgnoreCase))
                return "\u2014";
            return string.Join("/", s.Split('/').Select(p => p.Trim()).Where(p => p.Length > 0).Select(p => char.ToUpperInvariant(p[0]) + p.Substring(1)));
        }
    }
}
