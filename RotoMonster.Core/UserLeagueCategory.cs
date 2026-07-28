using System;
using System.Collections.Generic;
using System.Text;

namespace RotoMonster.Core
{
    public class UserLeagueCategory
    {
        public int UserLeagueId { get; set; }
        public int CategoryId { get; set; }
        public bool IsActive { get; set; }
        public double Weight { get; set; }
        public double PointsPerStat { get; set; }

        public UserLeague UserLeague { get; set; }
        public Category Category { get; set; }
    }
}
