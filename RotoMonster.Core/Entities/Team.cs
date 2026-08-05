using System;
using System.Collections.Generic;
using System.Text;

namespace RotoMonster.Core
{
    public class Team
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string SportRadarId { get; set; }

        public List<SeasonTeam> SeasonTeams { get; set; }
        public List<TeamAlias> TeamAliases { get; set; }

    }
}
