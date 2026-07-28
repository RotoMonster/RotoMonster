using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace RotoMonster.Core
{
    public class NFLDefenseGame
    {
        public int PlayerId { get; set; }
        public int GameId { get; set; }
        public int TeamId { get; set; }
            
        public byte? Sacks { get; set; }
        public byte? Interceptions { get; set; }
        public byte? FumbleRecoveries { get; set; }
        public byte? Touchdowns { get; set; }
        public byte? Safeties { get; set; }
        public byte? BlockedKicks { get; set; }
        public byte? XpReturned { get; set; }
        public byte? Points { get; set; }
        public byte? PassAttempts { get; set; }
        public byte? PassCompletion { get; set; }
        public short? PassYards { get; set; }
        public byte? PassTouchdowns { get; set; }
        public byte? RushAttempts { get; set; }
        public short? RushYards { get; set; }
        public byte? RushTouchdowns { get; set; }
        public short? ReceivingAirYards { get; set; }
        public byte? PassSacks { get; set; }
        public double? Minutes { get; set; }

        public byte? Points0 { get; set; }
        public byte? Points1to6 { get; set; }
        public byte? Points7to13 { get; set; }
        public byte? Points14to20 { get; set; }
        public byte? Points21to27 { get; set; }
        public byte? Points28to34 { get; set; }
        public byte? Points35 { get; set; }
        public byte? Points2to10 { get; set; }
        public byte? Points11to20 { get; set; }

        public byte? Points14to17 { get; set; }
        public byte? Points35to45 { get; set; }
        public byte? Points46 { get; set; }


        public Player Player { get; set; }
        public Game Game { get; set; }

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
