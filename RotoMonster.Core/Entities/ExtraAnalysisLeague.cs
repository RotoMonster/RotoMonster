using System;
using System.Collections.Generic;
using System.Text;

namespace RotoMonster.Core
{
    public class ExtraAnalysisLeague
    {
        public int FantasyProviderId { get; set; }
        public string ProviderId { get; set; }
        public string Title { get; set; }
        public int? EntryFee { get; set; }
        public int? NumberOfTeams { get; set; }
        public DateTime DraftDate { get; set; }

        public FantasyProvider FantasyProvider { get; set; }
    }

}
