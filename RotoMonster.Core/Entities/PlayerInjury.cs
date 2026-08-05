using System;
using System.Collections.Generic;
using System.Text;

namespace RotoMonster.Core
{
    public class PlayerInjury
    {
        public int Id { get; set; }
        public int PlayerId { get; set; }
        public int? TeamId { get; set; }
        public DateTime DownloadDate { get; set; }
        public string ProviderInjuryId { get; set; }
        public string PlayerStatus { get; set; }
        public string InjuryStatus { get; set; }
        public string Comment { get; set; }
        public string Description { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? UpdateDate { get; set; }
        public DateTime? EstimatedReturnDate { get; set; }

        public TimeSpan TimeSince
        {
            get
            {
                return DateTime.UtcNow - UpdateDate.GetValueOrDefault();
            }
        }
    }
}
