using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RotoMonster.Core.PartialViewModels
{
    public class DepthChartTeamModel
    {
        public Team Team { get; set; }
        public List<DisplayPlayer> DisplayPlayers { get; set; } = new List<DisplayPlayer>();
    }

}
