using System;
using System.Collections.Generic;
using System.Text;

namespace RotoMonster.Core
{
    public class DisplayActiveRosterSpot
    {
        public PlayerType PlayerType { get; set; } = null;
        public List<Position> Positions { get; set; } = new List<Position>();
        public bool IsDefault { get; set; } = false;
        public int DisplayOrder { get; set; }

        public string Id
        {
            get
            {
                string outId = "";
                foreach (var p in Positions)
                {
                    outId += p.Id.ToString() + "_";
                }

                return outId;
            }
        }

    }
}
