using System;
using System.Collections.Generic;
using System.Text;

namespace RotoMonster.Core
{
    public class DisplayGameDates
    {
        public Team Team { get; set; } = null;
        public int GameCount { get; set; } = 0;
        public List<Game> Games { get; set; } = new List<Game>();
        public bool IsBackToBack { get; set; } = false;
    }
}
