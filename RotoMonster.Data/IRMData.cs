using Microsoft.AspNetCore.Mvc.Rendering;
using RotoMonster.Core;
using RotoMonster.Core.Libs;
using RotoMonster.Data.Libs;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text;

namespace RotoMonster.Data
{
    public partial interface IRMData
    {
        int ClearCache();
        int RemoveCacheItem(string cacheId);
        int RemoveCacheItems(string cacheId);

        IEnumerable<Player> GetPlayerByName(string name);
        List<string> AutoCompletePlayerSearch(string term);
        Player GetById(int playerId);
        Player Update(Player updatedPlayer);
        Player Add(Player newPlayer);
        Player Delete(int playerId);
        int GetCountOfPlayers();

        List<Season> GetSeasons();
        Season GetSeason(int SeasonId);
        Season GetDefaultSeason();
        bool IsSeasonComplete(Season season);
        Season GetPreviousSeason(int maxYear);

        // dates and date ranges
        DateTime GetActivePeriodStartDate(Season season, int weeksBack = 0);
        DateTime GetPeriod(Season season, int weeksBack = 0);
        DateTime GetCurrentGameDate(Season season);
        DateTime GetStartedGameDate(Season season);
        int GetCurrentWeekNumber(Season season);

        DateTime GetLiveGameDate(Season season);
        DateTime GetLiveStartGameDate(Season season);
        DateTime GetLiveEndGameDate(Season season);

        DateTime GetUpcomingGamesStartDate(Season season);
        DateTime GetUpcomingGamesEndDate(Season season);

        Player FindPlayer(string firstName, string lastName, DateTime birthdate, bool birthdateIsValid = true);

        List<Player> GetPlayers();
        Player GetPlayer(int playerId);
        List<Team> GetTeams();
        Team GetTeam(string code);

        List<PerValue> GetPerValues(int playerTypeId);
        PerValue GetDefaultPerValue(int playerTypeId);
        PerValue GetDefaultDisplayPerValue(int playerTypeId);
        PerValue GetSkillPerValue(int playerTypeId);
        PerValue GetPerGamePerValue(int playerTypeId);
        PerValue GetTotalPerValue(int playerTypeId);
        List<PlayerType> GetPlayerTypes();
        PlayerType GetPlayerType(int playerTypeId);
        PlayerType GetPlayerType(string playerTypeTitle);

        PlayerType GetDefaultPlayerType();
        SeasonPlayer GetSeasonPlayer(int playerId);
        SeasonPlayer GetSeasonPlayer(int playerId, PlayerType playerType, Season season);
        List<SeasonPlayer> GetSeasonPlayers(Season season, PlayerType playerType);
        List<SeasonPlayer> GetAllSeasonPlayers(Season season);
        List<Game> GetGames(Season season);
        Game GetGame(int gameId);
        List<Game> GetGames(Season season, Team team);
        List<Game> GetGames(Season season, DateTime startDate, DateTime endDate);

        Game NextGame(Season season);
        TimeSpan TimeUntilNextGame(Season season);

        Category GetStartsCategory(int playerTypeId);
        Category GetGamesCategory(int playerTypeId);
        Category GetMeasureCategory(int playerTypeId);
        List<CategorySetting> GetDefaultCategorySettings(PlayerType playerType);
        List<Category> GetDisplayCategories();
        List<Category> GetDisplayCategories(PlayerType playerType);
        List<PlayerDefaultPosition> PlayerDefaultPositions { get; }
        List<PositionSourcePlayer> GetPlayerSeasonPositions(FantasyProvider provider, Season season);

        List<PositionSourcePlayer> GetPlayerPositionSourcePlayers(FantasyProvider provider, Player player, Season season);
        List<Position> GetPositionSourcePositions(PositionSource positionSource);
        PositionSource GetPositionSource(FantasyProvider fantasyProvider);
        List<Position> GetPositions();
        List<Position> GetActualPositions(PlayerType playerType);

