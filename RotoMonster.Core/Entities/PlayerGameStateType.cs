using System;
using System.Collections.Generic;
using System.Text;

namespace RotoMonster.Core
{
    public class PlayerGameStateType
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Descrition { get; set; }
        public string Icon { get; set; }
        public string TextColor { get; set; }
        public string BackgroundColor { get; set; }
        public bool IsStarter { get; set; }
        public bool IsProbableStarter { get; set; }
        public bool IsBench { get; set; }
        public bool ShowLockAfterStart { get; set; }
    }
}
