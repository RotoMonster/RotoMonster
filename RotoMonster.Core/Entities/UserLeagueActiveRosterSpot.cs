using System;
using System.Collections.Generic;
using System.Text;

namespace RotoMonster.Core
{
    public class UserLeagueActiveRosterSpot
    {
        public int UserLeagueId { get; set; }
        public int ActiveRosterSpotId { get; set; }
        public int NumberOfPlayers { get; set; }

        public UserLeague UserLeague { get; set; }
        public ActiveRosterSpot ActiveRosterSpot { get; set; }
    }
}
