using System;
using System.Collections.Generic;
using System.Text;

namespace RotoMonster.Core
{
    public class DraftPlayer
    {
        public int DraftId { get; set; }
        public int PlayerId { get; set; }
        public int DraftOrder { get; set; }
        public int? Price { get; set; }
        public string ProviderTeamId { get; set; }

        public Draft Draft { get; set; }
        public Player Player { get; set; }
    }
}
