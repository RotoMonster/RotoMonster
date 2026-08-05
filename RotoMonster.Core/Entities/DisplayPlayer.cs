using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace RotoMonster.Core
{
    public class DisplayPlayer
    {
        public string DisplayTitle { get; set; } = "";
        public bool IndentDisplayTitle { get; set; } = false;
        public bool ShowBottomBorder { get; set; } = false;
        public SeasonPlayer SeasonPlayer { get; set; }
        public StatPlayer StatPlayer { get; set; }
        public ValuePlayer ValuePlayer { get; set; }
        public ProjectionPlayer ProjectionPlayer { get; set; }
        public PositionValuePlayer PositionValuePlayer { get; set; }
        public ValuePlayer OpposingTeamValuePlayer { get; set; }
        public ValuePlayer NextOpposingTeamValuePlayer { get; set; }
        public ValuePlayer CurrentPeriodValuePlayer { get; set; }
        public ValuePlayer PreviousPeriodValuePlayer { get; set; }
        public AdpPlayer AdpPlayer { get; set; }
        public AdpPlayer AllAdpPlayer { get; set; } = null;
        public AdpPlayer DefaultAdpPlayer { get; set; }
        public DraftPlayer DraftPlayer { get; set; }
        public DepthPlayer DepthPlayer { get; set; }
        public GameScoringAlert GameScoringAlert { get; set; }
        public List<DisplayPlayer> HigherDepthInjuredDisplayPlayers { get; set; }
        public List<DisplayValuePlayer> DisplayValuePlayers { get; set; } = new List<DisplayValuePlayer>();
        public List<Position> Positions { get; set; }
        public List<PlayerPositionPercent> PlayerPositionPercents { get; set; }
        public UserLeagueTeam UserLeagueTeam { get; set; }
        public OwnershipPlayer OwnershipPlayer { get; set; }
        public OwnershipPlayer NoWaiverOwnershipPlayer { get; set; }
        public PlayerInjury PlayerInjury { get; set; }
        public PlayerStatus PlayerStatus { get; set; }
        public List<Article> RecentArticles { get; set; }
        public bool IsMyPlayer { get; set; }
        public bool IsMyLastPlayer { get; set; } = false;
        public bool IsActive { get; set; }
        public bool IsIR { get; set; }
        public Game Game { get; set; } = null;
        public PlayerGameState PlayerGameState { get; set; } = null;
        public Game NextGame { get; set; } = null;
        public List<Game> TeamGames { get; set; } = null;
        public List<Game> RemainingWeekGames { get; set; } = null;
        public List<Game> NextWeekGames { get; set; } = null;
        public bool IsBreak { get; set; } = false;
        public bool IsWaiver { get; set; } = false;

        public ValuePlayer ExtraValuePlayer1 { get; set; } = null;
        public ValuePlayer ExtraValuePlayer2 { get; set; } = null;
        public ValuePlayer ExtraValuePlayer3 { get; set; } = null;

        public MonsterBarPlayer MonsterBarGamePlayer { get; set; } = null;
        public MonsterBarPlayer MonsterBarTotalPlayer { get; set; } = null;

        public ActiveRosterSpot ActiveRosterSpot { get; set; }
        public MonsterBotPlayer MonsterBotPlayer { get; set; }
        public List<UserLeague> AvailableInUserLeagues { get; set; } = null;
        public List<UserLeague> OwnedInUserLeagues { get; set; } = null;
        public List<PlayerGameDate> PlayerGameDates { get; set; } = null;
        public List<PlayerAnalysisGame> PlayerAnalysisGames { get; set; } = null;
        public int TeamGameCount { get; set; } = 0;
        public int PlayerGameCount { get; set; } = 0;

    }
}
