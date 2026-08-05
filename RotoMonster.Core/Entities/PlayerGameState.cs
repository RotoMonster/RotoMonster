using System;
using System.Collections.Generic;
using System.Text;

namespace RotoMonster.Core
{
    public class PlayerGameState
    {
        public int GameId { get; set; }
        public int TeamId { get; set; }
        public int PlayerId { get; set; }
        public int? PositionId { get; set; }
        public int PlayerGameStateTypeId { get; set; }
        public DateTime DateAdded { get; set; }
        public string Details { get; set; }

        public Game Game { get; set; }
        public Team Team { get; set; }
        public Player Player { get; set; }
        public Position Position { get; set; }
        public PlayerGameStateType PlayerGameStateType { get; set; }
    }
}
