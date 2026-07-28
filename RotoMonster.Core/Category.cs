using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace RotoMonster.Core
{
    public class Category
    {
        public int Id { get; set; }
        public PlayerType PlayerType { get; set; }
        public string Title { get; set; }
        public string Abbreviation { get; set; }
        public string SourceField { get; set; }
        public bool? IsPositive { get; set; }
        public bool? IsDefault { get; set; }
        public bool IsDisplayCategory { get; set; }
        public bool IsMeasureCategory { get; set; } // the category that measures how much of a game is played (minutes, innings, etc.)
        public bool IsDefaultDisplayCategory { get; set; }
        public double? DefaultPointsPerStat { get; set; }
        public bool? UseAsValue { get; set; }
        public bool? ExcludeFromEase { get; set; }
        public bool IsScoringAlertCategory { get; set; }
        public bool? IsDisabled { get; set; }
        public int DisplayOrder { get; set; }
        public int? WeightCategoryId { get; set; }
        public Category WeightCategory { get; set; }
        public string YahooId { get; set; }
        public string ESPNId { get; set; }
        public string FanTraxGroup { get; set; }
        public string FanTraxId { get; set; }
        public string CBSId { get; set; }
        public string OtherAbbreviations { get; set; }
        public bool PerValuesSameAsTotal { get; set; }

        public List<CategoryPerValue> CategoryPerValues { get; set; }

        [NotMapped]
        public string DisplayTitle
        {
            get
            {
                if (PlayerType.DisplayTitle == "")
                    return Title;
                else
                    return PlayerType.DisplayTitle + " : " + Title;
            }
        }


    }
}
