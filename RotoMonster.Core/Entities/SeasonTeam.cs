using System;
using System.Collections.Generic;
using System.Text;

namespace RotoMonster.Core
{
    public class SeasonTeam
    {
        public int TeamId { get; set; }
        public int SeasonId { get; set; }
        public int DivisionId { get; set; }

        public Team Team { get; set; }
    }
}
