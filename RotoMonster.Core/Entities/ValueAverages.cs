using System;
using System.Collections.Generic;
using System.Text;

namespace RotoMonster.Core
{
    public class ValueAverages
    {
        public Dictionary<int, double> Averages = new Dictionary<int, double>();
        public Dictionary<int, double> AverageAs = new Dictionary<int, double>();
        public Dictionary<int, double> Stdevs = new Dictionary<int, double>();

        public double PointsAverageValue { get; set; }
        public double CategoryAverageValue { get; set; }
        public double PointsMinValue { get; set; }
        public double PointsMaxValue { get; set; }
    }
}
