using System;
using System.Collections.Generic;
using System.Text;

namespace RotoMonster.Core
{
    public class UserLeagueImportError
    {
        public int UserLeagueId { get; set; }
        public string Error { get; set; }

        public UserLeague UserLeague { get; set; }
    }
}
