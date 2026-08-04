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
    ///   - Cached methods. On a cache hit there is no I/O to await, so async only
    ///     adds state-machine overhead.
    ///   - Methods that take an already-loaded entity and walk its navigation
    ///     properties in memory. GetTeamsSelectItems(season) is the example - it
    ///     reads season.SeasonTeams and never queries.
    ///
    /// The sync version of each method stays exactly where it is. Nothing is
    /// deleted until every page using it has been converted and verified.
    /// </summary>
    public partial interface IRMData
    {
        // Player status and injuries
        Task<List<PlayerStatus>> GetActivePlayerStatusesAsync();
        Task<List<PlayerInjury>> GetPlayerInjuriesAsync();

        // Positions
        Task<List<Position>> GetPositionsAsync();
        Task<List<Position>> GetActualPositionsAsync(PlayerType playerType);

        // User leagues
        Task<List<UserLeagueTeam>> GetUserLeagueTeamsAsync(UserLeague userLeague);
        Task<List<UserLeagueTeamPlayer>> GetUserLeagueTeamPlayersAsync(UserLeague userLeague);
        Task<UserLeague> GetUserLeagueAsync(int id);
        Task<UserLeague> GetUserLeagueAsync(string userId, int id);
        Task<List<UserLeague>> GetUserLeaguesAsync();
        Task<List<UserLeague>> GetUserLeaguesAsync(string userId);
        Task<List<UserLeague>> GetTrackedUserLeaguesAsync(string userId);
        Task<UserLeague> GetDefaultUserLeagueAsync();
        Task<UserLeague> SelectUserLeagueAsync(string userId, UserLeague selectThisUserLeague);
        Task<string> GetUserLeagueCategoryCodeAsync(UserLeague userLeague, PlayerType playerType);

        // Display player fill
        Task FillDisplayPlayerUserLeagueTeamsAsync(UserLeague userLeague, List<DisplayPlayer> displayPlayers);

        // Display categories
        Task<List<UserDisplayCategory>> GetUserDisplayCategoriesAsync(string userId, UserLeague userLeague);
        Task<List<UserDisplayCategory>> GetUserDisplayCategoriesAsync(string userId, UserLeague userLeague, PlayerType playerType);
        Task<PlayerStatus> GetPlayerActivePlayerStatusAsync(int playerId);

        // Waivers, scoring alerts, game states
        Task<List<UserLeagueWaiverPlayer>> GetUserLeagueWaiverPlayersAsync(UserLeague userLeague);
        Task<List<GameScoringAlert>> GetGameScoringAlertsAsync(Season season, System.DateTime startDate, System.DateTime endDate);
        Task<List<PlayerGameState>> GetPlayerGameStatesAsync(System.DateTime startDate, System.DateTime endDate);
        // Drafts and display columns
        Task<List<DisplayColumn>> GetDisplayColumnsAsync(string userId);
        Task<Draft> AddDraftAsync(Draft draft);
        // Articles
        Task<Article> GetArticleAsync(int articleId);
    }
}
