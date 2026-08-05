using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RotoMonster.Core;

namespace RotoMonster.Models.Shared
{
    public class PositionBoostHashModel
    {
        public Dictionary<int,double> PositionBoostHash { get; set; }
        public Dictionary<int, double> ActiveRosterSpotBoostHash { get; set; }
        public List<Position> Positions { get; set; }
        public List<ActiveRosterSpot> ActiveRosterSpots { get; set; }
    }
}
