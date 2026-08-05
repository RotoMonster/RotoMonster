using System;
using System.Collections.Generic;
using System.Text;
using RotoMonster.Core;

namespace RotoMonster.Models.Shared
{
    public class GameUserLeagueTeamPlayersModel
    {
        public List<GameUserLeagueTeamPlayer> GameUserLeagueTeamPlayers { get; set; }
        public bool UseOwnColor { get; set; } = true;
    }

}
