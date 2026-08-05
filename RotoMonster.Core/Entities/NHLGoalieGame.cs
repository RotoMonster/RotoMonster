using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations.Schema;

namespace RotoMonster.Core
{
    public class NHLGoalieGame
    {
        public int PlayerId { get; set; }
        public int GameId { get; set; }
        public int TeamId { get; set; }

        public byte Started { get; set; }
        public byte Shifts { get; set; }
        public string Credit { get; set; }
        public byte Wins { get; set; }
        public byte Shutouts { get; set; }
        public byte Assists { get; set; }

        public double PowerPlayTimeOnIce { get; set; }
        public byte PowerPlayShotsAgainst { get; set; }
        public byte PowerPlayGoalsAgainst { get; set; }
        public byte PowerPlaySaves { get; set; }

        public double ShorthandedTimeOnIce { get; set; }
        public byte ShorthandedShotsAgainst { get; set; }
        public byte ShorthandedGoalsAgainst { get; set; }
        public byte ShorthandedPlaySaves { get; set; }

        public double EvenstrengthTimeOnIce { get; set; }
        public byte EvenstrengthShotsAgainst { get; set; }
        public byte EvenstrengthGoalsAgainst { get; set; }
        public byte EvenstrengthPlaySaves { get; set; }

        public byte PenaltyShotsAgainst { get; set; }
        public byte PenaltyGoalsAgainst { get; set; }
        public byte PenaltySaves { get; set; }

        public byte ShootoutShotsAgainst { get; set; }
        public byte ShootoutGoalsAgainst { get; set; }
        public byte ShootoutSaves { get; set; }

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
