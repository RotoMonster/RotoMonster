using System;
using System.Collections.Generic;
using System.Text;

namespace RotoMonster.Core
{
    public class UserLeagueTeamAnalysis
    {
        public UserLeagueTeam UserLeagueTeam { get; set; }
        public double AverageOwnershipPercent { get; set; }
        public double AverageActivePercent { get; set; }
        public List<UserLeagueTeamAnalysisPlayerType> UserLeagueTeamAnalysisPlayerTypes { get; set; } = new List<UserLeagueTeamAnalysisPlayerType>();
    }

    public class UserLeagueTeamAnalysisPlayerType
    {
        public PlayerType PlayerType { get; set; }
        public int PlayerCount { get; set; } = 0;
        public double AverageOwnershipPercent { get; set; }
        public double AverageActivePercent { get; set; }
    }

}
