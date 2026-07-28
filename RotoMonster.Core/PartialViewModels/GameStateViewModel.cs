using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RotoMonster.Core.PartialViewModels
{
    public class GameStateViewModel
    {
        public Game Game { get; set; }
        public Sport Sport { get; set; }
        public bool ShowOpponent { get; set; } = false;
        public bool ShowScores { get; set; } = false;
        public Team Team { get; set; }
        public bool ShowCompact { get; set; } = false;
    }
}
