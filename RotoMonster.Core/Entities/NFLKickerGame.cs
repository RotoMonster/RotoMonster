using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace RotoMonster.Core
{
    public class NFLKickerGame
    {
        public int PlayerId { get; set; }
        public int GameId { get; set; }
        public int TeamId { get; set; }

        public byte? FieldGoals { get; set; }
        public byte? FieldGoalsMade { get; set; }
        public byte? FieldGoals0to19 { get; set; }
        public byte? FieldGoals20to29 { get; set; }
        public byte? FieldGoals30to39 { get; set; }
        public byte? FieldGoals40to49 { get; set; }
        public byte? FieldGoals50 { get; set; }

        public byte? FieldGoals0to39 { get; set; }

        public byte? FieldGoalsBlocked { get; set; }
        public int? FieldGoalsYards { get; set; }
        public byte? FieldGoalsLongest { get; set; }
        public byte? ExtraPointsAttempts { get; set; }
        public byte? ExtraPointsBlocked { get; set; }
        public byte? ExtraPointsMade { get; set; }

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
