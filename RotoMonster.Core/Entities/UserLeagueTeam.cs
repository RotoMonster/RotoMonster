using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace RotoMonster.Core
{
    public class UserLeagueTeam
    {
        public UserLeagueTeam()
        {
            UserLeagueTeamPlayers = new List<UserLeagueTeamPlayer>();
        }

        public long Id { get; set; }
        public int UserLeagueId { get; set; }
        public int TeamNumber { get; set; }
        public string Title { get; set; }
        public string ProviderId { get; set; }
        public List<UserLeagueTeamPlayer> UserLeagueTeamPlayers { get; set; }

        public UserLeague UserLeague { get; set; }

        [NotMapped]
        public int DraftOrder { get; set; } = 0;

        public UserLeagueTeamAnalysis GetUserLeagueTeamAnalysis(List<PlayerType> playerTypes, List<SeasonPlayer> seasonPlayers, List<OwnershipPlayer> ownershipPlayers, bool useActiveOnly, bool excludeIR)
        {
            var teamAnalysis = new UserLeagueTeamAnalysis();
            teamAnalysis.UserLeagueTeam = this;

            double allOwnershipPercent = 0;
            double allTotalPlayers = 0;

            foreach (var playerType in playerTypes)
            {
                double totalOwnershipPercent = 0;
                double totalActivePercent = 0;
                double totalActivePlayers = 0;
                double totalPlayers = 0;

                foreach (var userLeagueTeamPlayer in UserLeagueTeamPlayers)
                {
                    var seasonPlayer = (from sp in seasonPlayers where sp.PlayerId == userLeagueTeamPlayer.PlayerId select sp).FirstOrDefault();
                    if (seasonPlayer != null && seasonPlayer.PlayerTypeId == playerType.Id)
                    {
                        if (!excludeIR || !userLeagueTeamPlayer.IsIR)
                        {
                            var ownershipPlayer = (from op in ownershipPlayers where op.PlayerId == userLeagueTeamPlayer.PlayerId select op).FirstOrDefault();
                            if (ownershipPlayer != null)
                            {
                                if (userLeagueTeamPlayer.IsActive || !useActiveOnly)
                                {
                                    double activePercent = (ownershipPlayer == null ? 0 : ownershipPlayer.ActivePercent);
                                    totalActivePercent += activePercent;
                                    totalActivePlayers++;
                                }
                                double ownershipPercent = (ownershipPlayer == null ? 0 : ownershipPlayer.OwnershipPercent);
                                totalOwnershipPercent += ownershipPercent;
                                totalPlayers++;
                                allOwnershipPercent += ownershipPercent;
                                allTotalPlayers++;
                            }
                        }
                    }
                }

                if (totalActivePlayers > 0)
                {
                    var userLeagueTeamAnalysisPlayerType = new UserLeagueTeamAnalysisPlayerType();
                    userLeagueTeamAnalysisPlayerType.PlayerType = playerType;
                    userLeagueTeamAnalysisPlayerType.PlayerCount = System.Convert.ToInt32(totalPlayers);
                    userLeagueTeamAnalysisPlayerType.AverageOwnershipPercent = (totalPlayers == 0 ? 0 : totalOwnershipPercent / totalPlayers);
                    userLeagueTeamAnalysisPlayerType.AverageActivePercent = (totalActivePlayers == 0 ? 0 : totalActivePercent / totalActivePlayers);
                    teamAnalysis.UserLeagueTeamAnalysisPlayerTypes.Add(userLeagueTeamAnalysisPlayerType);
                }
                else
                {
                    var userLeagueTeamAnalysisPlayerType = new UserLeagueTeamAnalysisPlayerType();
                    userLeagueTeamAnalysisPlayerType.PlayerType = playerType;
                    teamAnalysis.UserLeagueTeamAnalysisPlayerTypes.Add(userLeagueTeamAnalysisPlayerType);
                }
            }

            if (allTotalPlayers > 0)
                teamAnalysis.AverageOwnershipPercent = (allTotalPlayers == 0 ? 0 : allOwnershipPercent / allTotalPlayers);

            return teamAnalysis;
        }

    }
}