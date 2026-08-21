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
        /// Loads every league's children in a fixed number of queries rather than
        /// one set per league. Parent rows stay AsNoTracking to match the previous
        /// behaviour - a tracked parent would revive the dead LastSelectedDate write
        /// in SelectUserLeague as a side effect.
        /// </summary>
        private async Task HydrateUserLeaguesAsync(List<UserLeague> leagues)
        {
            if (leagues == null || leagues.Count == 0)
                return;

            var ids = leagues.Select(l => l.Id).ToList();

            var rosterSpots = (await (from ars in db.UserLeagueActiveRosterSpots
                                          .Include(i => i.ActiveRosterSpot)
                                              .ThenInclude(t => t.ActiveRosterSpotPositions)
                                              .ThenInclude(t2 => t2.Position)
                                      where ids.Contains(ars.UserLeagueId)
                                      orderby ars.ActiveRosterSpot.DisplayOrder
                                      select ars).ToListAsync()).ToLookup(x => x.UserLeagueId);

            var categories = (await (from ulc in db.UserLeagueCategories
                                         .Include(i => i.Category).ThenInclude(t => t.PlayerType)
                                         .Include(i => i.Category).ThenInclude(t => t.WeightCategory)
                                         .Include(i => i.Category).ThenInclude(t => t.CategoryPerValues)
                                     where ids.Contains(ulc.UserLeagueId)
                                     orderby ulc.Category.DisplayOrder
                                     select ulc).ToListAsync()).ToLookup(x => x.UserLeagueId);

            var playerTypes = (await (from pt in db.UserLeaguePlayerTypes
                                          .Include(i => i.PlayerType).Include(i => i.CategoriesString)
                                      where ids.Contains(pt.UserLeagueId)
                                      orderby pt.PlayerType.DisplayOrder
                                      select pt).ToListAsync()).ToLookup(x => x.UserLeagueId);

            var teams = await (from t in db.UserLeagueTeams.AsNoTracking()
                               where ids.Contains(t.UserLeagueId)
                               orderby t.Title
                               select t).ToListAsync();

            var teamPlayers = (await (from p in db.UserLeagueTeamPlayers.AsNoTracking()
                                          .Include(i => i.Player)
                                      join team in db.UserLeagueTeams on p.UserLeagueTeamId equals team.Id
                                      where ids.Contains(team.UserLeagueId)
                                      select p).Include(t => t.UserLeagueTeam).ToListAsync())
                                          .ToLookup(p => p.UserLeagueTeamId);

            foreach (var t in teams)
                t.UserLeagueTeamPlayers = teamPlayers[t.Id].ToList();

            var teamsByLeague = teams.ToLookup(t => t.UserLeagueId);

            foreach (var league in leagues)
            {
                league.UserLeagueActiveRosterSpots = rosterSpots[league.Id].ToList();
                league.UserLeagueCategories = categories[league.Id].ToList();
                league.UserLeaguePlayerTypes = playerTypes[league.Id].ToList();
                league.UserLeagueTeams = teamsByLeague[league.Id].ToList();
            }
        }

        public async Task<List<UserLeague>> GetUserLeaguesAsync(string userId)
        {
            if (userId == null)
                return new List<UserLeague>();

            var leagues = await (from l in db.UserLeagues.AsNoTracking()
                                 where l.SeasonId == GetDefaultSeason().Id && l.UserId == userId
                                 orderby l.DisplayTitle, l.Title
                                 select l).Include(a => a.FantasyProvider).ToListAsync();

            await HydrateUserLeaguesAsync(leagues);

            return leagues;
        }

        public async Task<List<UserLeague>> GetTrackedUserLeaguesAsync(string userId)
        {
            if (userId == null)
                return new List<UserLeague>();

            var leagues = await (from l in db.UserLeagues.AsNoTracking()
                                 where l.SeasonId == GetDefaultSeason().Id && l.UserId == userId && l.TrackLeague
                                 orderby l.DisplayTitle, l.Title
                                 select l).Include(a => a.FantasyProvider).ToListAsync();

            await HydrateUserLeaguesAsync(leagues);

            return leagues;
        }

        public async Task<UserLeague> GetUserLeagueAsync(string userId, int id)
        {
            if (userId == null || id == 0)
                return null;

            var leagues = await (from l in db.UserLeagues.AsNoTracking()
                                 where l.SeasonId == GetDefaultSeason().Id && l.UserId == userId && l.Id == id
                                 select l).Include(a => a.FantasyProvider).ToListAsync();

            await HydrateUserLeaguesAsync(leagues);

            return leagues.FirstOrDefault();
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

            if (selectThisUserLeague != null && userId != null)
            {
                var latest = await (from x in db.UserLeagues
                                    where x.UserId == userId
                                    select x.LastSelectedDate).MaxAsync();

                if (selectThisUserLeague.LastSelectedDate == null
                    || (latest != null && selectThisUserLeague.LastSelectedDate < latest))
                {
                    var now = DateTime.UtcNow;
                    var leagueId = selectThisUserLeague.Id;

                    await db.UserLeagues
                        .Where(x => x.Id == leagueId)
                        .ExecuteUpdateAsync(s => s.SetProperty(x => x.LastSelectedDate, now));

                    selectThisUserLeague.LastSelectedDate = now;
                }
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
        // ---------------------------------------------------------------
        // Drafts and display columns - async twins
        //
        // These two keep their sync versions because they have callers outside
        // PlayerRankings: AddDraft is used by UserLeagues/Import, and
        // GetDisplayColumns by DisplaySettings. The sync versions go once
        // those pages are converted.
        // ---------------------------------------------------------------

        public async Task<List<DisplayColumn>> GetDisplayColumnsAsync(string userId)
        {
            if (userId == null)
                return new List<DisplayColumn>();

            var displayColumns = new List<DisplayColumn>();

            var columns = await (from c in db.UserOptionTypes
                                 where c.OptionGroup == "DisplayColumn"
                                 where c.IsEnabled
                                 orderby c.DisplayOrder
                                 select c).ToListAsync();

            var userSettings = await (from u in db.UserOptions where u.UserId == userId select u).ToListAsync();

            foreach (var column in columns)
            {
                var displayColumn = new DisplayColumn();
                displayColumn.UserOptionType = column;
                var userSetting = (from u in userSettings where u.UserOptionTypeId == column.Id select u).FirstOrDefault();
                if (userSetting != null)
                    displayColumn.IsSelected = userSetting.ValueBool.GetValueOrDefault(false);
                else
                    displayColumn.IsSelected = column.DefaultValueBool.GetValueOrDefault(false);
                displayColumns.Add(displayColumn);
            }

            return displayColumns;
        }

        /// <summary>
        /// NOTE: two SaveChanges calls in sequence, matching the sync version.
        /// They must stay sequential - the second depends on draft.Id being
        /// populated by the first.
        /// </summary>
        public async Task<Draft> AddDraftAsync(Draft draft)
        {
            if (draft == null || draft.DraftPlayers.Count == 0)
                return draft;

            var match = await (from d in db.Drafts.AsNoTracking()
                               where d.FantasyProviderId == draft.FantasyProviderId
                                  && d.ProviderLeagueId == draft.ProviderLeagueId
                               select d).FirstOrDefaultAsync();

            if (match == null)
            {
                db.Drafts.Add(draft);
                await db.SaveChangesAsync();

                foreach (var pt in draft.DraftPlayerTypes)
                {
                    pt.DraftId = draft.Id;
                    db.DraftPlayerTypes.Add(pt);
                }
                await db.SaveChangesAsync();
            }

            return draft;
        }
        // ---------------------------------------------------------------
        // Articles
        // ---------------------------------------------------------------

        public async Task<Article> GetArticleAsync(int articleId)
        {
            return await (from a in db.Articles where a.Id == articleId select a).FirstOrDefaultAsync();
        }
        // ---------------------------------------------------------------
        // Writes - display settings and user leagues
        //
        // Where the sync version loops over a query while removing, these
        // materialize with ToListAsync first. Same effect; async cannot
        // enumerate a live query and mutate inside the loop.
        // ---------------------------------------------------------------

        public async Task<int> CommitAsync()
        {
            return await db.SaveChangesAsync();
        }

        public async Task<List<DisplayColumn>> UpdateDisplayColumnsAsync(string userId, List<DisplayColumn> displayColumns)
        {
            var userOptions = await (from u in db.UserOptions.Include(i => i.UserOptionType)
                                     where u.UserId == userId && u.UserOptionType.OptionGroup == "DisplayColumn"
                                     select u).ToListAsync();

            foreach (var userOption in userOptions)
                db.Remove(userOption);
            await db.SaveChangesAsync();

            foreach (var displayColumn in displayColumns)
            {
                var userOption = new UserOption();
                userOption.UserId = userId;
                userOption.UserOptionTypeId = displayColumn.UserOptionType.Id;
                userOption.ValueBool = displayColumn.IsSelected;
                db.Add(userOption);
            }
            await db.SaveChangesAsync();

            return displayColumns;
        }

        public async Task<List<UserDisplayCategory>> UpdateUserDisplayCategoriesAsync(string userId, List<UserDisplayCategory> userDisplayCategories)
        {
            var existing = await (from udc in db.UserDisplayCategories where udc.UserId == userId select udc).ToListAsync();
            foreach (var u in existing)
                db.UserDisplayCategories.Remove(u);
            await db.SaveChangesAsync();

            foreach (var udc in userDisplayCategories)
                db.UserDisplayCategories.Add(udc);
            await db.SaveChangesAsync();

            return userDisplayCategories;
        }

        /// <summary>
        /// Hard delete across six child tables then the league itself. The order
        /// and the two-stage SaveChanges match the sync version exactly.
        /// </summary>
        public async Task DeleteUserLeagueAsync(int userLeagueId)
        {
            foreach (var ulc in await (from x in db.UserLeagueCategories where x.UserLeagueId == userLeagueId select x).ToListAsync())
                db.Remove(ulc);
            foreach (var ars in await (from x in db.UserLeagueActiveRosterSpots where x.UserLeagueId == userLeagueId select x).ToListAsync())
                db.Remove(ars);
            foreach (var pt in await (from x in db.UserLeaguePlayerTypes where x.UserLeagueId == userLeagueId select x).ToListAsync())
                db.Remove(pt);
            foreach (var err in await (from x in db.UserLeagueImportErrors where x.UserLeagueId == userLeagueId select x).ToListAsync())
                db.Remove(err);
            foreach (var o in await (from x in db.UserLeagueMissingPlayers where x.UserLeagueId == userLeagueId select x).ToListAsync())
                db.Remove(o);
            foreach (var o in await (from x in db.UserLeagueWaiverPlayers where x.UserLeagueId == userLeagueId select x).ToListAsync())
                db.Remove(o);
            await db.SaveChangesAsync();

            UserLeague ul = await (from u in db.UserLeagues where u.Id == userLeagueId select u).FirstOrDefaultAsync();
            if (ul != null)
            {
                db.UserLeagues.Remove(ul);
                await db.SaveChangesAsync();
            }
        }

        public async Task SetUserLeagueTrackAsync(int userLeagueId, bool trackLeague)
        {
            var userLeague = await (from ul in db.UserLeagues
                                    where ul.Id == userLeagueId
                                    select ul).FirstOrDefaultAsync();

            if (userLeague == null) return;

            userLeague.TrackLeague = trackLeague;
            await db.SaveChangesAsync();
        }

        public async Task<UserLeague> UpdateUserLeagueAsync(UserLeague userLeague)
        {
            userLeague.FillUserLeagueCategoriesCode(GetCategories());
            db.Update(userLeague);
            await db.SaveChangesAsync();
            UpdateUserLeagueUpdatedDate(userLeague.Id, DateTime.UtcNow, false);

            foreach (var ulc in await (from x in db.UserLeagueCategories where x.UserLeagueId == userLeague.Id select x).ToListAsync())
                db.Remove(ulc);
            foreach (var ars in await (from x in db.UserLeagueActiveRosterSpots where x.UserLeagueId == userLeague.Id select x).ToListAsync())
                db.Remove(ars);
            foreach (var pt in await (from x in db.UserLeaguePlayerTypes where x.UserLeagueId == userLeague.Id select x).ToListAsync())
                db.Remove(pt);
            await db.SaveChangesAsync();

            foreach (var ulc in userLeague.UserLeagueCategories)
            {
                ulc.UserLeagueId = userLeague.Id;
                db.UserLeagueCategories.Add(ulc);
            }
            foreach (var ars in userLeague.UserLeagueActiveRosterSpots)
            {
                ars.UserLeagueId = userLeague.Id;
                db.UserLeagueActiveRosterSpots.Add(ars);
            }
            foreach (var pt in userLeague.UserLeaguePlayerTypes)
            {
                pt.UserLeagueId = userLeague.Id;
                pt.CategoriesStringId = GetCategoriesString(pt.CategoriesCode1).Id;
                db.UserLeaguePlayerTypes.Add(pt);
            }

            await db.SaveChangesAsync();

            return userLeague;
        }

        public async Task<UserLeague> AddUserLeagueAsync(UserLeague userLeague)
        {
            userLeague.CreatedDate = DateTime.UtcNow;
            userLeague.FillUserLeagueCategoriesCode(GetCategories());
            db.UserLeagues.Add(userLeague);
            await db.SaveChangesAsync();
            UpdateUserLeagueUpdatedDate(userLeague.Id, userLeague.CreatedDate.GetValueOrDefault(), false);

            foreach (var ulc in userLeague.UserLeagueCategories)
            {
                ulc.UserLeagueId = userLeague.Id;
                db.UserLeagueCategories.Add(ulc);
            }
            foreach (var ars in userLeague.UserLeagueActiveRosterSpots)
            {
                ars.UserLeague = null;
                ars.UserLeagueId = userLeague.Id;
                db.UserLeagueActiveRosterSpots.Add(ars);
            }
            foreach (var ult in userLeague.UserLeagueTeams)
            {
                ult.UserLeague = null;
                ult.Id = 0;
                ult.UserLeagueId = userLeague.Id;
                db.UserLeagueTeams.Add(ult);
            }
            foreach (var pt in userLeague.UserLeaguePlayerTypes)
            {
                pt.UserLeague = null;
                pt.UserLeagueId = userLeague.Id;
                pt.CategoriesStringId = GetCategoriesString(pt.CategoriesCode1).Id;
                db.UserLeaguePlayerTypes.Add(pt);
            }
            foreach (var err in userLeague.UserLeagueImportErrors)
            {
                err.UserLeague = null;
                err.UserLeagueId = userLeague.Id;
                db.UserLeagueImportErrors.Add(err);
            }
            await db.SaveChangesAsync();

            return userLeague;
        }

        public async Task<UserLeague> GetNewCustomUserLeagueAsync()
        {
            var defaultLeague = await GetDefaultUserLeagueAsync();
            defaultLeague.Id = 0;
            defaultLeague.Title = "New Custom League";
            defaultLeague.DisplayTitle = defaultLeague.Title;
            defaultLeague.ProviderLeagueId = "";
            defaultLeague.IsProLeague = false;

            defaultLeague.UserLeagueTeams.Clear();

            defaultLeague.FantasyProvider = null;

            return defaultLeague;
        }
        // ---------------------------------------------------------------
        // Logs - raw ADO, not EF
        //
        // These bypass EF entirely and run SQL against the _Logs table, so the
        // async form uses the reader's own async methods rather than any EF
        // extension. Note the sync version calls OpenConnection without
        // checking whether the connection is already open; that behaviour is
        // preserved here rather than fixed.
        // ---------------------------------------------------------------

        public async Task<List<LogItem>> GetLogItemsAsync(string filterLevel)
        {
            var logItems = new List<LogItem>();

            using (var command = db.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = "SELECT Id, Message, MessageTemplate, [Level], TimeStamp, Exception, Properties From _Logs ORDER BY TimeStamp DESC";
                await db.Database.OpenConnectionAsync();

                using (var result = await command.ExecuteReaderAsync())
                {
                    while (await result.ReadAsync())
                    {
                        if (filterLevel != null && filterLevel.Length > 0 && filterLevel != (string)result["Level"])
                            continue;

                        var logItem = new LogItem();
                        logItem.Id = (int)result["Id"];
                        logItem.Level = result["Level"] != DBNull.Value ? (string)result["Level"] : "";
                        logItem.Message = result["Message"] != DBNull.Value ? (string)result["Message"] : "";
                        logItem.MessageTemplate = result["MessageTemplate"] != DBNull.Value ? (string)result["MessageTemplate"] : "";
                        logItem.TimeStamp = (DateTime)result["TimeStamp"];
                        logItem.Exception = result["Exception"] != DBNull.Value ? (string)result["Exception"] : "";
                        logItem.Properties = result["Properties"] != DBNull.Value ? (string)result["Properties"] : "";
                        logItems.Add(logItem);
                    }
                }
            }

            return logItems;
        }

        public async Task ClearLogItemsAsync()
        {
            using (var command = db.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = "DELETE _Logs";
                await db.Database.OpenConnectionAsync();
                await command.ExecuteNonQueryAsync();
            }
            ClearCache();
        }
    }
}
