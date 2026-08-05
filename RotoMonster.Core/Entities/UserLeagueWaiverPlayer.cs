using System;
using System.Collections.Generic;
using System.Text;

namespace RotoMonster.Core
{
    public class UserLeagueWaiverPlayer
    {
        public int UserLeagueId { get; set; }
        public int PlayerId { get; set; }
        public DateTime AddedDate { get; set; }

        public UserLeague UserLeague { get; set; }
        public Player Player { get; set; }
    }
}
