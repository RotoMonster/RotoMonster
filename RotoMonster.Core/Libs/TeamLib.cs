using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RotoMonster.Core.Libs
{
    public class TeamLib
    {
        public Team GetAliasMatch(List<Team> teams, string alias)
        {
            foreach (var team in teams)
            {
                if (team.Name == alias)
                    return team;

                var match = (from a in team.TeamAliases where a.Alias == alias select a).FirstOrDefault();
                if (match != null)
                    return team;
            }

            return null;
        }
    }
}
