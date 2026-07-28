using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RotoMonster.Core
{
    public class GameScoringAlert
    {
        public int Id { get; set; }
        public int GameId { get; set; }
        public int PlayerId { get; set; }
        public int TeamId { get; set; }
        public int CategoryId { get; set; }
        public DateTime ScoringDate { get; set; }
        public int CurrentValue { get; set; }


        public Game Game { get; set; }
        public Player Player { get; set; }
        public Team Team { get; set; }
        public Category Category { get; set; }

        public TimeSpan TimeSince
        {
            get
            {
                return DateTime.UtcNow - ScoringDate;
            }
        }
    }
}
