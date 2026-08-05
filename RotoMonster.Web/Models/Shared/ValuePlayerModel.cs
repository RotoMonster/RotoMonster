using System;
using System.Collections.Generic;
using System.Text;
using RotoMonster.Core;

namespace RotoMonster.Models.Shared
{
    public class ValuePlayerModel
    {
        public ValuePlayer ValuePlayer { get; set; } = null;
        public string DisplayFormat { get; set; } = "#####0.00";
        public bool ShowGames { get; set; } = false;
        public bool ShowMPG { get; set; } = false;
    }
}
