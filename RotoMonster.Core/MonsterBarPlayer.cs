using System;
using System.Collections.Generic;
using System.Text;

namespace RotoMonster.Core
{
    public class MonsterBarPlayer
    {
        public Player Player { get; set; }
        public bool IsGoodFreeAgent { get; set; } = false;
        public List<MonsterBarValuePlayer> MonsterBarValuePlayers { get; set; } = new List<MonsterBarValuePlayer>();
    }

}
