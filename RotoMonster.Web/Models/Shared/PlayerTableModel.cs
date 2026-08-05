using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RotoMonster.Core;

namespace RotoMonster.Models.Shared
{
    public class PlayerTableModel
    {
        public Sport Sport { get; set; }
        public List<Position> AllPositions { get; set; }
        public List<DisplayPlayer> DisplayPlayers { get; set; }
        public List<CategorySetting> CategorySettings { get; set; }
        public PlayerType PlayerType { get; set; }
        public PerValue DisplayPerValue { get; set; }
        public Draft Draft { get; set; } = null;
        public List<PerValue> ValuePerValues { get; set; } = new List<PerValue>();
        public int GamesCategoryId { get; set; }
        public int StartsCategoryId { get; set; }
        public int ViewLimit { get; set; }
        public List<DisplayCategory> BeforeCategories { get; set; }
        public List<DisplayCategory> AfterCategories { get; set; }
        public List<UserDisplayCategory> UserDisplayCategories { get; set; }
        public UserDisplayColumns UserDisplayColumns { get; set; } = new UserDisplayColumns();
        public bool IsPlayerHistory { get; set; } = false;
        public string PlayerHeaderTitle { get; set; } = "Player";
        public bool ColorStats { get; set; } = true;
        public bool ShowRank { get; set; } = true;
        public bool ShowPositions { get; set; } = true;
        public bool ShowTeam { get; set; } = true;
        public bool ShowDraft { get; set; } = false;
        public bool ShowAdp { get; set; } = true;
        public bool ShowAllAdp { get; set; } = true;
        public bool ShowDefaultAdp { get; set; } = true;
        public bool ShowCategoryValues { get; set; } = true;
        public bool ShowDepth { get; set; } = true;
        public bool ShowInjuries { get; set; } = true;
        public bool ShowTrending { get; set; } = true;
        public bool ShowPositionalValue { get; set; } = true;
        public bool ShowCurrentGame { get; set; } = true;
        public bool ShowGames { get; set; } = true;
        public bool ShowPositionPercents { get; set; } = false;

        public bool ShowIR { get; set; } = false;
        public int NextPick { get; set; } = 0;
        public bool ShowMonsterBot { get; set; } = false;
        public bool ShowEase { get; set; } = false;
        public string UserId { get; set; } = null;
        public string ScoringSystem { get; set; } = "C";

        public string ExtraColumnTitle1 { get; set; } = "";
        public string ExtraColumnDescription1 { get; set; } = "";
        public string ExtraColumnTitle2 { get; set; } = "";
        public string ExtraColumnDescription2 { get; set; } = "";
        public string ExtraColumnTitle3 { get; set; } = "";
        public string ExtraColumnDescription3 { get; set; } = "";
        public string NextColumnTitle { get; set; } = "Next";

        public bool ShowCurrentWeekGame { get; set; } = false;
        public DateTime RemainingWeekStartDate { get; set; }
        public DateTime RemainingWeekEndDate { get; set; }

        public bool ShowNextWeekGames { get; set; } = false;
        public DateTime NextWeekStartDate { get; set; }
        public DateTime NextWeekEndDate { get; set; }

        public UserLeague SelectedUserLeague { get; set; } = null;
        public DateSelect[] DateSelects { get; set; }
        public DateSelect[] StreamSelects { get; set; }
        public List<PlayerGameState> PlayerGameStates { get; set; } = null;
        public MonsterBar MonsterBarGame { get; set; } = null;
        public MonsterBar MonsterBarTotal { get; set; } = null;
        public PositionBoostHashModel PositionBoostHashModel { get; set; } = null;

        public bool ShowAnalysisDates { get; set; } = false;
        public DateTime AnalysisStartDate { get; set; }
        public DateTime AnalysisEndDate { get; set; }

        public bool ShowProjections { get; set; } = false;
        public bool ShowProjectionPercents { get; set; } = false;

        public List<DateTime> AnalysisDates
        {
            get
            {
                List<DateTime> dates = new List<DateTime>();
                if (AreAnalysisDatesValid)
                {
                    var current = AnalysisStartDate;
                    while (current <= AnalysisEndDate)
                    {
                        dates.Add(current);
                        current = current.AddDays(1);
                    }
                }
                return dates;
            }
        }

        public bool AreAnalysisDatesValid
        {
            get
            {
                return AnalysisEndDate >= AnalysisStartDate;
            }
        }

        public void FillUserDefaultShows()
        {
            ShowTrending = UserDisplayColumns.IsSelected("Waiver Trends");
            ShowIR = UserDisplayColumns.IsSelected("IR %");
            ShowPositionalValue = UserDisplayColumns.IsSelected("PositionValue");
            ShowDepth = UserDisplayColumns.IsSelected("Depth");
            ShowCategoryValues = UserDisplayColumns.IsSelected("Category Values");
            ShowCurrentGame = UserDisplayColumns.IsSelected("Current Game");
            // ShowCurrentWeekGame = UserDisplayColumns.IsSelected("Current Week");
            // ShowNextWeekGames = UserDisplayColumns.IsSelected("Next Week");
        }
    }
}
