using System;
using System.Collections.Generic;
using System.Text;

namespace RotoMonster.Core.PartialViewModels
{
    public class GameDatesHeaderModel
    {
        public GameDatesHeaderModel(int week, DateSelect[] dateSelects)
        {
            Week = week;
            DateSelects = dateSelects;
        }

        public string Title { get; set; } = "";
        public string Tooltip { get; set; } = "";
        public string SortId { get; set; } = "";
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int StartIndex { get; set; }
        public int Week { get; set; }
        public DateSelect[] DateSelects { get; set; }
        public DateSelect[] StreamSelects { get; set; }
    }

}
