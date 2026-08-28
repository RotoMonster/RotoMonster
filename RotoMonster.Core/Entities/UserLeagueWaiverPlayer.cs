using System;
using System.Collections.Generic;
using System.Text;

namespace RotoMonster.Core
{
    public class UserLeagueWaiverPlayer
    {
        public int UserLeagueId { get; set; }
        public int PlayerId { get; set; }
        /// <summary>
        /// When the player landed on waivers. From the drop transaction where
        /// we have it, rather than when we happened to look.
        /// </summary>
        public DateTime AddedDate { get; set; }

        /// <summary>
        /// The date they can be claimed off waivers.
        ///
        /// Not when they can be played - a player dropped on Monday with a two
        /// day waiver period is claimable on Thursday, but in a weekly league
        /// would not be startable until the following period. Claimable is the
        /// useful one.
        ///
        /// Null where we could not work it out, usually because there is no
        /// drop transaction for them.
        /// </summary>
        public DateTime? WaiverDate { get; set; }

        public UserLeague UserLeague { get; set; }
        public Player Player { get; set; }
    }
}
