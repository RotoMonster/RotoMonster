using System;
using System.Collections.Generic;
using System.Text;

namespace RotoMonster.Core.PartialViewModels
{
    public class UserLeagueTableModel
    {
        public FantasyProvider FantasyProvider { get; set; }
        public List<UserLeague> ProviderUserLeagues { get; set; }
        public List<UserLeague> CurrentUserLeagues { get; set; }
        public bool ShowMyTeam { get; set; }
    }
}
