using System;
using System.Collections.Generic;
using System.Text;

namespace RotoMonster.Core
{
    public class DisplayMonsterBarPlayer
    {
        public MonsterBarPlayer MonsterBarPlayer { get; set; }
        public List<MonsterBarItem> MonsterBarItems { get; set; }
        public bool IsCompact { get; set; } = false;
    }
}
