using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RotoMonster.Core
{
    public class PlayerAnalysisGame
    {
        public SeasonPlayer SeasonPlayer { get; set; }
        public Game Game { get; set; }
        public ValuePlayer OpponentEasePlayer { get; set; }
        public List<PlayerGameState> PlayerGameStates { get; set; }
    }
}
