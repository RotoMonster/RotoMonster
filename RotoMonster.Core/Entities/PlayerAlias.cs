using System;
using System.Collections.Generic;
using System.Text;

namespace RotoMonster.Core
{
    public class PlayerAlias
    {
        public int Id { get; set; }
        public string Alias { get; set; }
        public Player Player { get; set; }
    }
}
