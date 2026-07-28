using RotoMonster.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace RotoMonster.Data.Libs
{
    public interface ISportDbLib
    {
        IEnumerable<dynamic> GetStats(PlayerType playerType, Season season, DateTime startDate, DateTime endDate, bool finishedOnly, Game extraGame = null);
    }

}
