using RotoMonster.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace RotoMonster.Core
{
    public class PlayerStatus
    {
        public int Id { get; set; }
        public int PlayerId { get; set; }
        public int? GameId { get; set; }
        public bool IsActive { get; set; }
        public int PlayerStatusTypeId { get; set; }
        public int? PlayerStatusTagTypeId { get; set; }
        public string OwningUserId { get; set; }
        public DateTime DateAdded { get; set; }
        public DateTime? DateDeactivated { get; set; }
        public DateTime? EstimatedReturnDate { get; set; }
        public string Comment { get; set; }
        public string Subject { get; set; }
        public string Source { get; set; }
        public string SourceUrl { get; set; }
        public DateTime? DateDeleted { get; set; }
        public string DeletedByUserId { get; set; }
        public short? GamePercent { get; set; }

        public Player Player { get; set; }
        public PlayerStatusType PlayerStatusType { get; set; }
        public PlayerStatusTagType PlayerStatusTagType { get; set; }

        [NotMapped] public List<Game> EstimatedGamesToMiss { get; set; } = null;

        public TimeSpan TimeSince
        {
            get
            {
                return DateTime.UtcNow - DateAdded;
            }
        }

        public bool IsOut
        {
            get
            {
                return PlayerStatusType.UsesDate.GetValueOrDefault() || PlayerStatusType.PlayType == "O";
            }
        }


    }
}
