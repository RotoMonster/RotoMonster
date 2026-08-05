using System;
using System.Collections.Generic;
using System.Text;

namespace RotoMonster.Core
{
    public class Division
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int DisplayOrder { get; set; }
        public List<SeasonDivision> SeasonDivisions { get; set; }
        public List<SeasonTeam> SeasonTeams { get; set; }

    }
}
