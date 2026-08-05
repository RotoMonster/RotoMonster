using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace RotoMonster.Core
{
    public class NBAPlayerGame
    {
        public int PlayerId { get; set; }
        public int GameId { get; set; }
        public int TeamId { get; set; }

        public double Minutes { get; set; }
        public byte FieldGoals { get; set; }
        public byte FieldGoalsAttempted { get; set; }
        public byte Threes { get; set; }
        public byte ThreesAttempted { get; set; }
        public byte FreeThrows { get; set; }
        public byte FreeThrowsAttempted { get; set; }
        public byte OffensiveRebounds { get; set; }
        public byte DefensiveRebounds { get; set; }
        public byte Assists { get; set; }
        public byte Steals { get; set; }
        public byte Blocks { get; set; }
        public byte Turnovers { get; set; }
        public byte Points { get; set; }
        public byte Fouls { get; set; }
        public byte Started { get; set; }
        public byte DoubleDoubles { get; set; }
        public byte TripleDoubles { get; set; }
        public byte Technicals { get; set; }
        public double PlusMinus { get; set; }
        public double? Usage { get; set; }
        public byte Wins { get; set; }
        public bool? FoulTrouble { get; set; }

        public Player Player { get; set; }
        public Game Game { get; set; }
        public Team Team { get; set; }

        [NotMapped]
        public int OpponentTeamId
        {
            get
            {
                return Game.GetOpponentId(TeamId);
            }
        }
    }
}
