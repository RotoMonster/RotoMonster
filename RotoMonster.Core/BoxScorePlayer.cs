using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RotoMonster.Core
{
    public class BoxScorePlayer
    {
        public Team Team { get; set; }
        public Game Game { get; set; }
        public ValuePlayer ValuePlayer { get; set; }
        public SeasonPlayer SeasonPlayer { get; set; }
    }
}
