using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RotoMonster.Core;
using RotoMonster.Data;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace RotoMonster.Pages
{
    public class NewsFeedRow
    {
        public DisplayPlayer DisplayPlayer { get; set; }
        public PlayerStatus PlayerStatus { get; set; }
        public double? Start { get; set; }
        public int PlayerId { get; set; }
        public string Name { get; set; }
        public string Team { get; set; }
        public List<string> Positions { get; set; } = new List<string>();
        public string Status { get; set; }
        public string StatusBg { get; set; }
        public string StatusFg { get; set; }
        public string Group { get; set; }
        public string Details { get; set; }
        public string Return { get; set; }
        public string Change { get; set; }
        public string Time { get; set; }
        public DateTime LocalDate { get; set; }
        public double? Own { get; set; }
        public bool Mine { get; set; }
        public string SourceUrl { get; set; }
    }

    public class NewsFeedDay
    {
        public string Label { get; set; }
        public string Sub { get; set; }
        public List<NewsFeedRow> Rows { get; set; } = new List<NewsFeedRow>();
    }

    public class NewsModel : RMPageModel
    {
        public List<NewsFeedRow> Rows { get; set; } = new List<NewsFeedRow>();
        public List<NewsFeedRow> MyRows { get; set; } = new List<NewsFeedRow>();
        public List<NewsFeedDay> Days { get; set; } = new List<NewsFeedDay>();
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

            var picked = new List<Tuple<PlayerStatus, PlayerStatus, DisplayPlayer, double?>>();
            var statuses = await db.GetActivePlayerStatusesAsync();
            foreach (var g in statuses.Where(s => s.PlayerStatusType != null).GroupBy(s => s.PlayerId))
            {
                var ordered = g.OrderByDescending(s => s.DateAdded).ToList();
                var ps = ordered[0];
                var prev = ordered.Count > 1 ? ordered[1] : null;
                var sp = db.GetSeasonPlayer(ps.PlayerId);
                if (sp == null || sp.Player == null || sp.Team == null)
                    continue;
                var dp = new DisplayPlayer();
                dp.SeasonPlayer = sp;
                dp.Positions = positionsByPlayer.TryGetValue(ps.PlayerId, out var pl) ? pl : new List<Position>();
                ownership.TryGetValue(ps.PlayerId, out var op);
                picked.Add(Tuple.Create(ps, prev, dp, op != null ? (double?)op.OwnershipPercent : null));
            }

            if (selectedLeague != null && picked.Count > 0)
                await db.FillDisplayPlayerUserLeagueTeamsAsync(selectedLeague, picked.Select(i => i.Item3).ToList());

            var today = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, Game.DisplayZone).Date;

            foreach (var i in picked.OrderByDescending(x => x.Item1.DateAdded))
            {
                var ps = i.Item1;
                var sp = i.Item3.SeasonPlayer;
                var typeTitle = (ps.PlayerStatusType.Title ?? "").Trim();
                var details = (ps.Comment ?? "").Trim();
                if (typeTitle.Length > 0 && details.StartsWith(typeTitle + " - ", StringComparison.OrdinalIgnoreCase))
                    details = details.Substring(typeTitle.Length + 3).Trim();
                if (details.Equals("unknown", StringComparison.OrdinalIgnoreCase))
                    details = "";
                if (details.Length == 0)
                {
                    var subject = (ps.Subject ?? "").Trim();
                    if (!subject.Equals("unknown", StringComparison.OrdinalIgnoreCase))
                        details = subject;
                }
                if (details.Length > 0)
                    details = char.ToUpperInvariant(details[0]) + details.Substring(1);

                var bg = Hex(ps.PlayerStatusType.BackgroundColor) ?? "#64748b";
                var local = TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(ps.DateAdded, DateTimeKind.Utc), Game.DisplayZone);

                string change = null;
                if (i.Item2 != null && i.Item2.PlayerStatusType != null)
                {
                    var prevTitle = (i.Item2.PlayerStatusType.Title ?? "").Trim();
                    if (prevTitle.Length > 0 && !prevTitle.Equals(typeTitle, StringComparison.OrdinalIgnoreCase))
                        change = prevTitle + " → " + typeTitle;
                }

                ownership.TryGetValue(ps.PlayerId, out var rowOwn);
                Rows.Add(new NewsFeedRow
                {
                    DisplayPlayer = i.Item3,
                    PlayerStatus = ps,
                    Start = rowOwn != null ? (double?)rowOwn.ActivePercent : null,
                    PlayerId = sp.Player.Id,
                    Name = ((sp.Player.FirstName ?? "") + " " + (sp.Player.LastName ?? "")).Trim(),
                    Team = sp.Team.Code != null ? sp.Team.Code.Trim() : "",
                    Positions = (i.Item3.Positions ?? new List<Position>()).Where(p => p != null && !string.IsNullOrWhiteSpace(p.Abbreviation)).Select(p => p.Abbreviation.Trim()).ToList(),
                    Status = typeTitle,
                    StatusBg = bg,
                    StatusFg = TextOn(bg),
                    Group = PlayerNewsCards.Group(ps),
                    Details = details,
                    Return = ps.EstimatedReturnDate.HasValue ? ps.EstimatedReturnDate.Value.ToString("MMM d", CultureInfo.InvariantCulture) : null,
                    Change = change,
                    Time = local.ToString("h:mm tt", CultureInfo.InvariantCulture),
                    LocalDate = local.Date,
                    Own = i.Item4,
                    Mine = selectedLeague != null && i.Item3.IsMyPlayer,
                    SourceUrl = ps.SourceUrl
                });
            }

            MyRows = Rows.Where(r => r.Mine).Select(r => new NewsFeedRow { DisplayPlayer = r.DisplayPlayer, PlayerStatus = r.PlayerStatus, Start = r.Start, PlayerId = r.PlayerId, Name = r.Name, Team = r.Team, Positions = r.Positions, Status = r.Status, StatusBg = r.StatusBg, StatusFg = r.StatusFg, Group = r.Group, Details = r.Details, Return = r.Return, Change = r.Change, Time = r.LocalDate == today ? r.Time : r.LocalDate.ToString("ddd", CultureInfo.InvariantCulture) + " " + r.Time, LocalDate = r.LocalDate, Own = r.Own, Mine = r.Mine, SourceUrl = r.SourceUrl }).ToList();

            foreach (var g in Rows.GroupBy(r => r.LocalDate))
            {
                var label = g.Key == today ? "Today"
                    : g.Key == today.AddDays(-1) ? "Yesterday"
                    : g.Key.ToString("dddd", CultureInfo.InvariantCulture);
                var sub = g.Key >= today.AddDays(-1) ? g.Key.ToString("ddd, MMM d", CultureInfo.InvariantCulture) : g.Key.ToString("MMM d", CultureInfo.InvariantCulture);
                if (g.Key < today.AddDays(-6))
                {
                    label = g.Key.ToString("MMM d", CultureInfo.InvariantCulture);
                    sub = g.Key.ToString("dddd", CultureInfo.InvariantCulture);
                }
                Days.Add(new NewsFeedDay { Label = label, Sub = sub, Rows = g.ToList() });
            }
        }

        private static string Hex(string c)
        {
            if (string.IsNullOrWhiteSpace(c))
                return null;
            c = c.Trim();
            return c.StartsWith("#") ? c : "#" + c;
        }

        private static string TextOn(string hex)
        {
            var h = hex.TrimStart('#');
            if (h.Length == 3)
                h = new string(new[] { h[0], h[0], h[1], h[1], h[2], h[2] });
            if (h.Length != 6 || !int.TryParse(h, NumberStyles.HexNumber, null, out var v))
                return "#ffffff";
            double r = (v >> 16) & 255, g = (v >> 8) & 255, b = v & 255;
            var lum = (0.299 * r + 0.587 * g + 0.114 * b) / 255;
            return lum > 0.6 ? "#111827" : "#ffffff";
        }
    }
}
