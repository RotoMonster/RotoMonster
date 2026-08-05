using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RotoMonster.Core;

namespace RotoMonster.Models.Shared
{
    public class DepthChartTeamModel
    {
        public Team Team { get; set; }
        public List<DisplayPlayer> DisplayPlayers { get; set; } = new List<DisplayPlayer>();
    }

}
