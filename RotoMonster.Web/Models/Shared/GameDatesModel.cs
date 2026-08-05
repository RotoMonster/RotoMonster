using System;
using System.Collections.Generic;
using System.Text;
using RotoMonster.Core;

namespace RotoMonster.Models.Shared
{
    public class GameDatesModel
    {
        public List<Game> TeamGames { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateSelect[] DateSelects { get; set; }

        public List<PlayerGameDate> PlayerGameDates { get; set; } = new List<PlayerGameDate>();

    }

    public class DateSelect
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool Selected { get; set; }

    }

}
