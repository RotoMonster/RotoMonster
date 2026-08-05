using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RotoMonster.Core
{
    public class PositionValuePlayer
    {
        public double PositionValue { get; set; } = 0;
        public string PositionValueColor { get; set; } = "";
        public ValuePlayer DefaultValuePlayer { get; set; } // using normal settings
        public Position MostValuablePosition { get; set; }
        public ActiveRosterSpot MostValuableActiveRosterSpot { get; set; }
        public bool IsStartable { get; set; } = false;  // good enough to start
        public bool IsOwnable { get; set; } = false;    // good enough to own
        public double Weight { get; set; } = 1;
    }
}