        List<StatPlayer> GetTeamStatPlayers(PlayerType playerType, int seasonId, DateTime startDate, DateTime endDate);
        List<StatPlayer> GetOpposingTeamStatPlayers(PlayerType playerType, int seasonId, DateTime startDate, DateTime endDate);
        List<ValuePlayer> GetOpposingTeamValuePlayers(PlayerType playerType, Season season, DateTime startDate, DateTime endDate, List<CategorySetting> categorySettings, string scoringSystem);
        ActiveRosterSpot GetEaseActiveRosterSpot(Position position);
        List<ValuePlayer> GetTeamEaseValuePlayers(PlayerType playerType, Season season, DateTime startDate, DateTime endDate, List<CategorySetting> categorySettings, string scoringSystem);
        List<StatPlayer> GetStatPlayers(PlayerType playerType, int seasonId, DateTime startDate, DateTime endDate, bool finishedOnly, Game exactGame = null, bool skipCache = false);
        List<PlayerGamePosition> GetPlayerGamePositions(PlayerType playerType, int seasonId, DateTime startDate, DateTime endDate);

        List<GameLogGame> GetPlayerStatPlayerGameLog(UserLeague userLeague, Player player, PlayerType playerType, PerValue perGamePerValue, ValueAverages perGameValueAverages, Season season, List<ValuePlayer> teamEaseValuePlayers);

        GetPositionValuePlayersResult GetPositionValuePlayers(
            PlayerType playerType,
            List<ValuePlayer> valuePlayers,
            UserLeague userLeague,
            List<Position> positionSourcePositions,
            List<PositionSourcePlayer> positionSourcePlayers,
            List<OwnershipPlayer> ownershipPlayers);

        List<ValuePlayer> GetValuePlayers(
            PlayerType playerType,
            Season season,
            DateTime startDate,
            DateTime endDate,
            int pastGames,
            List<CategorySetting> categorySettings,
            string scoringSystem,
            PerValue perValue,
            int leagueSize,
            bool finishedOnly,
            out ValueAverages outValueAverages);

        MonsterBar GetMonsterBar(PlayerType playerType,
            Season season,
            List<CategorySetting> categorySettings,
            string scoringSystem,
            PerValue perValue,
            int leagueSize,
            int activeSize);

        List<BoxScorePlayer> GetBoxScorePlayers(Season season, Game game, bool onlyPlayed);

        Sport Sport { get; }
        ISportDbLib GetSportDbLib();

        UserLeague SelectUserLeague(string userId, UserLeague selectThisUserLeague);
        List<UserLeague> GetUserLeagues(string userId);
        List<UserLeague> GetTrackedUserLeagues(string userId);
        List<UserLeague> GetUserLeagues();
        UserLeague GetUserLeague(string userId, int id);
        UserLeague GetUserLeague(int id);
        UserLeague GetDefaultUserLeague();
        UserLeague GetNewCustomUserLeague();
        List<int> GetUserLeagueIdsWithCategoriesCode(string categoriesCode, Season season);
        List<int> GetUserLeagueIdsWithNoWaivers(Season season);
        List<int> GetAuctionUserLeagueIds(Season season);

        List<UserLeagueTeam> GetUserLeagueTeams(UserLeague userLeague);
        List<UserLeagueTeamPlayer> GetUserLeagueTeamPlayers(UserLeague userLeague);
        List<string> GetValidCategoryCodes(PlayerType playerType);

        UserLeague UpdateUserLeague(UserLeague league);
        void UpdateUserLeagueTeams(int userLeagueId,
            List<UserLeagueTeam> userLeagueTeams,
            List<UserLeagueMissingPlayer> userLeagueMissingPlayers,
            List<UserLeagueWaiverPlayer> userLeagueWaiverPlayers);

        List<UserLeagueWaiverPlayer> GetUserLeagueWaiverPlayers(UserLeague userLeague);

        List<CategorySetting> GetUserLeagueCategorySettings(UserLeague userLeague, PlayerType playerType);
        int GetUserLeagueLeagueSize(UserLeague userLeague, PlayerType playerType);
        List<PositionSourcePlayer> GetUserLeagueSeasonPlayerPositions(UserLeague userLeague, Season season);
        List<PlayerDefaultPosition> GetPlayerDefaultPositions();
        PlayerDefaultPosition GetPlayerDefaultPosition(int playerId);
        List<DisplayActiveRosterSpot> GetDisplayActiveRosterSpots(List<UserLeagueActiveRosterSpot> userLeagueActiveRosterSpots, List<Position> positionSourcePositions);

        string GetUserLeagueScoringSystem(UserLeague userLeague);
        FantasyProvider GetFantasyProvider(int id);
        FantasyProvider GetFantasyProvider(string providerName);
        FantasyProvider GetDefaultFantasyProvider();
        List<ActiveRosterSpot> GetActiveRosterSpots();
        List<ActiveRosterSpotPosition> GetActiveRosterSpotPositions();
        Task<List<UserLeagueActiveRosterSpot>> GetDefaultUserLeagueActiveRosterSpots();

