using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RotoMonster.Models.Shared;
using RotoMonster.Core;

namespace RotoMonster.Models.Shared
{
    public class BoxScoreModel
    {
        public Game Game { get; set; }
        public List<BoxScorePlayer> BoxScorePlayers { get; set; }
        public Dictionary<PlayerType, PlayerTableModel> PlayerTableModels = new Dictionary<PlayerType, PlayerTableModel>();
        public Dictionary<Team, Dictionary<PlayerType, PlayerTableModel>> TeamPlayerTableModels = new Dictionary<Team, Dictionary<PlayerType, PlayerTableModel>>();
        public List<PlayerType> PlayerTypes { get; set; }
        public List<Article> Articles { get; set; } = null;
    }
}
