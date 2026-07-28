using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace RotoMonster.Core
{
    public class NFLOffensiveGame
    {
        public int PlayerId { get; set; }
        public int GameId { get; set; }
        public int TeamId { get; set; }
        public byte? PassAttempts { get; set; }
        public byte? PassCompletions { get; set; }
        public int? PassYards { get; set; }
        public byte? PassTD { get; set; }
        public byte? PassInt { get; set; }
        public byte? PassSacks { get; set; }
        public int? PassSackYards { get; set; }
        public byte? RushAttempts { get; set; }
        public int? RushYards { get; set; }
        public byte? RushTD { get; set; }
        public byte? RushFumbles { get; set; }
        public byte? RecTargets { get; set; }
        public byte? RecReceptions { get; set; }
        public int? RecYards { get; set; }
        public byte? RecTD { get; set; }
        public byte? Fumbles { get; set; }
        public byte? FumblesLost { get; set; }
        public byte? RushRedzoneAttempted { get; set; }
        public int? RushYardsLost { get; set; }
        public byte? RushLost { get; set; }
        public byte? RushBrokenTackles { get; set; }
        public int? RushYardsAfterContact { get; set; }
        public byte? RushKneelDowns { get; set; }
        public int? RecYardsAfterCatch { get; set; }
        public byte? RecRedzoneTargets { get; set; }
        public int? RecAirYards { get; set; }
        public byte? RecBrokenTackles { get; set; }
        public byte? RecDroppedPasses { get; set; }
        public byte? RecCatchablePasses { get; set; }
        public int? RecYardsAafterContact { get; set; }
        public double? PassRating { get; set; }
        public int? PassAirYards { get; set; }
        public byte? RassRedzoneAttempts { get; set; }
        public byte? PassThrowAways { get; set; }
        public byte? PassPoorThrows { get; set; }
        public byte? PassDefendedPasses { get; set; }
        public byte? PassDroppedPasses { get; set; }
        public byte? PassSpikes { get; set; }
        public byte? PassBlitzes { get; set; }
        public byte? PassHurries { get; set; }
        public byte? PassKnockdowns { get; set; }
        public double? PassPocketTime { get; set; }
        public byte? ReturnReturns { get; set; }
        public int? ReturnYards { get; set; }
        public byte? ReturnTD { get; set; }
        public byte? ReturnFaircatches { get; set; }

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