        List<Category> GetCategories(PlayerType playerType);
        List<Category> GetCategories();
        List<Category> GetValueCategories();
        List<Category> GetPointCategories();
        List<DisplayCategory> GetBeforeDisplayCategories(PlayerType playerType);
        List<DisplayCategory> GetAfterDisplayCategories(PlayerType playerType);
        UserLeague AddUserLeague(UserLeague userLeague);
        void AddUserLeagueMissingPlayers(int userLeagueId, List<UserLeagueMissingPlayer> missingPlayers);
        void UpdateUserLeagueUpdatedDate(int userLeagueId, DateTime updatedDate, bool rostersUpdated);
        void DeleteUserLeague(int userLeagueId);
        Draft AddDraft(Draft draft);
        Task<Draft> GetDraft(FantasyProvider fantasyProvider, string fantasyProviderId);
        void DeleteDraft(int draftId);
        void DeleteDraft(FantasyProvider fantasyProvider, string providerLeagueId);
        List<Draft> GetDrafts(FantasyProvider fantasyProvider);
        List<Draft> GetDrafts(Season season);
        List<DraftPlayer> GetDraftPlayers(Draft draft);
        Task<bool> IsDraftFinished(FantasyProvider fantasy, string fantasyProviderId);

        List<FantasyProviderPlayer> GetFantasyProviderPlayers(FantasyProvider fantasyProvider);

        void FillDisplayPlayerUserLeagueTeams(UserLeague userLeague, List<DisplayPlayer> displayPlayers);

        List<SelectListItem> GetPerValuesSelectItems(PlayerType playerType);
        List<SelectListItem> GetTeamsSelectItems(Season season);
        Task<List<SelectListItem>> GetPlayerFilterSelectItems(UserLeague userLeague);
        List<SelectListItem> GetProjectionSourceSelectItems();
        List<SelectListItem> GetDayOfWeekSelectItems(DateTime startDate, DateTime endDate);

        DateTime GetOwnershipPlayersDate(string categoriesCode, DateTime maxDate);
        List<OwnershipPlayer> GetOwnershipPlayers(string categoriesCode, DateTime gameDate, string lineupFrequency = "");
        List<OwnershipPlayer> GetAllOwnershipPlayers(UserLeague userLeague, DateTime gameDate);
        List<OwnershipPlayer> GetAllDefaultOwnershipPlayers(DateTime gameDate);
        List<OwnershipPlayer> GetOwnershipPlayersWithChange(string categoriesCode, DateTime gameDate, int hoursBack);
        List<OwnershipPlayer> GetTrendingPlayers();
        void FillOwnershipPlayers(string categoriesCode, List<UserLeague> sourceUserLeagues);
        public DateTime GetCurrentOwnershipGameDate(string categoriesCode, bool existingOnly);

        List<UserDisplayCategory> GetUserDisplayCategories(string userId, UserLeague userLeague);
        List<UserDisplayCategory> GetUserDisplayCategories(string userId, UserLeague userLeague, PlayerType playerType);
        List<UserDisplayCategory> GetDefaultDisplayCategories();
        List<UserDisplayCategory> UpdateUserDisplayCategories(string userId, List<UserDisplayCategory> userDisplayCategories);

        List<AdpPlayer> GetAdpPlayers(List<Draft> drafts);
        List<AdpPlayer> GetAdpPlayers(Season season, string categoriesCode, int pastNumberOfDrafts, DateTime earliestDate);
        List<AdpPlayer> GetAdpPlayers(Season season, string categoriesCode, DateTime startDate, DateTime endDate);

        int DeletePlayerInjuries();
        PlayerInjury AddPlayerInjury(PlayerInjury playerInjury);
        int AddPlayerInjuries(List<PlayerInjury> playerInjuries);
        List<PlayerInjury> UpdatePlayerInjuries(List<PlayerInjury> playerInjuries);
        List<PlayerInjury> GetPlayerInjuries();

        PlayerStatusType GetPlayerStatusTypeByName(string name);
        PlayerStatus AddPlayerStatus(PlayerStatus playerStatus);
        PlayerStatus DisablePlayerStatus(int playerId);
        List<PlayerStatus> GetActivePlayerStatuses();
        PlayerStatus GetPlayerActivePlayerStatus(int playerId);

