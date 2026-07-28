using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace RotoMonster.Core
{
    public class MLBHitterGame
    {
        public int PlayerId { get; set; }
        public int GameId { get; set; }
        public int TeamId { get; set; }

        public byte AB { get; set; }
        public byte R { get; set; }
        public byte H { get; set; }
        public byte RBI { get; set; }
        public byte BB { get; set; }
        public byte K { get; set; }
        public byte LOB { get; set; }
        public byte Singles { get; set; }
        public byte Doubles { get; set; }
        public byte Triples { get; set; }
        public byte HR { get; set; }
        public byte SB { get; set; }
        public byte CS { get; set; }
        public byte SacFlies { get; set; }
        public byte SacBunts { get; set; }
        public byte HBP { get; set; }
        public byte RBITwoOut { get; set; }
        public byte GrandSlams { get; set; }
        public byte GIDP { get; set; }
        public byte Errors { get; set; }
        public byte PastBalls { get; set; }
        public byte Starts { get; set; }
        public byte PA { get; set; }
        public byte BattingOrder { get; set; }
        public byte Assists { get; set; }
        public byte FullInnings { get; set; }
        public byte ThirdInnings { get; set; }
        public byte Putouts { get; set; }

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
