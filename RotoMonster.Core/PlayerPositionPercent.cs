using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RotoMonster.Core
{
    public class PlayerPositionPercent
    {
        public Player Player { get; set; }
        public Position Position { get; set; }
        public float CategoryValue { get; set; }
        public double Percent { get; set; }
        public string PercentColorCode { get; set; }

        public int DisplayPercent
        {
            get
            {
                return (int)Math.Round(Percent, 0);
            }
        }

    }
}
