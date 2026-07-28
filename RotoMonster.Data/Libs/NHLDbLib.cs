using Microsoft.EntityFrameworkCore;
using RotoMonster.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace RotoMonster.Data.Libs
{
    public class NHLDbLib : ISportDbLib
    {
        private readonly RMDBContext db;

        public NHLDbLib(RMDBContext db)
        {
            this.db = db;
        }

        public IEnumerable<dynamic> GetStats(PlayerType playerType, Season season, DateTime startDate, DateTime endDate, bool finishedOnly, Game exactGame = null)
        {
            if (playerType.Title == "Skater")
            {
                var q = (from bpg in db.NHLSkaterGames.AsNoTracking()
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
                             PowerPlayTimeOnIce = groupResult.Sum(f => f.PowerPlayTimeOnIce),
                             PowerPlayShots = groupResult.Sum(f => f.PowerPlayShots),
                             PowerPlayGoals = groupResult.Sum(f => f.PowerPlayGoals),
                             PowerPlayMissedShots = groupResult.Sum(f => f.PowerPlayMissedShots),
                             PowerPlayAssists = groupResult.Sum(f => f.PowerPlayAssists),
                             PowerPlayFaceoffsWon = groupResult.Sum(f => f.PowerPlayFaceoffsWon),
                             PowerPlayFaceoffsLost = groupResult.Sum(f => f.PowerPlayFaceoffsLost),
                             ShorthandedTimeOnIce = groupResult.Sum(f => f.ShorthandedTimeOnIce),
                             ShorthandedShots = groupResult.Sum(f => f.ShorthandedShots),
                             ShorthandedGoals = groupResult.Sum(f => f.ShorthandedGoals),
                             ShorthandedMissedShots = groupResult.Sum(f => f.ShorthandedMissedShots),
                             ShorthandedAssists = groupResult.Sum(f => f.ShorthandedAssists),
                             ShorthandedFaceoffsWon = groupResult.Sum(f => f.ShorthandedFaceoffsWon),
                             ShorthandedFaceoffsLost = groupResult.Sum(f => f.ShorthandedFaceoffsLost),
                             EvenstrengthTimeOnIce = groupResult.Sum(f => f.EvenstrengthTimeOnIce),
                             EvenstrengthShots = groupResult.Sum(f => f.EvenstrengthShots),
                             EvenstrengthGoals = groupResult.Sum(f => f.EvenstrengthGoals),
                             EvenstrengthMissedShots = groupResult.Sum(f => f.EvenstrengthMissedShots),
                             EvenstrengthAssists = groupResult.Sum(f => f.EvenstrengthAssists),
                             EvenstrengthFaceoffsWon = groupResult.Sum(f => f.EvenstrengthFaceoffsWon),
                             EvenstrengthFaceoffsLost = groupResult.Sum(f => f.EvenstrengthFaceoffsLost),
                             PenaltyShots = groupResult.Sum(f => f.PenaltyShots),
                             PenaltyGoals = groupResult.Sum(f => f.PenaltyGoals),
                             PenaltyMissedShots = groupResult.Sum(f => f.PenaltyMissedShots),
                             ShootoutShots = groupResult.Sum(f => f.ShootoutShots),
                             ShootoutGoals = groupResult.Sum(f => f.ShootoutGoals),
                             ShootoutMissedShots = groupResult.Sum(f => f.ShootoutMissedShots),
                             Penalties = groupResult.Sum(f => f.Penalties),
                             PenaltyMinutes = groupResult.Sum(f => f.PenaltyMinutes),
                             BlockedAttempts = groupResult.Sum(f => f.BlockedAttempts),
                             Hits = groupResult.Sum(f => f.Hits),
                             Giveaways = groupResult.Sum(f => f.Giveaways),
                             Takeaways = groupResult.Sum(f => f.Takeaways),
                             BlockedShots = groupResult.Sum(f => f.BlockedShots),
                             PlusMinus = groupResult.Sum(f => f.PlusMinus),
                             OvertimeGoals = groupResult.Sum(f => f.OvertimeGoals),
                             OvertimeAssists = groupResult.Sum(f => f.OvertimeAssists),
                             OvertimeShots = groupResult.Sum(f => f.OvertimeShots),
                             PenaltiesMajor = groupResult.Sum(f => f.PenaltiesMajor),
                             PenaltiesMinor = groupResult.Sum(f => f.PenaltiesMinor),
                             PenaltiesMisconduct = groupResult.Sum(f => f.PenaltiesMisconduct),
                             EmptynetGoals = groupResult.Sum(f => f.EmptynetGoals),
                             Shifts = groupResult.Sum(f => f.Shifts)
                         }).ToList();

                return q;
            }

            if (playerType.Title == "Goalie")
            {
                var q = (from bpg in db.NHLGoalieGames.AsNoTracking()
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
                             Wins = groupResult.Sum(f => f.Wins),
                             Assists = groupResult.Sum(f => f.Assists),
                             Shutouts = groupResult.Sum(f => f.Shutouts),
                             PowerPlayTimeOnIce = groupResult.Sum(f => f.PowerPlayTimeOnIce),
                             PowerPlayShotsAgainst = groupResult.Sum(f => f.PowerPlayShotsAgainst),
                             PowerPlayGoalsAgainst = groupResult.Sum(f => f.PowerPlayGoalsAgainst),
                             PowerPlaySaves = groupResult.Sum(f => f.PowerPlaySaves),
                             ShorthandedTimeOnIce = groupResult.Sum(f => f.ShorthandedTimeOnIce),
                             ShorthandedShotsAgainst = groupResult.Sum(f => f.ShorthandedShotsAgainst),
                             ShorthandedGoalsAgainst = groupResult.Sum(f => f.ShorthandedGoalsAgainst),
                             ShorthandedPlaySaves = groupResult.Sum(f => f.ShorthandedPlaySaves),
                             EvenstrengthTimeOnIce = groupResult.Sum(f => f.EvenstrengthTimeOnIce),
                             EvenstrengthShotsAgainst = groupResult.Sum(f => f.EvenstrengthShotsAgainst),
                             EvenstrengthGoalsAgainst = groupResult.Sum(f => f.EvenstrengthGoalsAgainst),
                             EvenstrengthPlaySaves = groupResult.Sum(f => f.EvenstrengthPlaySaves),
                             PenaltyShotsAgainst = groupResult.Sum(f => f.PenaltyShotsAgainst),
                             PenaltyGoalsAgainst = groupResult.Sum(f => f.PenaltyGoalsAgainst),
                             PenaltySaves = groupResult.Sum(f => f.PenaltySaves),
                             ShootoutShotsAgainst = groupResult.Sum(f => f.ShootoutShotsAgainst),
                             ShootoutGoalsAgainst = groupResult.Sum(f => f.ShootoutGoalsAgainst),
                             ShootoutSaves = groupResult.Sum(f => f.ShootoutSaves)
                         }).ToList();
                return q;
            }

            throw new Exception("Need to add support for " + playerType.Title);
        }

    }
}
