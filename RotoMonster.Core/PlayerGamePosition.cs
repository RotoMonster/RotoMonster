using System;
using System.Collections.Generic;
using System.Text;

namespace RotoMonster.Core
{
    public class PlayerGamePosition
    {
        public int PlayerId { get; set; }
        public int GameId { get; set; }
        public int PositionId { get; set; }
        public int Percent { get; set; } = 0;

        public Player Player { get; set; }
        public Game Game { get; set; }
        public Position Position { get; set; }
    }

}
