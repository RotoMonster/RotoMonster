using System;
using System.ComponentModel.DataAnnotations;

namespace RotoMonster.Core
{
    /// <summary>
    /// One matchup in a league's schedule - two teams and the period they meet.
    ///
    /// The teams are the PROVIDER's ids, not UserLeagueTeam ids. A roster
    /// refresh deletes and recreates the team rows, so their ids do not
    /// survive; the provider's do, and are what the schedule comes back with.
    /// Join to UserLeagueTeam.ProviderId to get the names.
    /// </summary>
    public class UserLeagueMatchup
    {
        public long Id { get; set; }

        public int UserLeagueId { get; set; }

        /// <summary>
        /// The scoring period, as the provider numbers it. A week in most
        /// leagues, but Fantrax periods can be days.
        /// </summary>
        public int Period { get; set; }

        [StringLength(50)]
        public string AwayProviderTeamId { get; set; }

        [StringLength(50)]
        public string HomeProviderTeamId { get; set; }

        /// <summary>
        /// A playoff period rather than a regular one, where the provider says
        /// so. Stored now because backfilling it later would mean refetching
        /// every schedule.
        /// </summary>
        public bool IsPlayoff { get; set; }

        public UserLeague UserLeague { get; set; }
    }
}
