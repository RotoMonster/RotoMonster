using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace RotoMonster.Core
{
    public class Draft
    {
        public int Id { get; set; }
        public int SeasonId { get; set; }
        public int FantasyProviderId { get; set; }
        public DateTime DateAdded { get; set; } = DateTime.UtcNow;
        public DateTime DraftDate { get; set; }
        public string Title { get; set; } = "";
        public string ProviderLeagueId { get; set; } = "";
        public int LeagueSize { get; set; } = 0;
        public int NumberOfTeams { get; set; } = 0;
        public bool IsProLeague { get; set; } = false;
        public bool IsDynasty { get; set; } = false;
        public bool IsAuction { get; set; } = false;
        public bool IsMoney { get; set; } = false;
        public string LeagueType { get; set; } = "";
        public bool IsMock { get; set; } = false;
        public bool IsFinished { get; set; } = false;

        public FantasyProvider FantasyProvider { get; set; }
        public Season Season { get; set; }

        public List<DraftPlayer> DraftPlayers { get; set; }

        [NotMapped] public bool IsPreDraft { get; set; } = false;
        [NotMapped] public bool IsLive { get; set; } = false;

        [NotMapped] public List<UserLeagueTeam> DraftUserLeagueTeams = new List<UserLeagueTeam>();

        [NotMapped] public List<DraftPlayerType> DraftPlayerTypes = new List<DraftPlayerType>();

        public void ImportUserLeague(UserLeague userLeague)
        {
            foreach(var pt in userLeague.UserLeaguePlayerTypes)
            {
                var newPt = new DraftPlayerType();
                newPt.PlayerTypeId = pt.PlayerTypeId;
                newPt.CategoriesStringId = pt.CategoriesStringId;
                DraftPlayerTypes.Add(newPt);
            }
        }

        public bool IsAnalysis
        {
            get
            {
                return Title.IndexOf("Yahoo Public") >= 0 || Title.IndexOf("Classic Draft") >= 0;
            }
        }

    }
}
