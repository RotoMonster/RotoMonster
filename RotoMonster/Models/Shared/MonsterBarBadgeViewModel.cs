using System.Collections.Generic;
using RotoMonster.Core;

namespace RotoMonster.Models.Shared
{
    public enum MonsterBarGroup
    {
        None,
        Projection,
        LastSeason,
        CurrentSeason
    }

    public enum MonsterBarEmphasis
    {
        Top,
        Ownable,
        Dim
    }

    public class MonsterBarBadgeViewModel
    {
        public string Label { get; set; }

        public bool IsCompact { get; set; }

        public List<MonsterBarBadgeCell> Cells { get; set; } = new List<MonsterBarBadgeCell>();

        /// <summary>
        /// Maps the domain objects onto the badge shape. Kept here rather than in the
        /// partial so the call sites stay one line and the mapping is testable.
        /// </summary>
        public static MonsterBarBadgeViewModel From(
            MonsterBarPlayer monsterBarPlayer,
            List<MonsterBarItem> monsterBarItems,
            bool isCompact = false,
            string label = null,
            bool isTotal = false)
        {
            var model = new MonsterBarBadgeViewModel { IsCompact = isCompact, Label = label };

            if (monsterBarPlayer == null || monsterBarItems == null)
                return model;

            for (int i = 0; i < monsterBarItems.Count; i++)
            {
                MonsterBarItem item = monsterBarItems[i];

                MonsterBarValuePlayer valuePlayer = null;
                if (monsterBarPlayer.MonsterBarValuePlayers != null
                    && i < monsterBarPlayer.MonsterBarValuePlayers.Count)
                {
                    valuePlayer = monsterBarPlayer.MonsterBarValuePlayers[i];
                }

                if (valuePlayer == null || valuePlayer.ValuePlayer == null)
                {
                    model.Cells.Add(new MonsterBarBadgeCell { IsEmpty = true });
                    continue;
                }

                var cell = new MonsterBarBadgeCell
                {
                    Description = item == null ? null : item.Description,
                    ColorCode = valuePlayer.ValuePlayer.LeagueValueColor,
                    Group = GroupFor(item == null ? null : item.Title),
                    Emphasis = valuePlayer.IsTopPlayer
                        ? MonsterBarEmphasis.Top
                        : (valuePlayer.IsOwnablePlayer ? MonsterBarEmphasis.Ownable : MonsterBarEmphasis.Dim)
                };

                // The "D" (day) column shows its own title instead of a game count,
                // and never shows the measure text. Carried over from the old partial.
                if (item != null && item.Title == "D")
                {
                    cell.GamesText = item.Title;
                }
                else
                {
                    cell.GamesText = valuePlayer.ValuePlayer.StatPlayer == null
                        ? ""
                        : valuePlayer.ValuePlayer.StatPlayer.Games.ToString();

                    // Compact cells are too narrow for the measure text.
                    if (!isCompact
                        && valuePlayer.ValuePlayer.StatPlayer != null
                        && !string.IsNullOrEmpty(valuePlayer.ValuePlayer.StatPlayer.MeasureText))
                    {
                        cell.MeasureText = valuePlayer.ValuePlayer.StatPlayer.MeasureText;
                    }
                }

                model.Cells.Add(cell);
            }

            int playerId = (monsterBarPlayer.Player != null) ? monsterBarPlayer.Player.Id : 0;
            for (int c = 0; c < model.Cells.Count; c++)
                model.Cells[c].TooltipId = "mbtip-" + (isTotal ? "t" : "g") + "-" + playerId + "-" + c;

            return model;
        }

        /// <summary>
        /// Header row above a column of badges. Column labels come from the item
        /// descriptions so the header reads "Last Season" rather than "LS".
        /// </summary>
        public static MonsterBarBadgeViewModel Header(List<MonsterBarItem> monsterBarItems, string label = null)
        {
            var model = new MonsterBarBadgeViewModel { IsHeader = true, Label = label };

            if (monsterBarItems == null)
                return model;

            foreach (var item in monsterBarItems)
            {
                if (item == null) continue;

                model.Cells.Add(new MonsterBarBadgeCell
                {
                    GamesText = item.Title,
                    Description = item.Description,
                    Group = GroupFor(item.Title),
                    TooltipId = "mbtip-hdr-" + model.Cells.Count
                });
            }

            return model;
        }

        public bool IsHeader { get; set; }

        private static MonsterBarGroup GroupFor(string title)
        {
            switch (title)
            {
                case "LS":
                    return MonsterBarGroup.LastSeason;
                case "S":
                case "2M":
                case "3W":
                case "W":
                case "D":
                    return MonsterBarGroup.CurrentSeason;
                default:
                    return MonsterBarGroup.None;
            }
        }
    }

    public class MonsterBarBadgeCell
    {
        public string Description { get; set; }

        public string GamesText { get; set; }

        public string MeasureText { get; set; }

        public string ColorCode { get; set; }

        public MonsterBarEmphasis Emphasis { get; set; } = MonsterBarEmphasis.Dim;

        public MonsterBarGroup Group { get; set; } = MonsterBarGroup.None;

        public bool IsEmpty { get; set; }

        /// <summary>
        /// Set by the factory methods from the player, the bar type and the column
        /// index. Deliberately NOT a random guid - a random id makes every page
        /// render different, which breaks before/after HTML comparison.
        /// </summary>
        public string TooltipId { get; set; }

        /// <summary>
        /// Header labels can arrive with HTML entities already in them (&#x27;25).
        /// Decode once so Razor escapes once - same visible result, still safe.
        /// </summary>
        public string DecodedGamesText
        {
            get
            {
                if (string.IsNullOrEmpty(GamesText)) return "";
                return System.Net.WebUtility.HtmlDecode(GamesText);
            }
        }

        public string EmphasisClass
        {
            get
            {
                switch (Emphasis)
                {
                    case MonsterBarEmphasis.Top:
                        return "monster-bar-cell--top";
                    case MonsterBarEmphasis.Ownable:
                        return "monster-bar-cell--ownable";
                    default:
                        return "monster-bar-cell--dim";
                }
            }
        }

        public string GroupClass
        {
            get
            {
                switch (Group)
                {
                    case MonsterBarGroup.Projection:
                        return "monster-bar-cell--projection";
                    case MonsterBarGroup.LastSeason:
                        return "monster-bar-cell--last-season";
                    case MonsterBarGroup.CurrentSeason:
                        return "monster-bar-cell--current-season";
                    default:
                        return "";
                }
            }
        }

        /// <summary>
        /// LeagueValueColor arrives as a bare hex code with no leading #.
        /// </summary>
        public string BackgroundColor
        {
            get
            {
                if (string.IsNullOrEmpty(ColorCode)) return "";
                if (ColorCode.StartsWith("#") || ColorCode.StartsWith("var(")) return ColorCode;
                return "#" + ColorCode;
            }
        }
    }
}
