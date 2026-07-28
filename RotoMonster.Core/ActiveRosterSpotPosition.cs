using System;
using System.Collections.Generic;
using System.Text;

namespace RotoMonster.Core
{
    public class ActiveRosterSpotPosition
    {
        public int ActiveRosterSpotId { get; set; }
        public int PositionId { get; set; }

        public Position Position { get; set; }
        public ActiveRosterSpot ActiveRosterSpot { get; set; }
    }
}
