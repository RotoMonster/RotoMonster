using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace RotoMonster.Core
{
    public class Season
    {
        public int Id { get; set; }
        public int? Year { get; set; }
        public string Title { get; set; }
        public string Abbreviation { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool? IsRegularSeason { get; set; }
        public string YahooId { get; set; }
        public string ESPNYear { get; set; }
        public bool IsEnabled { get; set; }
        public int DisplayOrder { get; set; }

        public List<SeasonTeam> SeasonTeams { get; set; }
        public List<SeasonDivision> Divisions { get; set; }
        public List<SeasonPlayer> SeasonPlayers { get; set; }

        [NotMapped]
        public DateTime UpdatedDate { get; set; }
        [NotMapped] public bool HasStarted { get; set; }
        [NotMapped] public bool IsFinished { get; set; } = false;
        [NotMapped] public string State { get; set; } = "";
    }
}
