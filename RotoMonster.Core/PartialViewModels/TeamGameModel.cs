using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RotoMonster.Core.PartialViewModels
{
    public class TeamGameModel
    {
        public Sport Sport { get; set; }
        public Team Team { get; set; }
        public Game Game { get; set; }
        public DisplayPlayer DisplayPlayer { get; set; }
    }
}
