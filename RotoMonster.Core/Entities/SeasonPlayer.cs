using System;
using System.Collections.Generic;
using System.Text;

namespace RotoMonster.Core
{
    public class SeasonPlayer
    {
        public int PlayerId { get; set; }
        public int SeasonId { get; set; }
        public int TeamId { get; set; }
        public int PlayerTypeId { get; set; }
        public int? Salary { get; set; }

        public Player Player { get; set; }
        public Season Season { get; set; }
        public Team Team { get; set; }
        public PlayerType PlayerType { get; set; }
    }
}
