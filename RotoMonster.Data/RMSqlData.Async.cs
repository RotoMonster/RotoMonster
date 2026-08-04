using Microsoft.EntityFrameworkCore;
using RotoMonster.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RotoMonster.Data
{
    /// <summary>
    /// Async implementations. Partial file of RMSqlData, so these share the same
    /// injected RMDBContext and IMemoryCache as everything in RMSqlData.cs - no
    /// DI change, no second context, nothing new to register.
    ///
    /// IMPORTANT: because the DbContext is shared and scoped per request, these
    /// must be awaited SEQUENTIALLY. Running two of them concurrently on one
    /// request (Task.WhenAll) throws "a second operation was started on this
    /// context". If parallel loading is ever wanted, that needs
    /// IDbContextFactory and a separate context per operation.
    /// </summary>
    public partial class RMSqlData
    {
        /// <summary>
        /// Async form of GetPlayerInjuries.
        ///
        /// The sync version makes THREE round trips - Count(), Max(), then the
        /// select. That is preserved here rather than fixed, so the async
        /// conversion can be verified as a pure no-op change. Collapsing it to
        /// one query is a separate improvement worth making afterwards.
        /// </summary>
        public async Task<List<PlayerInjury>> GetPlayerInjuriesAsync()
        {
            if (await db.PlayerInjuries.CountAsync() == 0)
                return new List<PlayerInjury>();

            DateTime? maxDate = await db.PlayerInjuries.MaxAsync(p => p.DownloadDate);
            if (maxDate == null)
                return new List<PlayerInjury>();

            return await (from p in db.PlayerInjuries select p).ToListAsync();
        }

        /// <summary>
        /// Async form of GetUserLeagues().
        /// </summary>
        public async Task<List<UserLeague>> GetUserLeaguesAsync()
        {
            return await db.UserLeagues.AsNoTracking()
                .Include(a => a.Season)
                .Include(a => a.FantasyProvider)
                .ToListAsync();
        }
    }
}
