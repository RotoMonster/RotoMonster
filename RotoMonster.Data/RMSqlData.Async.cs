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
    /// must be awaited SEQUENTIALLY. Running two concurrently on one request
    /// (Task.WhenAll) throws "a second operation was started on this context".
    /// Parallel loading would need IDbContextFactory and a context per operation.
    ///
    /// WHAT IS AND ISN'T HERE: only methods that actually reach the database and
    /// are not served from cache. Cached methods stay sync - on a hit there is no
    /// I/O to await. Methods that walk already-loaded navigation properties stay
    /// sync too. Where a method below calls a cached one (GetGames,
    /// GetSeasonPlayer, GetDefaultCategoriesString, GetSeason), that call stays
    /// sync on purpose.
    ///
    /// Every method here is a faithful translation of its sync twin. Known
    /// inefficiencies are preserved rather than fixed, so the conversion can be
    /// verified as a pure no-op by comparing rendered pages. Fixing them is a
    /// separate pass - the ones spotted are noted in comments.
    /// </summary>
    public partial class RMSqlData
    {
        // ---------------------------------------------------------------
        // Player status and injuries
        // ---------------------------------------------------------------

        /// <summary>
        /// NOTE: calls GetSeasonPlayer once per status inside the loop. That is an
        /// N+1, but GetSeasonPlayer is cached so it is not a database round trip
        /// each time. Preserved as-is.
        /// </summary>
        public async Task<List<PlayerStatus>> GetActivePlayerStatusesAsync()
        {
            var playerStatuses = await (from ps in db.PlayerStatuses.AsNoTracking()
                                        .Include(ps => ps.Player)
                                        .Include(ps => ps.PlayerStatusType)
                                        .Include(ps => ps.PlayerStatusTagType)
                                        where ps.IsActive
                                        orderby ps.DateAdded descending, ps.PlayerId
                                        select ps).ToListAsync();

            var seasonGames = GetGames(GetDefaultSeason());

            foreach (var playerStatus in playerStatuses)
            {
                if (playerStatus.EstimatedReturnDate != null)
                {
                    var seasonPlayer = GetSeasonPlayer(playerStatus.PlayerId);
                    if (seasonPlayer != null)
                    {
                        playerStatus.EstimatedGamesToMiss = (from g in seasonGames
                                                             where g.IncludesTeam(seasonPlayer.TeamId)
                                                                && !g.IsFinished
                                                                && g.GameDate < playerStatus.EstimatedReturnDate
                                                             select g).ToList();
                    }
                }
            }

            return playerStatuses;
        }

        /// <summary>
        /// NOTE: three round trips where one would do - Count, Max, then the
        /// select. Preserved so the async change stays a no-op.
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

        // ---------------------------------------------------------------
        // Positions
        // ---------------------------------------------------------------

        public async Task<List<Position>> GetPositionsAsync()
        {
            return await (from p in db.Positions.AsNoTracking().Include(i => i.PlayerType)
                          select p).ToListAsync();
        }

        public async Task<List<Position>> GetActualPositionsAsync(PlayerType playerType)
        {
            var positions = await GetPositionsAsync();
            return (from p in positions
                    where p.IsActualPosition && p.PlayerType.Id == playerType.Id
                    orderby p.DisplayOrder
                    select p).ToList();
        }

        // ---------------------------------------------------------------
        // User leagues
        // ---------------------------------------------------------------

        public async Task<List<UserLeagueTeam>> GetUserLeagueTeamsAsync(UserLeague userLeague)
        {
            if (userLeague == null)
                return new List<UserLeagueTeam>();

            return await (from t in db.UserLeagueTeams.AsNoTracking()
                          where t.UserLeagueId == userLeague.Id
                          orderby t.Title
                          select t).ToListAsync();
        }

        public async Task<List<UserLeagueTeamPlayer>> GetUserLeagueTeamPlayersAsync(UserLeague userLeague)
        {
            if (userLeague == null)
                return new List<UserLeagueTeamPlayer>();

            return await (from p in db.UserLeagueTeamPlayers.AsNoTracking()
                          .Include(i => i.Player)
                          join team in db.UserLeagueTeams on p.UserLeagueTeamId equals team.Id
                          where team.UserLeagueId == userLeague.Id
                          select p).Include(t => t.UserLeagueTeam).ToListAsync();
        }

        /// <summary>
        /// Five round trips per league. That is the sync behaviour too - not
        /// something introduced here.
        /// </summary>
        public async Task<UserLeague> GetUserLeagueAsync(int id)
        {
            var userLeague = await (from ul in db.UserLeagues.Include(i => i.FantasyProvider).AsNoTracking()
                                    where ul.Id == id
                                    select ul).FirstOrDefaultAsync();

            if (userLeague == null)
                return null;

            userLeague.UserLeagueActiveRosterSpots = await (from ars in db.UserLeagueActiveRosterSpots
                                                            .Include(i => i.ActiveRosterSpot)
                                                                .ThenInclude(t => t.ActiveRosterSpotPositions)
                                                                .ThenInclude(t2 => t2.Position)
                                                            where ars.UserLeagueId == userLeague.Id
                                                            orderby ars.ActiveRosterSpot.DisplayOrder
                                                            select ars).ToListAsync();

            userLeague.UserLeagueCategories = await (from ulc in db.UserLeagueCategories
                                                     .Include(i => i.Category).ThenInclude(t => t.PlayerType)
                                                     .Include(i => i.Category).ThenInclude(t => t.WeightCategory)
                                                     .Include(i => i.Category).ThenInclude(t => t.CategoryPerValues)
                                                     where ulc.UserLeagueId == userLeague.Id
                                                     orderby ulc.Category.DisplayOrder
                                                     select ulc).ToListAsync();

            userLeague.UserLeaguePlayerTypes = await (from pt in db.UserLeaguePlayerTypes
                                                      .Include(i => i.PlayerType).Include(i => i.CategoriesString)
                                                      where pt.UserLeagueId == userLeague.Id
                                                      orderby pt.PlayerType.DisplayOrder
                                                      select pt).ToListAsync();

            userLeague.UserLeagueTeams = await GetUserLeagueTeamsAsync(userLeague);

            var teamPlayers = await GetUserLeagueTeamPlayersAsync(userLeague);
            foreach (var t in userLeague.UserLeagueTeams)
                t.UserLeagueTeamPlayers = (from p in teamPlayers where p.UserLeagueTeamId == t.Id select p).ToList();

            return userLeague;
        }

        public async Task<List<UserLeague>> GetUserLeaguesAsync()
        {
            return await db.UserLeagues.AsNoTracking()
                .Include(a => a.Season)
                .Include(a => a.FantasyProvider)
                .ToListAsync();
        }

        /// <summary>
        /// NOTE: this is an N+1 and a real one - it loads the league list, then
        /// calls GetUserLeagueAsync per league, which is five more queries each.
        /// Preserved from the sync version. Worth revisiting.
        /// </summary>
        public async Task<List<UserLeague>> GetUserLeaguesAsync(string userId)
        {
            if (userId == null)
                return new List<UserLeague>();

            var leagues = await (from l in db.UserLeagues
                                 where l.SeasonId == GetDefaultSeason().Id && l.UserId == userId
                                 orderby l.DisplayTitle, l.Title
                                 select l).Include(a => a.FantasyProvider).ToListAsync();

            var outLeagues = new List<UserLeague>();
            foreach (var league in leagues)
                outLeagues.Add(await GetUserLeagueAsync(league.Id));

            return outLeagues;
        }

        public async Task<List<UserLeague>> GetTrackedUserLeaguesAsync(string userId)
        {
            var leagues = await GetUserLeaguesAsync(userId);
            return (from ul in leagues where ul.TrackLeague select ul).ToList();
        }

        public async Task<UserLeague> GetUserLeagueAsync(string userId, int id)
        {
            if (userId == null || id == 0)
                return null;

            var leagues = await GetUserLeaguesAsync(userId);
            return (from u in leagues where u.Id == id select u).FirstOrDefault();
        }

        public async Task<UserLeague> GetDefaultUserLeagueAsync()
        {
            var userLeague = await (from ul in db.UserLeagues where ul.IsDefault select ul).FirstOrDefaultAsync();

            if (userLeague != null)
                return await GetUserLeagueAsync(userLeague.Id);

            return null;
        }

        /// <summary>
        /// NOTE: this WRITES on a GET. It stamps LastSelectedDate and calls
        /// SaveChanges every time a page loads. Preserved, but worth raising -
        /// a write on every page view is a real cost and a surprise.
        /// </summary>
        public async Task<UserLeague> SelectUserLeagueAsync(string userId, UserLeague selectThisUserLeague)
        {
            if (selectThisUserLeague == null)
            {
                var leagues = await GetUserLeaguesAsync(userId);
                selectThisUserLeague = (from u in leagues
                                        where u.TrackLeague == true
                                        orderby u.LastSelectedDate descending, u.Title ascending
                                        select u).FirstOrDefault();

                if (selectThisUserLeague == null)
                {
                    selectThisUserLeague = (from u in leagues
                                            orderby u.LastSelectedDate descending, u.Title ascending
                                            select u).FirstOrDefault();
                }
            }

            if (selectThisUserLeague != null)
            {
                selectThisUserLeague.LastSelectedDate = DateTime.UtcNow;
                await db.SaveChangesAsync();
            }

            if (selectThisUserLeague == null)
                selectThisUserLeague = await GetDefaultUserLeagueAsync();

            return selectThisUserLeague;
        }

        public async Task<string> GetUserLeagueCategoryCodeAsync(UserLeague userLeague, PlayerType playerType)
        {
            var catString = await (from pt in db.UserLeaguePlayerTypes.Include(i => i.CategoriesString)
                                   where pt.UserLeagueId == userLeague.Id && pt.PlayerTypeId == playerType.Id
                                   select pt.CategoriesString).FirstOrDefaultAsync();

            if (catString == null)
                catString = GetDefaultCategoriesString(playerType);

            return catString.Code;
        }

        // ---------------------------------------------------------------
        // Display player fill
        // ---------------------------------------------------------------

        public async Task FillDisplayPlayerUserLeagueTeamsAsync(UserLeague userLeague, List<DisplayPlayer> displayPlayers)
        {
            if (userLeague == null)
                return;

            var leaguePlayers = await GetUserLeagueTeamPlayersAsync(userLeague);

            foreach (var dp in displayPlayers)
            {
                var tp = (from p in leaguePlayers
                          where p.PlayerId == dp.SeasonPlayer.Player.Id
                          select p).FirstOrDefault();
                if (tp != null)
                {
                    dp.UserLeagueTeam = tp.UserLeagueTeam;
                    dp.IsMyPlayer = (dp.UserLeagueTeam.ProviderId == userLeague.MyProviderTeamId);
                    dp.IsActive = tp.IsActive;
                    dp.IsIR = tp.IsIR;
                }
            }
        }
        // ---------------------------------------------------------------
        // Display categories
        // ---------------------------------------------------------------

        /// <summary>
        /// Async twin rather than an in-place conversion, because five pages call
        /// the sync version and only Players is converted so far. The sync one
        /// goes once the other four are done.
        /// </summary>
        public async Task<List<UserDisplayCategory>> GetUserDisplayCategoriesAsync(string userId, UserLeague userLeague)
        {
            if (userId == null)
                return GetDefaultDisplayCategories();

            var userDisplayCategories = await (from udc in db.UserDisplayCategories.AsNoTracking()
                                               .Include(i => i.Category).ThenInclude(i => i.PlayerType)
                                               .Include(i => i.Category).ThenInclude(i => i.CategoryPerValues)
                                               where udc.UserId == userId
                                               orderby udc.DisplayOrder ascending
                                               select udc).ToListAsync();

            if (userLeague == null)
                return userDisplayCategories;

            var filteredUserDisplayCategories = new List<UserDisplayCategory>();
            foreach (var userDisplayCategory in userDisplayCategories)
            {
                var match = (from c in userLeague.UserLeagueCategories
                             where c.CategoryId == userDisplayCategory.CategoryId
                             select c).FirstOrDefault();
                if (match == null)
                    filteredUserDisplayCategories.Add(userDisplayCategory);
            }

            return filteredUserDisplayCategories;
        }

        public async Task<List<UserDisplayCategory>> GetUserDisplayCategoriesAsync(string userId, UserLeague userLeague, PlayerType playerType)
        {
            if (userId == null)
                return (from dc in GetDefaultDisplayCategories()
                        where dc.Category.PlayerType.Id == playerType.Id
                        orderby dc.Category.DisplayOrder
                        select dc).ToList();

            var userDisplayCategories = await (from udc in db.UserDisplayCategories.AsNoTracking()
                                               .Include(i => i.Category).ThenInclude(i => i.PlayerType)
                                               .Include(i => i.Category).ThenInclude(i => i.CategoryPerValues)
                                               where udc.UserId == userId && udc.Category.PlayerType.Id == playerType.Id
                                               orderby udc.DisplayOrder ascending
                                               select udc).ToListAsync();

            foreach (var lcat in (from ulc in userLeague.UserLeagueCategories
                                  where ulc.Category.PlayerType.Id == playerType.Id
                                  select ulc))
            {
                if (userDisplayCategories.Find(dc => dc.CategoryId == lcat.CategoryId) == null)
                {
                    var userDisplayCategory = new UserDisplayCategory();
                    userDisplayCategory.Category = lcat.Category;
                    userDisplayCategory.CategoryId = lcat.CategoryId;
                    userDisplayCategory.DisplayOrder = lcat.Category.DisplayOrder;
                    userDisplayCategories.Add(userDisplayCategory);
                }
            }

            return (from dc in userDisplayCategories orderby dc.Category.DisplayOrder select dc).ToList();
        }

        /// <summary>
        /// Filters the already-async status list. No query of its own.
        ///
        /// The sync version has exactly one caller, so this could have been an
        /// in-place conversion rather than a twin. Kept as a twin only because
        /// its sync form is still referenced from IRMData.
        /// </summary>
        public async Task<PlayerStatus> GetPlayerActivePlayerStatusAsync(int playerId)
        {
            var statuses = await GetActivePlayerStatusesAsync();

            return (from ps in statuses
                    where ps.PlayerId == playerId
                    orderby ps.DateAdded descending
                    select ps).FirstOrDefault();
        }
        // ---------------------------------------------------------------
        // Waivers, scoring alerts, game states
        // ---------------------------------------------------------------

        public async Task<List<UserLeagueWaiverPlayer>> GetUserLeagueWaiverPlayersAsync(UserLeague userLeague)
        {
            if (userLeague == null)
                return new List<UserLeagueWaiverPlayer>();

            return await (from p in db.UserLeagueWaiverPlayers
                          where p.UserLeagueId == userLeague.Id
                          select p).ToListAsync();
        }

        public async Task<List<GameScoringAlert>> GetGameScoringAlertsAsync(Season season, DateTime startDate, DateTime endDate)
        {
            return await (from s in db.GameScoringAlerts
                          .Include(i => i.Game)
                          .Include(i => i.Player).ThenInclude(i2 => i2.PlayerDefaultPositions)
                          .Include(i => i.Team)
                          .Include(i => i.Category)
                          where !s.Game.IsFinished && s.Game.GameDate >= startDate && s.Game.GameDate <= endDate
                          orderby s.ScoringDate descending, s.Category.DisplayOrder ascending
                          select s).ToListAsync();
        }

        public async Task<List<PlayerGameState>> GetPlayerGameStatesAsync(DateTime startDate, DateTime endDate)
        {
            return await (from p in db.PlayerGameStates.AsNoTracking()
                          .Include(i => i.Player)
                          .Include(i => i.Team)
                          .Include(i => i.Game)
                          .Include(i => i.PlayerGameStateType)
                          join g in db.Games.AsNoTracking() on p.GameId equals g.Id
                          where g.GameDate >= startDate && g.GameDate <= endDate
                          select p).ToListAsync();
        }
    }
}
