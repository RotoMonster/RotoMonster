using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace RotoMonster.Core
{
    public class Sport
    {
        [StringLength(3)] public string Title { get; set; }
        [StringLength(20)] public string SportType { get; set; }
        [StringLength(20)] public string DivisionTitle { get; set; }
        public bool UsesCategories { get; set; }
        public bool UsesPointsPerStat { get; set; }
        public string DefaultScoringSystem { get; set; }
        public DayOfWeek StartDayOfWeek { get; set; }
        [StringLength(10)] public string MenuColor { get; set; }
        [StringLength(10)] public string LogoColor { get; set; }
        public double LowPoints { get; set; }
        public double HighPoints { get; set; }
        public int PeriodsPerGame { get; set; }
        public int MinutesPerPeriod { get; set; }   // won't work for MLB
        [StringLength(5)] public string ESPNCode { get; set; }
        public bool UseTotalMonsterBar { get; set; } = false;

        public bool IsNBA
        {
            get
            {
                return Title == "NBA";
            }
        }

        public bool IsMLB
        {
            get
            {
                return Title == "MLB";
            }
        }

        public bool IsNFL
        {
            get
            {
                return Title == "NFL";
            }
        }

        public bool IsNHL
        {
            get
            {
                return Title == "NHL";
            }
        }

        public DateTime WeekStartDate(Season season, DateTime startDate)
        {
            var today = startDate;
            if (today < season.StartDate)
                today = season.StartDate;
            while (today.DayOfWeek != StartDayOfWeek)
                today = today.AddDays(-1);

            return today;
        }

    }
}
