using System;
using System.Collections.Generic;
using System.Text;

namespace RotoMonster.Core
{
    public class PlayerGameDate
    {
        public SeasonPlayer SeasonPlayer { get; set; }
        public Game Game { get; set; }
        public PlayerGameState PlayerGameState { get; set; }
        public ValuePlayer EaseValuePlayer { get; set; }
    }

}
