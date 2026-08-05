using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace RotoMonster.Core
{
    public class PlayerGameMissed
    {
        public int GameId { get; set; }
        public int PlayerId { get; set; }
        public int TeamId { get; set; }
        public bool? IsActive { get; set; }
        public bool? Started { get; set; }
        public bool? Played { get; set; }
        [StringLength(100)]
        public string NotPlayingReason { get; set; }
        [StringLength(100)]
        public string NotPlayingDescription { get; set; }

        public Game Game { get; set; }
        public Player Player { get; set; }
        public Team Team { get; set; }
    }
}
