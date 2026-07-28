using System;
using System.Collections.Generic;
using System.Text;

namespace RotoMonster.Core
{
    public class MonsterBotPlayer
    {
        public DisplayPlayer DisplayPlayer { get; set; } = new DisplayPlayer();

        public List<MonsterBotPlayerComment> MonsterBotPlayerComments { get; set; } = new List<MonsterBotPlayerComment>();
    }
}
