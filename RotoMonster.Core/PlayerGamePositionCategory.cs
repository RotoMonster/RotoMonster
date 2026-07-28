using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RotoMonster.Core
{
    public class PlayerGamePositionCategory
    {
        public int GameId { get; set; }
        public int PlayerId { get; set; }
        public int TeamId { get; set; }
        public int PositionId { get; set; }
        public int CategoryId { get; set; }
        public double Percent { get; set; }
        public float CategoryValue { get; set; }

        public Game Game { get; set; }
        public Player Player { get; set; }
        public Team Team { get; set; }
        public Position Position { get; set; }
        public Category Category { get; set; }

    }
}
