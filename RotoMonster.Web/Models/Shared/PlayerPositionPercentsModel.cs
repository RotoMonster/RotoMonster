using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RotoMonster.Core;

namespace RotoMonster.Models.Shared
{
    public class PlayerPositionPercentsModel
    {
        public List<PlayerPositionPercent> PlayerPositionPercents { get; set; } = null;
        public List<PlayerPositionPercentOver> PlayerPositionPercentOvers { get; set; } = null;
        public List<Position> AllPositions { get; set; }
        public int EstimatedUpside { get; set; } = 0;
        public string EstimatedUpsideColorCode { get; set; } = "";
    }
}
