using System;
using System.Collections.Generic;
using System.Text;

namespace RotoMonster.Core
{
    public class UserLeagueTeamPlayer
    {
        public long UserLeagueTeamId { get; set; }
        public int PlayerId { get; set; }
        public bool IsActive { get; set; }
        public bool IsIR { get; set; }
        public int PickNumber { get; set; }
        public int AuctionPrice { get; set; }

        public UserLeagueTeam UserLeagueTeam { get; set; }
        public Player Player { get; set; }

    }
}