        Player AddPlayer(Player player, bool generateId);
        bool AddPositionSourcePlayer(PositionSourcePlayer positionSourcePlayer);
        bool AddPlayerDefaultPosition(PlayerDefaultPosition playerDefaultPosition);
        bool AddFantasyProviderPlayer(FantasyProviderPlayer fantasyProviderPlayer);
        Game AddGame(Game game);
        void UpdateGame(Game game);

        bool AddSeasonPlayer(SeasonPlayer seasonPlayer);
        int UpdateExtraAnalysisLeagues(List<ExtraAnalysisLeague> extraAnalysisLeagues);
        CategoriesString GetDefaultCategoriesString(PlayerType playerType);
        string GetUserLeagueCategoryCode(UserLeague userLeague, PlayerType playerType);
        void UpdateSeasonPlayerTeam(Season season, int playerId, int teamId);
        CategoriesString GetCategoriesString(string categoriesCode);
        CategoriesString GetCategoriesString(int categoriesStringId);

        List<DepthPlayer> GetDepthPlayers(PlayerType playerType, string categoriesCode, DateTime dateTime, bool sortByActive);

        Task<List<ProjectionPlayer>> GetProjectionPlayers(
            PlayerType playerType,
            Season season,
            DateTime pastStartDate,
            DateTime pastEndDate,
            DateTime projectedStartDate,
            DateTime projectedEndDate,
            List<CategorySetting> categorySettings,
            string scoringSystem,
            PerValue perValue,
            int leagueSize);

        List<LogItem> GetLogItems(string level);
        void ClearLogItems();

        // NBA
        bool AddNBAPlayerGame(NBAPlayerGame playerGame);

        // MLB
        bool AddHitterPlayerGame(MLBHitterGame playerGame);
        bool AddPitcherPlayerGame(MLBPitcherGame playerGame);
        bool MarkGameFinished(int gameId);

        // NFL
        bool AddNFLGame(NFLGame game);
        bool AddNFLOffensiveGame(NFLOffensiveGame pg);
        bool AddNFLKickerGame(NFLKickerGame pg);
        bool AddNFLDefenseGame(NFLDefenseGame pg);

        // NHL
        int ClearNHLPlayerGames(Game game);
        bool AddNHLSkaterGame(NHLSkaterGame game);
        bool AddNHLGoalieGame(NHLGoalieGame game);


        List<NFLGame> GetNFLGames(Season season, DateTime startDate, DateTime endDate);

        void DeleteNFLPlayerGames(Game game);

        List<PlayerGameStateType> GetPlayerGameStateTypes();
        int ClearDatePlayerGameStates(DateTime gameDate);
        bool AddPlayerGameState(PlayerGameState playerGameState);
        List<PlayerGameState> GetPlayerGameStates(DateTime startDate, DateTime endDate);

        List<PlayerGameDate> GetPlayerGameDates(DateTime startDate, DateTime endDate, List<ValuePlayer> teamEaseValuePlayers);

        CompletedTask GetCompletedTask(string taskId);
        CompletedTask AddCompletedTask(CompletedTask completedTask);

        bool AddArticle(Article article);
        void DeleteArticle(int articleId);
        Article GetArticle(int articleId);
        List<Article> GetArticles(DateTime startDate, DateTime dateTime, bool includeAutomatedArticles = true);
        List<Article> GetRecentArticles(int pastDays = 3);
        List<Article> GetPlayerRecentArticles(int playerId);
        List<Article> GetGameArticles(Game game);
        List<GameScoringAlert> GetGameScoringAlerts(Season season, DateTime startDate, DateTime endDate);

        void UpdatePlayerGamePositionCategories(Game game, List<PlayerGamePositionCategory> playerGamePositionCategories);
        List<PlayerPositionPercent> GetPlayerPositionPercents(Season season, DateTime startDate, DateTime endDate, int gameId = 0);

        List<DisplayColumn> GetDisplayColumns(string userId);
        Task<UserDisplayColumns> GetUserDisplayColumns(string userId);

        List<DisplayColumn> UpdateDisplayColumns(string userId, List<DisplayColumn> displayColumns);

        List<Helper> GetHelpers();
        Helper GetHelper(int helperId);

        int Commit();
    }
}
