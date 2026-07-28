using System;
using System.Collections.Generic;
using System.Text;

namespace RotoMonster.Core.PartialViewModels
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
