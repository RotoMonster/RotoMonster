using System;
using System.Collections.Generic;
using System.Text;

namespace RotoMonster.Core
{
    public class AdpPlayer
    {
        public int PlayerId { get; set; } = 0;
        public string CategoriesCode { get; set; } = "";
        public double Adp { get; set; } = 0;
        public double StdevPick { get; set; } = 0;
        public double MaxPick { get; set; } = 0;
        public double MinPick { get; set; } = 0;
        public double ProjectedLowPick { get; set; } = 0;
        public double ProjectedHighPick { get; set; } = 0;
        public double ColorPercent { get; set; } = 0;
        public double DraftPercent { get; set; }
        public int DraftCount { get; set; }
        public List<double> Picks { get; set; } = new List<double>();

        public double AveragePrice { get; set; } = 0;
        public double StdevPrice { get; set; } = 0;
        public double MaxPrice { get; set; } = 0;
        public double MinPrice { get; set; } = 0;
        public double ProjectedLowPrice { get; set; } = 0;
        public double ProjectedHighPrice { get; set; } = 0;

        public List<double> Prices { get; set; } = new List<double>();
    }
}
