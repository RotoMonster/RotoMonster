using System;
using System.Collections.Generic;
using System.Linq;
using RotoMonsterUI;

namespace RotoMonster.Pages
{
    public class PlayerNewsColumnModel
    {
        public string Title { get; set; } = "Latest news";
        public string EmptyText { get; set; } = "No player news right now.";
        public double AgeWindowMinutes { get; set; } = 15;
        public bool Grid { get; set; }
        public bool Strip { get; set; }
        public bool Compact { get; set; }
        public string SeeAllUrl { get; set; }
        public Dictionary<int, string> Groups { get; set; } = new Dictionary<int, string>();
        public List<NewsCardInput> Cards { get; set; } = new List<NewsCardInput>();
    }

    public static class PlayerNewsCards
    {
        public static string Group(RotoMonster.Core.PlayerStatus ps)
        {
            if (ps == null || ps.PlayerStatusType == null) return "other";
            var badge = (ps.PlayerStatusType.TextFormat ?? "").Replace("{t}", "").Trim().ToUpperInvariant();
            if (ps.IsOut || badge == "OUT" || badge.StartsWith("IR")) return "out";
            if (badge.StartsWith("D")) return "doubtful";
            if (badge.StartsWith("Q")) return "questionable";
            return "other";
        }

        public static NewsCardInput FromStatus(RotoMonster.Core.PlayerStatus s, RotoMonster.Core.DisplayPlayer dp, RotoMonster.Core.Sport sport, double? own, bool mine, bool dark)
        {
            var sp = dp.SeasonPlayer;
            var code = sp.Team != null && sp.Team.Code != null ? sp.Team.Code.Trim() : "";
            var sportTitle = sport != null && sport.Title != null ? sport.Title : "";
            string teamColor = null;
            if (code.Length > 0)
            {
                if (sportTitle == "NBA") teamColor = TeamColorHelper.GetNbaTeamColorVar(code);
                else if (sportTitle == "NFL" || sportTitle == "NHL") teamColor = "var(--team-" + sportTitle.ToLower() + "-" + code.ToLower() + ", var(--brand-primary))";
                else teamColor = TeamColorHelper.GetTeamColorVar(code);
            }

            var typeTitle = s.PlayerStatusType != null && s.PlayerStatusType.Title != null ? s.PlayerStatusType.Title.Trim() : "";
            var details = (s.Comment ?? "").Trim();
            if (typeTitle.Length > 0 && details.StartsWith(typeTitle + " - ", StringComparison.OrdinalIgnoreCase))
                details = details.Substring(typeTitle.Length + 3).Trim();
            if (details.Equals("unknown", StringComparison.OrdinalIgnoreCase)) details = "";
            var title = (s.Subject ?? "").Trim();
            if (title.Equals("unknown", StringComparison.OrdinalIgnoreCase) || (title.Length > 0 && details.IndexOf(title, StringComparison.OrdinalIgnoreCase) >= 0))
                title = "";
            if (s.EstimatedReturnDate.HasValue)
                details = (details.Length > 0 ? details + " \u00b7 " : "") + "est. return " + s.EstimatedReturnDate.Value.ToString("M/d");

            var positions = (dp.Positions ?? new List<RotoMonster.Core.Position>())
                .Where(p => p != null && !string.IsNullOrWhiteSpace(p.Abbreviation))
                .Select(p => new DisplayPosition
                {
                    Id = p.Id,
                    Abbreviation = p.Abbreviation,
                    Color = "var(--pos-" + p.Abbreviation.Trim().ToLower() + ")"
                }).ToList();

            return new NewsCardInput
            {
                NewsId = s.Id,
                DisplayPlayerInput = new DisplayPlayerInput
                {
                    PlayerId = sp.Player.Id,
                    PlayerName = ((sp.Player.FirstName ?? "") + " " + (sp.Player.LastName ?? "")).Trim(),
                    TeamCode = code,
                    TeamColor = teamColor,
                    Positions = positions,
                    OwnershipPercent = own.HasValue ? (int?)Math.Round(own.Value) : null
                },
                Sport = string.Equals(sportTitle, "MLB", StringComparison.OrdinalIgnoreCase) ? NewsCardSport.MLB : NewsCardSport.NBA,
                IsDarkMode = dark,
                StatusTypeText = typeTitle,
                StatusTypeColorCode = s.PlayerStatusType != null ? s.PlayerStatusType.BackgroundColor : null,
                NewsTitle = title.Length > 0 ? title : null,
                NewsDetails = details.Length > 0 ? details : null,
                SourceURL = s.SourceUrl,
                TimeSinceCreated = DateTime.UtcNow - s.DateAdded,
                OwnPercent = own,
                IsMyPlayer = mine,
                UserCanEdit = false,
                UserCanDelete = false
            };
        }
    }
}
