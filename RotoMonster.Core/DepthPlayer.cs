using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace RotoMonster.Core
{
    public class DepthPlayer
    {
        public SeasonPlayer SeasonPlayer { get; set; }
        public Position Position { get; set; }
        public Team Team { get; set; }
        public int Depth { get; set; }
        public OwnershipPlayer OwnershipPlayer { get; set; }
        public double TieBreakerSort { get; set; } = 0;
        public double OwnershipDepthPercent { get; set; } = 0;
        public double ActiveDepthPercent { get; set; }=0;

        public List<DepthPlayer> HigherDepthPlayers { get; set; }

        public List<DisplayPlayer> GetHigherDepthInjuredDisplayPlayers(List<PlayerStatus> playerStatuses)
        {
            if (HigherDepthPlayers != null)
            {
                var higherDepthInjuredDisplayPlayers = new List<DisplayPlayer>();
                foreach (var higherDepthPlayer in HigherDepthPlayers)
                {
                    var playerStatus = (from ps in playerStatuses where ps.PlayerId == higherDepthPlayer.SeasonPlayer.PlayerId select ps).FirstOrDefault();
                    if (playerStatus != null)
                    {
                        var displayPlayer = new DisplayPlayer();
                        displayPlayer.SeasonPlayer = higherDepthPlayer.SeasonPlayer;
                        displayPlayer.DepthPlayer = higherDepthPlayer;
                        displayPlayer.PlayerStatus = playerStatus;
                        higherDepthInjuredDisplayPlayers.Add(displayPlayer);
                    }
                }

                return higherDepthInjuredDisplayPlayers;
            }

            return null;
        }
    }

}
