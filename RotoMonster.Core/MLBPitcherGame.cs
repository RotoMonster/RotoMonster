using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace RotoMonster.Core
{
    public class MLBPitcherGame
    {
        public int PlayerId { get; set; }
        public int GameId { get; set; }
        public int TeamId { get; set; }

        public double Innings { get; set; }
        public byte HitsAllowed { get; set; }
        public byte RunsAgainst { get; set; }
        public byte RunsEarned { get; set; }
        public byte BB { get; set; }
        public byte BBI { get; set; }
        public byte K { get; set; }
        public byte HR { get; set; }
        public byte Pitches { get; set; }
        public byte Strikes { get; set; }
        public byte OutsGroundBalls { get; set; }
        public byte OutsFlyBalls { get; set; }
        public byte Outs { get; set; }
        public byte HBP { get; set; }
        public byte WildPitches { get; set; }
        public byte W { get; set; }
        public byte L { get; set; }
        public byte S { get; set; }
        public byte Holds { get; set; }
        public byte Balks { get; set; }
        public byte Shutouts { get; set; }
        public byte CG { get; set; }
        public byte BS { get; set; }
        public byte Singles { get; set; }
        public byte Doubles { get; set; }
        public byte Triples { get; set; }
        public byte SacFlies { get; set; }
        public byte SacBunts { get; set; }
        public byte PickOffs { get; set; }
        public byte InheritedRunners { get; set; }
        public byte InheritedRunnersScored { get; set; }
        public byte GamesFinished { get; set; }
        public byte GamesStarted { get; set; }
        public byte BoxscoreOrder { get; set; }
        public byte QS { get; set; }
        public byte AtBatsAgainst { get; set; }


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
