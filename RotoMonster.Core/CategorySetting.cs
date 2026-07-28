using System;
using System.Collections.Generic;
using System.Text;

namespace RotoMonster.Core
{
    public class CategorySetting
    {
        public Category Category { get; set; }
        public double PointsPerStat { get; set; }
        public bool IsActive { get; set; }
        public double Weight { get; set; }

        public CategorySetting()
        {
            Weight = 1;
            IsActive = true;
        }

        /**/
    }
}
