using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using RotoMonster.Core;

namespace RotoMonster.Data.Libs
{
    public class NBADbLib : ISportDbLib
    {
        private readonly RMDBContext db;

        public NBADbLib(RMDBContext db)
        {
            this.db = db;
        }

        public IEnumerable<dynamic> GetStats(PlayerType playerType, Season season, DateTime startDate, DateTime endDate, bool finishedOnly, Game exactGame = null)
        {
            var q = (from bpg in db.NBAPlayerGames.AsNoTracking()
                     join g in db.Games on bpg.GameId equals g.Id
                     where g.Season.Id == season.Id
                        && (exactGame != null) ? g.Id == exactGame.Id : g.GameDate >= startDate && g.GameDate <= endDate
                        && (finishedOnly ? g.IsFinished : true)
                     group bpg by bpg.PlayerId into groupResult
                     select new
                     {
                         PlayerId = groupResult.Max(f => f.PlayerId),
                         TeamId = groupResult.Max(f => f.TeamId),
                         Games = groupResult.Count(),
                         Starts = groupResult.Sum(f => f.Started),
                         Threes = groupResult.Sum(f => f.Threes),
                         Assists = groupResult.Sum(f => f.Assists),
                         Steals = groupResult.Sum(f => f.Steals),
                         Blocks = groupResult.Sum(f => f.Blocks),
                         DefensiveRebounds = groupResult.Sum(f => f.DefensiveRebounds),
                         OffensiveRebounds = groupResult.Sum(f => f.OffensiveRebounds),
                         DoubleDoubles = groupResult.Sum(f => f.DoubleDoubles),
                         TripleDoubles = groupResult.Sum(f => f.TripleDoubles),
                         FieldGoals = groupResult.Sum(f => f.FieldGoals),
                         FieldGoalsAttempted = groupResult.Sum(f => f.FieldGoalsAttempted),
                         Fouls = groupResult.Sum(f => f.Fouls),
                         FreeThrows = groupResult.Sum(f => f.FreeThrows),
                         FreeThrowsAttempted = groupResult.Sum(f => f.FreeThrowsAttempted),
                         Minutes = groupResult.Sum(f => f.Minutes),
                         ThreesAttempted = groupResult.Sum(f => f.ThreesAttempted),
                         Turnovers = groupResult.Sum(f => f.Turnovers),
                         Technicals = groupResult.Sum(f => f.Technicals),
                         PlusMinus = groupResult.Sum(f => f.PlusMinus),
                         Wins = groupResult.Sum(f => f.Wins)
                     }
            ).ToList();

            return (IEnumerable<dynamic>)q;
        }

    }
}
