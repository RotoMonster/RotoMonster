using Microsoft.EntityFrameworkCore;
using RotoMonster.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace RotoMonster.Data.Libs
{
    public class MLBDbLib : ISportDbLib
    {
        private readonly RMDBContext db;

        public MLBDbLib(RMDBContext db)
        {
            this.db = db;
        }

        public IEnumerable<dynamic> GetStats(PlayerType playerType, Season season, DateTime startDate, DateTime endDate, bool finishedOnly, Game exactGame = null)
        {
            if (playerType.Title == "Hitters")
            {
                var q = (from bpg in db.MLBHitterGames.AsNoTracking()
                         join g in db.Games on bpg.GameId equals g.Id
                         where g.Season.Id == season.Id
                            && (exactGame != null) ? g.Id == exactGame.Id : g.GameDate >= startDate && g.GameDate <= endDate
                            && (finishedOnly ? g.IsFinished : true)
                         orderby g.GameDate descending, g.Number descending
                         group bpg by bpg.PlayerId into groupResult
                         select new
                         {
                             PlayerId = groupResult.Max(f => f.PlayerId),
                             TeamId = groupResult.Max(f => f.TeamId),
                             GameId = groupResult.Max(f => f.GameId),
                             Games = groupResult.Count(),
                             BattingOrder = groupResult.Average(f => f.BattingOrder > 0 ? f.BattingOrder : 9),
                             H = groupResult.Sum(f => f.H),
                             HR = groupResult.Sum(f => f.HR),
                             RBI = groupResult.Sum(f => f.RBI),
                             SB = groupResult.Sum(f => f.SB),
                             R = groupResult.Sum(f => f.R),
                             BB = groupResult.Sum(f => f.BB),
                             Singles = groupResult.Sum(f => f.Singles),
                             Doubles = groupResult.Sum(f => f.Doubles),
                             Triples = groupResult.Sum(f => f.Triples),
                             K = groupResult.Sum(f => f.K),
                             CS = groupResult.Sum(f => f.CS),
                             AB = groupResult.Sum(f => f.AB),
                             Errors = groupResult.Sum(f => f.Errors),
                             GIDP = groupResult.Sum(f => f.GIDP),
                             HBP = groupResult.Sum(f => f.HBP),
                             SacFlies = groupResult.Sum(f => f.SacFlies),
                             PA = groupResult.Sum(f => f.PA),
                             Assists = groupResult.Sum(f => f.Assists)
                         }
                           ).ToList();

                return q;
            }

            if (playerType.Title == "Pitchers")
            {
                var q = (from bpg in db.MLBPitcherGames.AsNoTracking()
                         join g in db.Games on bpg.GameId equals g.Id
                         where g.Season.Id == season.Id
                            && (exactGame != null) ? g.Id == exactGame.Id : g.GameDate >= startDate && g.GameDate <= endDate
                            && (finishedOnly ? g.IsFinished : true)
                         group bpg by bpg.PlayerId into groupResult
                         select new
                         {
                             PlayerId = groupResult.Max(f => f.PlayerId),
                             Games = groupResult.Count(),
                             W = groupResult.Sum(f => f.W),
                             S = groupResult.Sum(f => f.S),
                             K = groupResult.Sum(f => f.K),
                             Innings = groupResult.Sum(f => f.Innings),
                             QS = groupResult.Sum(f => f.QS),
                             CG = groupResult.Sum(f => f.CG),
                             Singles = groupResult.Sum(f => f.Singles),
                             Doubles = groupResult.Sum(f => f.Doubles),
                             Triples = groupResult.Sum(f => f.Triples),
                             HR = groupResult.Sum(f => f.HR),
                             Shutouts = groupResult.Sum(f => f.Shutouts),
                             L = groupResult.Sum(f => f.L),
                             BS = groupResult.Sum(f => f.BS),
                             Holds = groupResult.Sum(f => f.Holds),
                             RunsAgainst = groupResult.Sum(f => f.RunsAgainst),
                             RunsEarned = groupResult.Sum(f => f.RunsEarned),
                             HitsAllowed = groupResult.Sum(f => f.HitsAllowed),
                             BB = groupResult.Sum(f => f.BB),
                             HBP = groupResult.Sum(f => f.HBP),
                             WildPitches = groupResult.Sum(f => f.WildPitches),
                             Balks = groupResult.Sum(f => f.Balks),
                             Outs = groupResult.Sum(f => f.Outs),
                             AtBatsAgainst = groupResult.Sum(f => f.AtBatsAgainst)
                         }).ToList();
                return q;
            }

            throw new Exception("Need to add support for " + playerType.Title);
        }

    }
}
