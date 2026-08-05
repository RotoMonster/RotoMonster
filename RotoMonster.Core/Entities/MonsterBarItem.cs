using System;
using System.Collections.Generic;
using System.Text;

namespace RotoMonster.Core
{
    public class MonsterBarItem
    {
        public List<ValuePlayer> ValuePlayers { get; set; }
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public int DisplayOrder { get; set; } = 0;
    }

}
