using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RotoMonster.Core
{
    public class GetPositionValuePlayersResult
    {
        public List<PositionValuePlayer> PositionValuePlayers { get; set; } = new List<PositionValuePlayer>();
        public Dictionary<int, double> PositionBoostHash { get; set; } = new Dictionary<int, double>();
        public Dictionary<int, double> ActiveRosterSpotBoostHash { get; set; } = new Dictionary<int, double>();
    }
}
