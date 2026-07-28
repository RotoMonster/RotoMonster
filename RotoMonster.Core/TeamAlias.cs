using System;
using System.Collections.Generic;
using System.Text;

namespace RotoMonster.Core
{
    public class TeamAlias
    {
        public int TeamId { get; set; }
        public string Alias { get; set; }

        public Team Team { get; set; }
    }
}
