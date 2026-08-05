using System;
using System.Collections.Generic;
using System.Text;
using RotoMonster.Core;

namespace RotoMonster.Models.Shared
{
    public class PlayerOwnershipChangeModel
    {
        public double CurrentPercent { get; set; }
        public double PercentChange { get; set; }

        public PlayerOwnershipChangeModel(double currentPercent, double percentChange)
        {
            CurrentPercent = currentPercent;
            PercentChange = percentChange;
        }
    }

}
