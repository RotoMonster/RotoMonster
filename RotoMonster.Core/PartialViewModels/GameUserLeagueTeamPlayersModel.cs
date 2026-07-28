using System;
using System.Collections.Generic;
using System.Text;

namespace RotoMonster.Core.PartialViewModels
{
    public class GameUserLeagueTeamPlayersModel
    {
        public List<GameUserLeagueTeamPlayer> GameUserLeagueTeamPlayers { get; set; }
        public bool UseOwnColor { get; set; } = true;
    }

}
