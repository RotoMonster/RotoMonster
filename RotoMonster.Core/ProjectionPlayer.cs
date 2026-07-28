using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RotoMonster.Core
{
    public class ProjectionPlayer
    {
        public ValuePlayer ValuePlayer { get; set; }
        public SeasonPlayer SeasonPlayer { get; set; }
        public List<PlayerPositionPercentOver> OverPlayerPositionPercents { get; set; } = new List<PlayerPositionPercentOver>();
        public double EstimatedUpside { get; set; } = 0;
        public string EstimatedUpsideColorCode { get; set; } = "";
        public int DisplayEstimatedOverPercent
        {
            get
            {
                return (int)Math.Round(EstimatedUpside, 0);
            }
        }
    }
}
