using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RotoMonster.Core.PartialViewModels
{
    public class FrontGameScoresModel
    {
        public Sport Sport { get; set; }
        public DateTime ScheduleStartDate { get; set; }
        public DateTime ScheduleEndDate { get; set; }
        public List<Game> ScheduleGames { get; set; }
        public List<GameUserLeagueTeamPlayer> ActiveGameUserLeagueTeamPlayers { get; set; }
        public List<GameUserLeagueTeamPlayer> OwnedGameUserLeagueTeamPlayers { get; set; }
    }
}
