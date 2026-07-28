using System;
using System.Collections.Generic;
using System.Text;

namespace RotoMonster.Core
{
    public class MonsterBar
    {
        public PlayerType PlayerType { get; set; }
        public string MeasureText { get; set; } = "";
        public List<MonsterBarItem> MonsterBarItems { get; set; } = new List<MonsterBarItem>();
        public List<MonsterBarPlayer> MonsterBarPlayers { get; set; } = new List<MonsterBarPlayer>();
        public UserLeagueTeam UserLeagueTeam { get; set; } = null;

        public string ColumnTitle(bool isCompact = false)
        {
            if (isCompact)
                return "MBar";
            else
                return "MonsterBar";
        }

        public string Tooltip
        {
            get
            {
                string tt = "MonsterBar: Shows Value Colors for ";
                foreach (var item in MonsterBarItems)
                {
                    tt += item.Description + ", ";
                }

                tt = tt.Trim();

                tt = tt.Substring(0, tt.Length - 1);

                tt += ". Also shows # of games";
                if (MeasureText.Length > 0)
                    tt += " and " + MeasureText;
                tt += ".";

                tt += " MonsterBot uses blue for top player, purple playable, and gray not playable.";

                return tt;
            }

        }
    }
}
