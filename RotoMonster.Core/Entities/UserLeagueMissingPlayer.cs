using System;
using System.Collections.Generic;
using System.Text;

namespace RotoMonster.Core
{
    public class UserLeagueMissingPlayer
    {
        public int UserLeagueId { get; set; }
        public string ProviderId { get; set; }
        public string FullName { get; set; }

        public UserLeague UserLeague { get; set; }

    }
}
