using Microsoft.EntityFrameworkCore;
using RotoMonster.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace RotoMonster.Data.Libs
{
    public class NFLDbLib : ISportDbLib
    {
        private readonly RMDBContext db;

        public NFLDbLib(RMDBContext db)
        {
            this.db = db;
        }

        public IEnumerable<dynamic> GetStats(PlayerType playerType, Season season, DateTime startDate, DateTime endDate, bool finishedOnly, Game exactGame = null)
        {
            if (playerType.Title == "Offensive")
            {
                var q = (from bpg in db.NFLOffensiveGame.AsNoTracking()
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
                             RushTD = groupResult.Sum(f => f.RushTD),
                             RecTD = groupResult.Sum(f => f.RecTD),
                             RushYards = groupResult.Sum(f => f.RushYards),
                             RecYards = groupResult.Sum(f => f.RecYards),
                             PassTD = groupResult.Sum(f => f.PassTD),
                             PassYards = groupResult.Sum(f => f.PassYards),
                             PassInt = groupResult.Sum(f => f.PassInt),
                             PassCompletions = groupResult.Sum(f => f.PassCompletions),
                             RecReceptions = groupResult.Sum(f => f.RecReceptions),
                             RecTargets = groupResult.Sum(f => f.RecTargets),
                             RushAttempts = groupResult.Sum(f => f.RushAttempts),
                             FumblesLost = groupResult.Sum(f => f.FumblesLost),
                             PassAttempts = groupResult.Sum(f => f.PassAttempts),
                             RushYardsAfterContact = groupResult.Sum(f => f.RushYardsAfterContact),
                             RecYardsAfterCatch = groupResult.Sum(f => f.RecYardsAfterCatch),
                             RushBrokenTackles = groupResult.Sum(f => f.RushBrokenTackles),
                             RecRedzoneTargets = groupResult.Sum(f => f.RecRedzoneTargets),
                             RecAirYards = groupResult.Sum(f => f.RecAirYards),
                             PassAirYards = groupResult.Sum(f => f.PassAirYards),
                             PassPoorThrows = groupResult.Sum(f => f.PassPoorThrows)
                         }
                       ).ToList();

                return q;
            }

            if (playerType.Title == "Kickers")
            {
                var q = (from bpg in db.NFLKickerGames.AsNoTracking()
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
                             FieldGoals = groupResult.Sum(f => f.FieldGoals),
                             FieldGoalsMade = groupResult.Sum(f => f.FieldGoalsMade),
                             FieldGoals0to19 = groupResult.Sum(f => f.FieldGoals0to19),
                             FieldGoals20to29 = groupResult.Sum(f => f.FieldGoals20to29),
                             FieldGoals30to39 = groupResult.Sum(f => f.FieldGoals30to39),
                             FieldGoals40to49 = groupResult.Sum(f => f.FieldGoals40to49),
                             FieldGoals50 = groupResult.Sum(f => f.FieldGoals50),
                             FieldGoalsBlocked = groupResult.Sum(f => f.FieldGoalsBlocked),
                             FieldGoalsYards = groupResult.Sum(f => f.FieldGoalsYards),
                             FieldGoalsLongest = groupResult.Sum(f => f.FieldGoalsLongest),
                             ExtraPointsAttempts = groupResult.Sum(f => f.ExtraPointsAttempts),
                             ExtraPointsBlocked = groupResult.Sum(f => f.ExtraPointsBlocked),
                             ExtraPointsMade = groupResult.Sum(f => f.ExtraPointsMade)
                         }
                       ).ToList();

                return q;
            }

            if (playerType.Title == "Defense")
            {
                var q = (from bpg in db.NFLDefenseGames.AsNoTracking()
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
                             Sacks = groupResult.Sum(f => f.Sacks),
                             Interceptions = groupResult.Sum(f => f.Interceptions),
                             FumbleRecoveries = groupResult.Sum(f => f.FumbleRecoveries),
                             Touchdowns = groupResult.Sum(f => f.Touchdowns),
                             Safeties = groupResult.Sum(f => f.Safeties),
                             BlockedKicks = groupResult.Sum(f => f.BlockedKicks),
                             XpReturned = groupResult.Sum(f => f.XpReturned),
                             Points = groupResult.Sum(f => f.Points),
                             PassAttempts = groupResult.Sum(f => f.PassAttempts),
                             PassCompletion = groupResult.Sum(f => f.PassCompletion),
                             PassYards = groupResult.Sum(f => f.PassYards),
                             PassTouchdowns = groupResult.Sum(f => f.PassTouchdowns),
                             RushAttempts = groupResult.Sum(f => f.RushAttempts),
                             RushYards = groupResult.Sum(f => f.RushYards),
                             RushTouchdowns = groupResult.Sum(f => f.RushTouchdowns),
                             ReceivingAirYards = groupResult.Sum(f => f.ReceivingAirYards),
                             PassSacks = groupResult.Sum(f => f.PassSacks),
                             Minutes = groupResult.Sum(f => f.Minutes),
                             Points0 = groupResult.Sum(f => f.Points0),
                             Points1to6 = groupResult.Sum(f => f.Points1to6),
                             Points7to13 = groupResult.Sum(f => f.Points7to13),
                             Points14to20 = groupResult.Sum(f => f.Points14to20),
                             Points21to27 = groupResult.Sum(f => f.Points21to27),
                             Points28to34 = groupResult.Sum(f => f.Points28to34),
                             Points35 = groupResult.Sum(f => f.Points35),
                             Points2to10 = groupResult.Sum(f => f.Points2to10),
                             Points11to20 = groupResult.Sum(f => f.Points11to20)
                         }
                       ).ToList();

                return q;
            }

            //if (playerType.Title == "Hitters")
            //{
            //    var q = (from bpg in db.MLBHitterGames.AsNoTracking()
            //             join g in db.Games on bpg.GameId equals g.Id
            //             where g.Season.Id == season.Id && g.GameDate >= startDate && g.GameDate <= endDate
            //             orderby g.GameDate descending, g.Number descending
            //             group bpg by bpg.PlayerId into groupResult
            //             select new
            //             {
            //                 PlayerId = groupResult.Max(f => f.PlayerId),
            //                 TeamId = groupResult.Max(f => f.TeamId),
            //                 Games = groupResult.Count(),
            //                 BattingOrder = groupResult.Average(f => f.BattingOrder > 0 ? f.BattingOrder : 9),
            //                 H = groupResult.Sum(f => f.H),
            //                 HR = groupResult.Sum(f => f.HR),
            //                 RBI = groupResult.Sum(f => f.RBI),
            //                 SB = groupResult.Sum(f => f.SB),
            //                 R = groupResult.Sum(f => f.R),
            //                 BB = groupResult.Sum(f => f.BB),
            //                 Singles = groupResult.Sum(f => f.Singles),
            //                 Doubles = groupResult.Sum(f => f.Doubles),
            //                 Triples = groupResult.Sum(f => f.Triples),
            //                 K = groupResult.Sum(f => f.K),
            //                 CS = groupResult.Sum(f => f.CS),
            //                 AB = groupResult.Sum(f => f.AB),
            //                 Errors = groupResult.Sum(f => f.Errors),
            //                 GIDP = groupResult.Sum(f => f.GIDP),
            //                 HBP = groupResult.Sum(f => f.HBP),
            //                 SacFlies = groupResult.Sum(f => f.SacFlies),
            //                 PA = groupResult.Sum(f => f.PA),
            //                 Assists = groupResult.Sum(f => f.Assists)
            //             }
            //               ).ToList();

            //    return q;
            //}

            //if (playerType.Title == "Pitchers")
            //{
            //    var q = (from bpg in db.MLBPitcherGames.AsNoTracking()
            //             join g in db.Games on bpg.GameId equals g.Id
            //             where g.Season.Id == season.Id && g.GameDate >= startDate && g.GameDate <= endDate
            //             group bpg by bpg.PlayerId into groupResult
            //             select new
            //             {
            //                 PlayerId = groupResult.Max(f => f.PlayerId),
            //                 Games = groupResult.Count(),
            //                 W = groupResult.Sum(f => f.W),
            //                 S = groupResult.Sum(f => f.S),
            //                 K = groupResult.Sum(f => f.K),
            //                 Innings = groupResult.Sum(f => f.Innings),
            //                 QS = groupResult.Sum(f => f.QS),
            //                 CG = groupResult.Sum(f => f.CG),
            //                 Singles = groupResult.Sum(f => f.Singles),
            //                 Doubles = groupResult.Sum(f => f.Doubles),
            //                 Triples = groupResult.Sum(f => f.Triples),
            //                 HR = groupResult.Sum(f => f.HR),
            //                 Shutouts = groupResult.Sum(f => f.Shutouts),
            //                 L = groupResult.Sum(f => f.L),
            //                 BS = groupResult.Sum(f => f.BS),
            //                 Holds = groupResult.Sum(f => f.Holds),
            //                 RunsAgainst = groupResult.Sum(f => f.RunsAgainst),
            //                 RunsEarned = groupResult.Sum(f => f.RunsEarned),
            //                 HitsAllowed = groupResult.Sum(f => f.HitsAllowed),
            //                 BB = groupResult.Sum(f => f.BB),
            //                 HBP = groupResult.Sum(f => f.HBP),
            //                 WildPitches = groupResult.Sum(f => f.WildPitches),
            //                 Balks = groupResult.Sum(f => f.Balks),
            //                 Outs = groupResult.Sum(f => f.Outs),
            //                 AtBatsAgainst = groupResult.Sum(f => f.AtBatsAgainst)
            //             }).ToList();
            //    return q;
            //}

            throw new Exception("Need to add support for " + playerType.Title);
        }


    }
}
