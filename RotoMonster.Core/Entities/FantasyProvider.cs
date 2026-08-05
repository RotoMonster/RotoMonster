using System;
using System.Collections.Generic;
using System.Text;

namespace RotoMonster.Core
{
    public class FantasyProvider
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public bool IsDefault { get; set; }
        public string LeagueURL { get; set; }
        public int DisplayOrder { get; set; }
    }
}
