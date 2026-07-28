using System;
using System.Collections.Generic;
using System.Text;

namespace RotoMonster.Core
{
    public class Article
    {
        public int Id { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string Type { get; set; }
        public string Title { get; set; }
        public string SportRadarId { get; set; }
        public string Byline { get; set; }
        public string Dateline { get; set; }
        public string Credit { get; set; }
        public string Content { get; set; }
        public bool IsInjury { get; set; }
        public bool IsTransaction { get; set; }

        public List<ArticleGame> ArticleGames { get; set; } = new List<ArticleGame>();
        public List<ArticlePlayer> ArticlePlayers { get; set; } = new List<ArticlePlayer>();
        public List<ArticleTeam> ArticleTeams { get; set; } = new List<ArticleTeam>();

        public DateTime EasternCreatedDate
        {
            get
            {
                TimeZoneInfo easternTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
                return TimeZoneInfo.ConvertTimeFromUtc(CreatedDate, easternTimeZone);
            }
        }

        public TimeSpan TimeSince
        {
            get
            {
                return DateTime.UtcNow - CreatedDate;
            }
        }

        public bool IsAutomated
        {
            get
            {
                bool isAuto = false;
                if (Content.IndexOf("Data Skrive") != -1)
                    isAuto=true;
                else if (Byline=="")
                    isAuto=true;

                return isAuto;
            }
        }

    }
}
