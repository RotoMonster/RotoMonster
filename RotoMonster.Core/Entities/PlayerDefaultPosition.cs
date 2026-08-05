using System;
using System.Collections.Generic;
using System.Text;

namespace RotoMonster.Core
{
    public class PlayerDefaultPosition
    {
        public int PlayerId { get; set; }
        public int PositionId { get; set; }

        public Player Player { get; set; }
        public Position Position { get; set; }

    }
}
