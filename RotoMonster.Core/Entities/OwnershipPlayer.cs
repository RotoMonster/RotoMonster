using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace RotoMonster.Core
{
    public class OwnershipPlayer
    {
        public DateTime GameDate { get; set; }
        public int PlayerId { get; set; }
        public int LeagueSize { get; set; } = 0;
        public int LeagueCount { get; set; } = 0;
        public int OwnCount { get; set; } = 0;
        public int ActiveCount { get; set; } = 0;
        public int IRCount { get; set; } = 0;
        public int CategoriesStringId { get; set; }

        public Player Player { get; set; }
        public CategoriesString CategoriesString { get; set; }

        [NotMapped]
        public double PercentOwnershipChange { get; set; } = 0;

        [NotMapped]
        public double PercentActiveChange { get; set; } = 0;

        [NotMapped]
        public double PercentIRChange { get; set; } = 0;

        [NotMapped]
        public double OwnershipPercent
        {
            get
            {
                if (LeagueCount > 0)
                    return Convert.ToDouble(OwnCount) / Convert.ToDouble(LeagueCount) * 100;
                else
                    return 0;
            }
        }

        [NotMapped]
        public double ActivePercent
        {
            get
            {
                if (LeagueCount > 0)
                    return Convert.ToDouble(ActiveCount) / Convert.ToDouble(LeagueCount) * 100;
                else
                    return 0;
            }
        }

        [NotMapped]
        public double IRPercent
        {
            get
            {
                if (LeagueCount > 0)
                    return Convert.ToDouble(IRCount) / Convert.ToDouble(LeagueCount) * 100;
                else
                    return 0;
            }
        }

    }
}
