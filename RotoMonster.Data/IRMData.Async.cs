using RotoMonster.Core;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RotoMonster.Data
{
    /// <summary>
    /// Async data methods, kept in a partial file so the async surface is
    /// separated from the 200-odd sync methods in IRMData.cs without needing a
    /// second interface, a second registration, or a second DbContext.
    ///
    /// WHAT BELONGS HERE: methods that actually touch the DbContext AND are not
    /// served from IMemoryCache. Two things that look like candidates but are not:
    ///
    ///   - Cached methods (85 of them). On a cache hit there is no I/O to await,
    ///     so async only adds state-machine overhead.
    ///   - Methods that take an already-loaded entity and walk its navigation
    ///     properties in memory. GetTeamsSelectItems(season) is the example - it
    ///     reads season.SeasonTeams and never queries.
    ///
    /// The sync version of each method stays exactly where it is. Nothing is
    /// deleted until a page has been converted and verified.
    /// </summary>
    public partial interface IRMData
    {
        Task<List<PlayerInjury>> GetPlayerInjuriesAsync();

        Task<List<UserLeague>> GetUserLeaguesAsync();
    }
}
