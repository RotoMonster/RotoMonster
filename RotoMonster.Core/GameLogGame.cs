using System;
using System.Collections.Generic;
using System.Text;

namespace RotoMonster.Core
{
    public class GameLogGame
    {
        public Player Player { get; set; }
        public Game Game { get; set; }
        public PlayerGameMissed PlayerGameMissed { get; set; }
        public StatPlayer StatPlayer { get; set; }
        public ValuePlayer ValuePlayer { get; set; }
        public ValuePlayer EaseValuePlayer { get; set; }
        public bool IsBreak { get; set; } = false;
    }
}
