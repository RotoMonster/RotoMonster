using System;
using System.Collections.Generic;
using System.Text;

namespace RotoMonster.Core
{
    public class PositionSourcePosition
    {
        public int PositionSourceId { get; set; }
        public int PositionId { get; set; }

        public PositionSource PositionSource { get; set; }
        public Position Position { get; set; }
    }
}
