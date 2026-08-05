using System;
using System.Collections.Generic;
using System.Text;

namespace RotoMonster.Core
{
    public class PositionSourcePlayer
    {
        public int SeasonId { get; set; }
        public int PositionSourceId { get; set; }
        public int PlayerId { get; set; }
        public int PositionId { get; set; }

        public Position Position { get; set; }
    }
}
