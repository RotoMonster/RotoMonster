using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations.Schema;

namespace RotoMonster.Core
{
    public class NHLSkaterGame
    {
        public int PlayerId { get; set; }
        public int GameId { get; set; }
        public int TeamId { get; set; }

        public byte Started { get; set; }

        public double PowerPlayTimeOnIce { get; set; }
        public byte PowerPlayShots { get; set; }
        public byte PowerPlayGoals { get; set; }
        public byte PowerPlayMissedShots { get; set; }
        public byte PowerPlayAssists { get; set; }
        public byte PowerPlayFaceoffsWon { get; set; }
        public byte PowerPlayFaceoffsLost { get; set; }

        public double ShorthandedTimeOnIce { get; set; }
        public byte ShorthandedShots { get; set; }
        public byte ShorthandedGoals { get; set; }
        public byte ShorthandedMissedShots { get; set; }
        public byte ShorthandedAssists { get; set; }
        public byte ShorthandedFaceoffsWon { get; set; }
        public byte ShorthandedFaceoffsLost { get; set; }

        public double EvenstrengthTimeOnIce { get; set; }
        public byte EvenstrengthShots { get; set; }
        public byte EvenstrengthGoals { get; set; }
        public byte EvenstrengthMissedShots { get; set; }
        public byte EvenstrengthAssists { get; set; }
        public byte EvenstrengthFaceoffsWon { get; set; }
        public byte EvenstrengthFaceoffsLost { get; set; }

        public byte PenaltyShots { get; set; }
        public byte PenaltyGoals { get; set; }
        public byte PenaltyMissedShots { get; set; }

        public byte ShootoutShots { get; set; }
        public byte ShootoutGoals { get; set; }
        public byte ShootoutMissedShots { get; set; }

        public byte Penalties { get; set; }
        public double PenaltyMinutes { get; set; }
        public byte BlockedAttempts { get; set; }
        public byte Hits { get; set; }
        public byte Giveaways { get; set; }
        public byte Takeaways { get; set; }
        public byte BlockedShots { get; set; }
        public double PlusMinus { get; set; }
        public byte OvertimeGoals { get; set; }
        public byte OvertimeAssists { get; set; }
        public byte OvertimeShots { get; set; }
        public byte PenaltiesMajor { get; set; }
        public byte PenaltiesMinor { get; set; }
        public byte PenaltiesMisconduct { get; set; }
        public byte EmptynetGoals { get; set; }
        public byte Shifts { get; set; }

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
