using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using RotoMonster.Core;
using RotoMonster.Core.Libs;
using RotoMonster.Data.Libs;
using System;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Net.WebSockets;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading;

namespace RotoMonster.Data
{
    public partial class RMSqlData : IRMData
    {
        private readonly RMDBContext db;
        private readonly IConfiguration config;
        private readonly IMemoryCache memoryCache;
        private ValuePlayerLib valuePlayerLib = new ValuePlayerLib();
        private readonly ColorLib colorLib = new ColorLib();

        //private readonly Dictionary<string, bool> cacheKeys = new Dictionary<string, bool>();
        private static CancellationTokenSource _resetCacheToken = new CancellationTokenSource();


        public RMSqlData(RMDBContext db, IConfiguration config, IMemoryCache memoryCache)
        {
            this.db = db;
            this.config = config;
            this.memoryCache = memoryCache;
        }

        public bool CacheItemExists(string cacheId)
        {
            return GetCacheItem(cacheId) != null;
        }

        public object AddCacheItem(string cacheId, object cacheItem)
        {
            if (memoryCache != null)
            {
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(30));
                cacheEntryOptions.AddExpirationToken(new CancellationChangeToken(_resetCacheToken.Token));

                memoryCache.Set(cacheId, cacheItem, cacheEntryOptions);
            }

            return cacheItem;
        }

        public int RemoveCacheItem(string cacheId)
        {
            int removes = 0;
            var field = typeof(MemoryCache).GetProperty("EntriesCollection", BindingFlags.NonPublic | BindingFlags.Instance);
            var collection = field.GetValue(memoryCache) as ICollection;
            var items = new List<string>();
            if (collection != null)
                foreach (var item in collection)
                {
                    memoryCache.Remove(item.ToString());

                    var methodInfo = item.GetType().GetProperty("Key");
                    var val = methodInfo.GetValue(item);
                    items.Add(val.ToString());
                }

            return removes;
        }

        public object GetCacheItem(string cacheId)
        {
            if (memoryCache != null)
            {
                object outObject;
                if (memoryCache.TryGetValue(cacheId, out outObject))
                    return outObject;
            }

            return null;
        }

        public int ClearCache()
        {
            if (_resetCacheToken != null && !_resetCacheToken.IsCancellationRequested && _resetCacheToken.Token.CanBeCanceled)
            {
                _resetCacheToken.Cancel();
                _resetCacheToken.Dispose();
            }

            _resetCacheToken = new CancellationTokenSource();

            return 0;
        }

        public List<PlayerDefaultPosition> PlayerDefaultPositions
        {
            get
            {
                string cacheId = "PlayerDefaultPositions";
                if (CacheItemExists(cacheId))
                    return (List<PlayerDefaultPosition>)GetCacheItem(cacheId);

                var pdp = db.PlayerDefaultPositions
                    .AsNoTracking()
                    .Include(a => a.Position).ToList();

                AddCacheItem(cacheId, pdp);

                return pdp;
            }
        }

        public Player Add(Player newPlayer)
        {
            db.Add(newPlayer);
            return newPlayer;
        }

        public int Commit()
        {
            return db.SaveChanges();
        }

        public Player Delete(int playerId)
        {
            var player = GetById(playerId);
            if (player != null)
            {
                db.Players.Remove(player);
            }
            return player;
        }

        public Player GetById(int playerId)
        {
            var player = db.Players.Find(playerId);

            return player;
        }

        public int GetCountOfPlayers()
        {
            return db.Players.Count();
        }

        public IEnumerable<Player> GetPlayerByName(string name)
        {
            string cacheId = "GetPlayerByName" + name;
            if (CacheItemExists(cacheId))
                return (IEnumerable<Player>)GetCacheItem(cacheId);

            IEnumerable<Player> query = null;
            if (name != null)
            {
                string cleanName = name.Replace(" ", "").ToLower();
                query = from p in db.Players.AsNoTracking()
                        where p.FirstName.StartsWith(name) || p.LastName.StartsWith(name)
                            || (p.FirstName + p.LastName).ToLower().Replace(" ", "").Contains(cleanName)
                            || (p.LastName + p.FirstName).ToLower().Replace(" ", "").Contains(cleanName)
                        orderby p.LastName, p.FirstName
                        select p;

                return query;
            }
            else
            {
                query = (from p in db.Players.AsNoTracking() select p).Take(0);
            }

            AddCacheItem(cacheId, query);

            return query;
        }

        public List<string> AutoCompletePlayerSearch(string term)
        {
            var seasonPlayerIds = new HashSet<int>();
            var season = GetDefaultSeason();
            if (season != null)
            {
                foreach (var sp in GetAllSeasonPlayers(season))
                    seasonPlayerIds.Add(sp.PlayerId);
            }

            var query = (from p in GetPlayerByName(term)
                         where seasonPlayerIds.Count == 0 || seasonPlayerIds.Contains(p.Id)
                         orderby p.LastName, p.FirstName
                         select p.FirstName + " " + p.LastName).Take(50).ToList();

            return query;
        }

        public SeasonPlayer GetSeasonPlayer(int playerId)
        {
            string cacheId = "GetSeasonPlayer" + playerId.ToString();
            if (CacheItemExists(cacheId))
                return (SeasonPlayer)GetCacheItem(cacheId);

            var sp = (from p in db.SeasonPlayers
                      .AsNoTracking()
                      .Include(i => i.Player)
                      .Include(i => i.Team)
                      .Include(i => i.Season)
                      .Include(i => i.PlayerType)
                      where p.PlayerId == playerId
                      orderby p.Season.EndDate descending
                      select p).FirstOrDefault();

            AddCacheItem(cacheId, sp);

            return sp;
        }

        public Player Update(Player updatedPlayer)
        {
            var entity = db.Players.Attach(updatedPlayer);
            entity.State = EntityState.Modified;

            return updatedPlayer;
        }

        public Sport Sport
        {
            get
            {
                string cacheId = "Sport";
                if (CacheItemExists(cacheId))
                    return (Sport)GetCacheItem(cacheId);

                var s = db.Sports.AsNoTracking().First();

                AddCacheItem(cacheId, s);

                return s;
            }
        }

        public List<PlayerGamePosition> GetPlayerGamePositions(PlayerType playerType, int seasonId, DateTime startDate, DateTime endDate)
        {
            string cacheId = "GetPlayerGamePositions";
            cacheId += ":PT" + playerType.Id.ToString();
            cacheId += ":S" + seasonId.ToString();
            cacheId += ":SD" + startDate.ToShortDateString();
            cacheId += ":ED" + endDate.ToShortDateString();
            if (CacheItemExists(cacheId))
                return (List<PlayerGamePosition>)GetCacheItem(cacheId);

            List<PlayerGamePosition> playerGamePositions = new List<PlayerGamePosition>();

            foreach (var player in GetPlayers())
            {
                var playerGamePosition = new PlayerGamePosition();
                playerGamePosition.Player = player;
                playerGamePosition.PlayerId = player.Id;
                playerGamePosition.Game = null;
                playerGamePosition.GameId = 0;
                if (player.DefaultPosition != null)
                {
                    playerGamePosition.Position = player.DefaultPosition;
                    playerGamePosition.PositionId = playerGamePosition.Position.Id;
                    playerGamePosition.Percent = 100;
                    playerGamePositions.Add(playerGamePosition);
                }
            }

            AddCacheItem(cacheId, playerGamePositions);

            return playerGamePositions;
        }

        public List<ValuePlayer> GetTeamEaseValuePlayers(
            PlayerType playerType,
            Season season,
            DateTime startDate,
            DateTime endDate,
            List<CategorySetting> categorySettings,
            string scoringSystem)
        {
            List<ValuePlayer> teamEaseValuePlayers = new List<ValuePlayer>();
            string cacheId = "GetTeamEaseValuePlayers"
                + ":S" + season.Id.ToString()
                + ":SD" + startDate.ToShortDateString()
                + ":ED" + endDate.ToShortDateString()
                + ":SC" + scoringSystem;
            foreach (var cs in categorySettings)
                cacheId += "CS" + cs.Category.Id.ToString() + "|" + cs.PointsPerStat.ToString() + "|" + cs.IsActive.ToString();
            if (CacheItemExists(cacheId))
                return (List<ValuePlayer>)GetCacheItem(cacheId);

            ColorLib colorLib = new ColorLib();

            var opposingStatPlayers = GetOpposingTeamStatPlayers(playerType, season.Id, startDate, endDate);

            List<int> activeRosterSpotIds = new List<int>();
            foreach (var statPlayer in opposingStatPlayers)
            {
                if (!activeRosterSpotIds.Exists(i => i == statPlayer.TeamActiveRosterSpotId))
                    activeRosterSpotIds.Add(statPlayer.TeamActiveRosterSpotId);
            }

            List<ValuePlayer> valuePlayers = new List<ValuePlayer>();
            foreach (int activeRosterSpotId in activeRosterSpotIds)
            {
                var processStatPlayers = (from sp in opposingStatPlayers where sp.TeamActiveRosterSpotId == activeRosterSpotId select sp).ToList();
                ValueAverages opposingTeamOutValueAverages;
                var processValuePlayers = valuePlayerLib.GetValuePlayers(processStatPlayers,
                    categorySettings,
                    GetGamesCategory(playerType.Id),
                    scoringSystem,
                    GetSkillPerValue(playerType.Id),
                    playerType,
                    GetDisplayCategories(),
                    opposingStatPlayers.Count,
                    out opposingTeamOutValueAverages);
                foreach (var valuePlayer in processValuePlayers)
                    valuePlayers.Add(valuePlayer);
            }

            AddCacheItem(cacheId, valuePlayers);

            return valuePlayers;
        }

        public ActiveRosterSpot GetEaseActiveRosterSpot(Position position)
        {
            if (position == null)
                return null;

            string cacheId = "GetEaseActiveRosterSpot" + position.Abbreviation;
            if (CacheItemExists(cacheId))
                return (ActiveRosterSpot)GetCacheItem(cacheId);

            ActiveRosterSpot activeRosterSpot = (from ars in db.ActiveRosterSpotPositions.Include(i => i.ActiveRosterSpot)
                                                 where ars.ActiveRosterSpot.UsesEase && ars.PositionId == position.Id
                                                 select ars.ActiveRosterSpot).FirstOrDefault();

            AddCacheItem(cacheId, activeRosterSpot);

            return activeRosterSpot;
        }

        public List<ValuePlayer> GetOpposingTeamValuePlayers(
            PlayerType playerType,
            Season season,
            DateTime startDate,
            DateTime endDate,
            List<CategorySetting> categorySettings,
            string scoringSystem)
        {
            string cacheId = "GetOpposingTeamValuePlayers"
                + ":PT" + playerType.Id.ToString()
                + ":S" + season.Id.ToString()
                + ":SD" + startDate.ToShortDateString()
                + ":ED" + endDate.ToShortDateString()
                + ":SC" + scoringSystem;
            foreach (var cs in categorySettings)
                cacheId += "CS" + cs.Category.Id.ToString() + "|" + cs.PointsPerStat.ToString() + "|" + cs.IsActive.ToString();
            if (CacheItemExists(cacheId))
                return (List<ValuePlayer>)GetCacheItem(cacheId);

            List<ValuePlayer> opposingTeamValuePlayers = new List<ValuePlayer>();

            ColorLib colorLib = new ColorLib();
            ValuePlayerLib lib = new ValuePlayerLib();

            var opposingTeamStatPlayers = GetOpposingTeamStatPlayers(playerType, season.Id, season.StartDate, season.EndDate);

            ValueAverages teamOutValueAverages;
            var teamStatPlayers = GetTeamStatPlayers(playerType, season.Id, season.StartDate, season.EndDate);
            var teamStatValuesPlayers = lib.GetValuePlayers(teamStatPlayers,
                    categorySettings,
                    GetGamesCategory(playerType.Id),
                    scoringSystem,
                    GetSkillPerValue(playerType.Id),
                    playerType,
                    GetDisplayCategories(),
                    teamStatPlayers.Count,
                    out teamOutValueAverages);

            foreach (var position in (from position1 in GetPositions() where position1.PlayerType.Id == playerType.Id && position1.IsActualPosition select position1))
            {
                var positionStatPlayers = (from statPlayer in opposingTeamStatPlayers where statPlayer.TeamActiveRosterSpotId == position.Id select statPlayer).ToList();
                ValueAverages opposingTeamOutValueAverages;
                var tmpValuePlayers = lib.GetValuePlayers(positionStatPlayers,
                    categorySettings,
                    GetGamesCategory(playerType.Id),
                    scoringSystem,
                    GetSkillPerValue(playerType.Id),
                    playerType,
                    GetDisplayCategories(),
                    positionStatPlayers.Count,
                    out opposingTeamOutValueAverages);
                foreach (var valuePlayer in tmpValuePlayers)
                    opposingTeamValuePlayers.Add(valuePlayer);
            }

            var statGames = (from g in GetGames(season) where g.IsFinished && g.GameDate >= startDate && g.GameDate <= endDate select g);
            foreach (var opposingTeamValuePlayer in opposingTeamValuePlayers)
            {
                var valueGames = (from g in statGames where g.IncludesTeam(opposingTeamValuePlayer.StatPlayer.Team.Id) select g).ToList();
                if (valueGames.Count > 0)
                {
                    foreach (var game in valueGames)
                    {
                        int opponentTeamId = game.GetOpponentId(opposingTeamValuePlayer.StatPlayer.Team.Id);
                        var teamStatValuePlayer = (from vp in teamStatValuesPlayers
                                                   where vp.StatPlayer.Player.Id == opponentTeamId && vp.StatPlayer.TeamActiveRosterSpotId == opposingTeamValuePlayer.StatPlayer.TeamActiveRosterSpotId
                                                   select vp).FirstOrDefault();
                        if (teamStatValuePlayer != null)
                            opposingTeamValuePlayer.ExpectedTeamLeagueValue += teamStatValuePlayer.LeagueValue;
                    }
                    opposingTeamValuePlayer.ExpectedTeamLeagueValue /= Convert.ToDouble(valueGames.Count);
                    if (scoringSystem == "P")
                    {
                        if (opposingTeamValuePlayer.ExpectedTeamLeagueValue != 0)
                            opposingTeamValuePlayer.OpponentValueBoost = (opposingTeamValuePlayer.LeagueValue - opposingTeamValuePlayer.ExpectedTeamLeagueValue) / opposingTeamValuePlayer.ExpectedTeamLeagueValue * 100;
                        opposingTeamValuePlayer.OpponentValueBoost = Math.Round(opposingTeamValuePlayer.OpponentValueBoost, 1);

                        if (opposingTeamValuePlayer.OpponentValueBoost >= 0)
                            opposingTeamValuePlayer.LeagueValueColor = colorLib.GetGreenRangeColorStyle(opposingTeamValuePlayer.OpponentValueBoost, 0, 50, true);
                        else
                            opposingTeamValuePlayer.LeagueValueColor = colorLib.GetRedRangeColorStyle(-1 * opposingTeamValuePlayer.OpponentValueBoost, 0, 50, true);
                    }
                    else if (scoringSystem == "C")
                    {
                        opposingTeamValuePlayer.OpponentValueBoost = opposingTeamValuePlayer.LeagueValue - opposingTeamValuePlayer.ExpectedTeamLeagueValue;
                        opposingTeamValuePlayer.OpponentValueBoost = Math.Round(opposingTeamValuePlayer.OpponentValueBoost, 2);
                        if (opposingTeamValuePlayer.OpponentValueBoost >= 0)
                            opposingTeamValuePlayer.LeagueValueColor = colorLib.GetGreenRangeColorStyle(opposingTeamValuePlayer.OpponentValueBoost, 0, 2, true);
                        else
                            opposingTeamValuePlayer.LeagueValueColor = colorLib.GetRedRangeColorStyle(-1 * opposingTeamValuePlayer.OpponentValueBoost, 0, 2, true);
                    }
                }
            }

            AddCacheItem(cacheId, opposingTeamValuePlayers);

            return opposingTeamValuePlayers;
        }

        public List<StatPlayer> GetOpposingTeamStatPlayers(PlayerType playerType, int seasonId, DateTime startDate, DateTime endDate)
        {
            var season = GetSeason(seasonId);
            List<StatPlayer> statPlayers = new List<StatPlayer>();
            List<Category> cats = GetCategories(playerType);
            List<PerValue> perValues = GetPerValues(playerType.Id);
            Category gamesCat = GetGamesCategory(playerType.Id);
            Category startsCat = GetStartsCategory(playerType.Id);
            var statCategories = (from c in GetCategories() where c.PlayerType.Id == playerType.Id && c.SourceField != null orderby c.DisplayOrder select c).ToList();

            List<Player> players = GetPlayers();
            List<SeasonPlayer> seasonPlayers = GetSeasonPlayers(GetSeason(seasonId), playerType);
            List<Team> teams = GetTeams();
            List<PlayerGamePosition> playerGamePositions = GetPlayerGamePositions(playerType, seasonId, startDate, endDate);
            var games = GetGames(season);
            var analyzedGames = (from g in games where g.GameDate >= startDate && g.GameDate <= endDate select g).ToList();

            StatPlayerLib statPlayerLib = new StatPlayerLib();
            IEnumerable<dynamic> query = null;

            if (Sport.IsNBA)
                query = (from pg in db.NBAPlayerGames.Include(i => i.Game)
                         join g in db.Games on pg.GameId equals g.Id
                         where g.IsFinished && g.GameDate >= startDate && g.GameDate <= endDate && g.SeasonId == seasonId
                         select pg);

            else if (Sport.IsMLB && playerType.Title == "Hitters")
                query = (from pg in db.MLBHitterGames.Include(i => i.Game)
                         join g in db.Games on pg.GameId equals g.Id
                         where g.IsFinished && g.GameDate >= startDate && g.GameDate <= endDate && g.SeasonId == seasonId
                         select pg);

            else if (Sport.IsMLB && playerType.Title == "Pitchers")
                query = (from pg in db.MLBPitcherGames.Include(i => i.Game)
                         join g in db.Games on pg.GameId equals g.Id
                         where g.IsFinished && g.GameDate >= startDate && g.GameDate <= endDate && g.SeasonId == seasonId
                         select pg);

            else if (Sport.IsNHL && playerType.Title == "Skater")
                query = (from pg in db.NHLSkaterGames.Include(i => i.Game)
                         join g in db.Games on pg.GameId equals g.Id
                         where g.IsFinished && g.GameDate >= startDate && g.GameDate <= endDate && g.SeasonId == seasonId
                         select pg);

            else if (Sport.IsNHL && playerType.Title == "Goalie")
                query = (from pg in db.NHLGoalieGames.Include(i => i.Game)
                         join g in db.Games on pg.GameId equals g.Id
                         where g.IsFinished && g.GameDate >= startDate && g.GameDate <= endDate && g.SeasonId == seasonId
                         select pg);

            else if (Sport.IsNFL && playerType.Title == "Offensive")
                query = (from pg in db.NFLOffensiveGame.Include(i => i.Game)
                         join g in db.Games on pg.GameId equals g.Id
                         where g.IsFinished && g.GameDate >= startDate && g.GameDate <= endDate && g.SeasonId == seasonId
                         select pg);

            else if (Sport.IsNFL && playerType.Title == "Kickers")
                query = (from pg in db.NFLKickerGames.Include(i => i.Game)
                         join g in db.Games on pg.GameId equals g.Id
                         where g.IsFinished && g.GameDate >= startDate && g.GameDate <= endDate && g.SeasonId == seasonId
                         select pg);

            else if (Sport.IsNFL && playerType.Title == "Defense")
                query = (from pg in db.NFLDefenseGames.Include(i => i.Game)
                         join g in db.Games on pg.GameId equals g.Id
                         where g.IsFinished && g.GameDate >= startDate && g.GameDate <= endDate && g.SeasonId == seasonId
                         select pg);

            if (query != null)
            {
                List<PlayerDefaultPosition> playerDefaultPositions = GetPlayerDefaultPositions();
                var activeRosterSpotPositions = GetActiveRosterSpotPositions();

                foreach (var row in query)
                {
                    int playerId = row.GetType().GetProperty("PlayerId").GetValue(row, null);
                    int teamId = row.GetType().GetProperty("TeamId").GetValue(row, null);
                    int opponentId = row.GetType().GetProperty("OpponentTeamId").GetValue(row, null);
                    var defPosition = (from dp in playerDefaultPositions where dp.PlayerId == playerId && dp.Position.IsActualPosition select dp).FirstOrDefault();
                    if (defPosition != null)
                    {
                        var activeRosterSpot = (from arsp in activeRosterSpotPositions where arsp.ActiveRosterSpot.UsesEase && arsp.PositionId == defPosition.PositionId select arsp.ActiveRosterSpot).FirstOrDefault();
                        if (activeRosterSpot != null)
                        {
                            StatPlayer statPlayer = (from sp in statPlayers where sp.Team.Id == opponentId && sp.TeamActiveRosterSpotId == activeRosterSpot.Id select sp).FirstOrDefault();
                            if (statPlayer == null)
                            {
                                statPlayer = new StatPlayer();
                                statPlayer.Player = new Player();
                                statPlayer.Player.Id = opponentId;
                                statPlayer.TeamActiveRosterSpotId = activeRosterSpot.Id;
                                statPlayer.Team = (from t in teams where t.Id == opponentId select t).FirstOrDefault();
                                statPlayers.Add(statPlayer);
                            }

                            statPlayer.Games++;
                            statPlayer.Set(gamesCat.Id, Convert.ToDouble(statPlayer.Games));
                            foreach (var cat in statCategories)
                            {
                                if (row.GetType().GetProperty(cat.SourceField) != null)
                                {
                                    double val = Convert.ToDouble(row.GetType().GetProperty(cat.SourceField).GetValue(row, null));
                                    statPlayer.Set(cat.Id, statPlayer.Get(cat.Id) + val);
                                }
                            }
                        }
                    }
                }

                foreach (var pg in (from s in statPlayers orderby s.Player.Id select s))
                {
                    pg.FillCalculated(Sport, cats, playerType);
                    pg.FillPerValueStats(perValues, cats);
                }

            }

            return statPlayers;
        }

        public List<StatPlayer> GetTeamStatPlayers(PlayerType playerType, int seasonId, DateTime startDate, DateTime endDate)
        {
            var season = GetSeason(seasonId);
            List<StatPlayer> statsPlayers = new List<StatPlayer>();
            List<Category> cats = GetCategories(playerType);
            List<PerValue> perValues = GetPerValues(playerType.Id);
            Category gamesCat = GetGamesCategory(playerType.Id);
            Category startsCat = GetStartsCategory(playerType.Id);

            List<Player> players = GetPlayers();
            List<SeasonPlayer> seasonPlayers = GetSeasonPlayers(GetSeason(seasonId), playerType);
            List<Team> teams = GetTeams();
            List<PlayerGamePosition> playerGamePositions = GetPlayerGamePositions(playerType, seasonId, startDate, endDate);
            var games = GetGames(season);
            var analyzedGames = (from g in games where g.GameDate >= startDate && g.GameDate <= endDate select g).ToList();

            StatPlayerLib statPlayerLib = new StatPlayerLib();


            if (Sport.IsNBA)
            {
                var playerGames = (from pg in db.NBAPlayerGames.AsNoTracking()
                                   .Include(i => i.Game)
                                   join g in db.Games.AsNoTracking() on pg.GameId equals g.Id
                                   where g.SeasonId == seasonId && g.GameDate >= startDate && g.GameDate <= endDate
                                   select pg).ToList();
                var q = (from bpg in playerGames
                         join g in analyzedGames on bpg.GameId equals g.Id
                         join pgp in playerGamePositions on bpg.PlayerId equals pgp.PlayerId
                         where g.Season.Id == seasonId
                         group bpg by new { bpg.TeamId, pgp.PositionId } into groupResult
                         select new
                         {
                             PositionId = groupResult.Key.PositionId,
                             TeamId = groupResult.Key.TeamId,
                             Games = groupResult.Count(),
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
                         }).ToList();

                statsPlayers = statPlayerLib.GetTeamStatPlayersFromTable(q, teams, analyzedGames, cats, gamesCat);
            }

            if (Sport.IsNFL && playerType.Title == "Offensive")
            {
                var playerGames = (from pg in db.NFLOffensiveGame.AsNoTracking()
                                   .Include(i => i.Game)
                                   join g in db.Games.AsNoTracking() on pg.GameId equals g.Id
                                   where g.SeasonId == seasonId && g.GameDate >= startDate && g.GameDate <= endDate
                                   select pg).ToList();

                var q = (from bpg in playerGames
                         join g in analyzedGames on bpg.GameId equals g.Id
                         join pgp in playerGamePositions on bpg.PlayerId equals pgp.PlayerId
                         where g.Season.Id == seasonId
                         group bpg by new { bpg.TeamId, pgp.PositionId } into groupResult
                         select new
                         {
                             PositionId = groupResult.Key.PositionId,
                             TeamId = groupResult.Key.TeamId,
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
                         }).ToList();

                statsPlayers = statPlayerLib.GetTeamStatPlayersFromTable(q, teams, analyzedGames, cats, gamesCat);
            }

            if (Sport.IsNFL && playerType.Title == "Kickers")
            {
                var playerGames = (from pg in db.NFLKickerGames.AsNoTracking()
                   .Include(i => i.Game)
                                   join g in db.Games.AsNoTracking() on pg.GameId equals g.Id
                                   where g.SeasonId == seasonId && g.GameDate >= startDate && g.GameDate <= endDate
                                   select pg).ToList();

                var q = (from bpg in playerGames
                         join g in analyzedGames on bpg.GameId equals g.Id
                         join pgp in playerGamePositions on bpg.PlayerId equals pgp.PlayerId
                         where g.Season.Id == seasonId
                         group bpg by new { bpg.TeamId, pgp.PositionId } into groupResult
                         select new
                         {
                             PositionId = groupResult.Key.PositionId,
                             TeamId = groupResult.Key.TeamId,
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
                         }).ToList();

                statsPlayers = statPlayerLib.GetTeamStatPlayersFromTable(q, teams, analyzedGames, cats, gamesCat);

            }

            if (Sport.IsNFL && playerType.Title == "Defense")
            {
                var playerGames = (from pg in db.NFLDefenseGames.AsNoTracking()
                   .Include(i => i.Game)
                                   join g in db.Games.AsNoTracking() on pg.GameId equals g.Id
                                   where g.SeasonId == seasonId && g.GameDate >= startDate && g.GameDate <= endDate
                                   select pg).ToList();

                var q = (from bpg in playerGames
                         join g in analyzedGames on bpg.GameId equals g.Id
                         join pgp in playerGamePositions on bpg.PlayerId equals pgp.PlayerId
                         where g.Season.Id == seasonId
                         group bpg by new { bpg.TeamId, pgp.PositionId } into groupResult
                         select new
                         {
                             PositionId = groupResult.Key.PositionId,
                             TeamId = groupResult.Key.TeamId,
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
                         }).ToList();

                statsPlayers = statPlayerLib.GetTeamStatPlayersFromTable(q, teams, analyzedGames, cats, gamesCat);
            }

            if (Sport.IsNHL && playerType.Title == "Skaters")
            {
                var playerGames = (from pg in db.NHLSkaterGames.AsNoTracking()
                   .Include(i => i.Game)
                                   join g in db.Games.AsNoTracking() on pg.GameId equals g.Id
                                   where g.SeasonId == seasonId && g.GameDate >= startDate && g.GameDate <= endDate
                                   select pg).ToList();
                var q = (from bpg in playerGames
                         join g in analyzedGames on bpg.GameId equals g.Id
                         join pgp in playerGamePositions on bpg.PlayerId equals pgp.PlayerId
                         where g.Season.Id == seasonId
                         group bpg by new { bpg.TeamId, pgp.PositionId } into groupResult
                         select new
                         {
                             PositionId = groupResult.Key.PositionId,
                             TeamId = groupResult.Key.TeamId,
                             Games = groupResult.Count(),
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

                statsPlayers = statPlayerLib.GetTeamStatPlayersFromTable(q, teams, analyzedGames, cats, gamesCat);
            }

            if (Sport.IsNHL && playerType.Title == "Goalies")
            {
                var playerGames = (from pg in db.NHLGoalieGames.AsNoTracking()
                   .Include(i => i.Game)
                                   join g in db.Games.AsNoTracking() on pg.GameId equals g.Id
                                   where g.SeasonId == seasonId && g.GameDate >= startDate && g.GameDate <= endDate
                                   select pg).ToList();
                var q = (from bpg in playerGames
                         join g in analyzedGames on bpg.GameId equals g.Id
                         join pgp in playerGamePositions on bpg.PlayerId equals pgp.PlayerId
                         where g.Season.Id == seasonId
                         group bpg by new { bpg.TeamId, pgp.PositionId } into groupResult
                         select new
                         {
                             PositionId = groupResult.Key.PositionId,
                             TeamId = groupResult.Key.TeamId,
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

                statsPlayers = statPlayerLib.GetTeamStatPlayersFromTable(q, teams, analyzedGames, cats, gamesCat);
            }

            foreach (var pg in (from s in statsPlayers orderby s.Player.Id select s))
            {
                pg.FillCalculated(Sport, cats, playerType);
                pg.FillPerValueStats(perValues, cats);
            }

            return statsPlayers;
        }

        public List<StatPlayer> GetStatPlayers(PlayerType playerType, int seasonId, DateTime startDate, DateTime endDate, bool finishedOnly, Game exactGame = null, bool skipCache = false)
        {
            string cacheId = String.Format("GetStatPlayers:S{0}:PT{1}:SD{2}:ED{3}:PG{4}:F{5}",
                seasonId,
                playerType.Id,
                startDate.ToShortDateString(),
                endDate.ToShortDateString(),
                exactGame != null ? exactGame.Id.ToString() : "",
                finishedOnly.ToString());

            if (!skipCache)
            {
                if (CacheItemExists(cacheId))
                    return (List<StatPlayer>)GetCacheItem(cacheId);
            }

            Category measureCategory = GetMeasureCategory(playerType.Id);
            StatPlayerLib statPlayerLib = new StatPlayerLib();
            Season season = GetSeason(seasonId);
            List<Category> cats = GetCategories(playerType);
            var q = GetSportDbLib().GetStats(playerType, season, startDate, endDate, finishedOnly, exactGame);
            List<StatPlayer> statsPlayers = statPlayerLib.GetStatPlayersFromTable(q, GetPlayers(), GetSeasonPlayers(season, playerType), GetTeams(), cats, GetGamesCategory(playerType.Id));
            List<PerValue> perValues = GetPerValues(playerType.Id);
            var gamePerValue = (from pv in perValues where pv.CategoryId == GetGamesCategory(playerType.Id).Id select pv).FirstOrDefault();
            foreach (var pg in (from s in statsPlayers orderby s.Player.Id select s))
            {
                pg.FillCalculated(Sport, cats, playerType);
                pg.FillPerValueStats(perValues, cats);

                if (measureCategory != null && gamePerValue != null)
                {
                    pg.MeasureCategory = measureCategory;
                    pg.MeasureText = String.Format("{0:#######0}", pg.Get(gamePerValue, measureCategory.Id));
                }
            }

            if (!skipCache)
                AddCacheItem(cacheId, statsPlayers);

            return statsPlayers;
        }

        public List<GameLogGame> GetPlayerStatPlayerGameLog(UserLeague userLeague, Player player, PlayerType playerType, PerValue perGamePerValue, ValueAverages perGameValueAverages, Season season, List<ValuePlayer> teamEaseValuePlayers)
        {
            ValuePlayerLib valuePlayerLib = new ValuePlayerLib();

            var gameLogGames = new List<GameLogGame>();
            var seasonPlayer = GetSeasonPlayer(player.Id, playerType, season);
            if (seasonPlayer == null)
                return gameLogGames;

            var games = (from g in GetGames(season)
                         where g.IsFinished && (g.AwayTeam.Id == seasonPlayer.TeamId || g.HomeTeam.Id == seasonPlayer.TeamId)
                         select g).ToList();

            if (seasonPlayer == null)
                return new List<GameLogGame>();

            if (Sport.Title == "NBA")
            {
                var playerGames = (from pg in db.NBAPlayerGames.AsNoTracking()
                   .Include(g => g.Game).ThenInclude(g2 => g2.AwayTeam)
                   .Include(g => g.Game).ThenInclude(g2 => g2.HomeTeam)
                   .Include(p => p.Player)
                                   join g in db.Games.AsNoTracking() on pg.GameId equals g.Id
                                   where pg.PlayerId == player.Id && g.Season.Id == season.Id
                                   orderby g.GameDate, g.Number
                                   select pg
                    ).ToList();
                foreach (var playedGames in playerGames)
                {
                    GameLogGame logGame = new GameLogGame();
                    logGame.Game = playedGames.Game;
                    logGame.Player = playedGames.Player;
                    var statPlayers = GetStatPlayers(playerType, season.Id, logGame.Game.GameDate, logGame.Game.GameDate, true);
                    logGame.StatPlayer = statPlayers.Find(p => p.Player.Id == playedGames.Player.Id);

                    gameLogGames.Add(logGame);
                }
            }

            if (Sport.Title == "MLB")
            {
                if (playerType.Title == "Hitters")
                {
                    var playerGames = (from pg in db.MLBHitterGames.AsNoTracking()
                                       .Include(g => g.Game).ThenInclude(g2 => g2.AwayTeam)
                                       .Include(g => g.Game).ThenInclude(g2 => g2.HomeTeam)
                                       .Include(p => p.Player)
                                       join g in db.Games.AsNoTracking() on pg.GameId equals g.Id
                                       where pg.PlayerId == player.Id && g.Season.Id == season.Id
                                       orderby g.GameDate, g.Number
                                       select pg
                           ).ToList();
                    foreach (var playedGames in playerGames)
                    {
                        GameLogGame logGame = new GameLogGame();
                        logGame.Game = playedGames.Game;
                        logGame.Player = playedGames.Player;
                        var statPlayers = GetStatPlayers(playerType, season.Id, logGame.Game.GameDate, logGame.Game.GameDate, true);
                        logGame.StatPlayer = statPlayers.Find(p => p.Player.Id == playedGames.Player.Id);

                        gameLogGames.Add(logGame);
                    }
                }   // end hitters

                else if (playerType.Title == "Pitchers")
                {
                    var playerGames = (from pg in db.MLBPitcherGames.AsNoTracking()
                                       .Include(g => g.Game).ThenInclude(g2 => g2.AwayTeam)
                                       .Include(g => g.Game).ThenInclude(g2 => g2.HomeTeam)
                                       .Include(p => p.Player)
                                       join g in db.Games.AsNoTracking() on pg.GameId equals g.Id
                                       where pg.PlayerId == player.Id && g.Season.Id == season.Id
                                       orderby g.GameDate, g.Number
                                       select pg
                           ).ToList();
                    foreach (var playedGames in playerGames)
                    {
                        GameLogGame logGame = new GameLogGame();
                        logGame.Game = playedGames.Game;
                        logGame.Player = playedGames.Player;
                        var statPlayers = GetStatPlayers(playerType, season.Id, logGame.Game.GameDate, logGame.Game.GameDate, true);
                        logGame.StatPlayer = statPlayers.Find(p => p.Player.Id == playedGames.Player.Id);

                        gameLogGames.Add(logGame);
                    }
                }   // end pitchers
            }

            if (Sport.Title == "NFL")
            {
                if (playerType.Title == "Defense")
                {
                    var playerGames = (from pg in db.NFLDefenseGames.AsNoTracking()
                                       .Include(g => g.Game).ThenInclude(g2 => g2.AwayTeam)
                                       .Include(g => g.Game).ThenInclude(g2 => g2.HomeTeam)
                                       .Include(p => p.Player)
                                       join g in db.Games.AsNoTracking() on pg.GameId equals g.Id
                                       where pg.PlayerId == player.Id && g.Season.Id == season.Id
                                       orderby g.GameDate, g.Number
                                       select pg
                           ).ToList();
                    foreach (var playedGames in playerGames)
                    {
                        GameLogGame logGame = new GameLogGame();
                        logGame.Game = playedGames.Game;
                        logGame.Player = playedGames.Player;
                        var statPlayers = GetStatPlayers(playerType, season.Id, logGame.Game.GameDate, logGame.Game.GameDate, true);
                        logGame.StatPlayer = statPlayers.Find(p => p.Player.Id == playedGames.Player.Id);

                        gameLogGames.Add(logGame);
                    }
                }   // end defense

                if (playerType.Title == "Offensive")
                {
                    var playerGames = (from pg in db.NFLOffensiveGame.AsNoTracking()
                                       .Include(g => g.Game).ThenInclude(g2 => g2.AwayTeam)
                                       .Include(g => g.Game).ThenInclude(g2 => g2.HomeTeam)
                                       .Include(p => p.Player)
                                       join g in db.Games.AsNoTracking() on pg.GameId equals g.Id
                                       where pg.PlayerId == player.Id && g.Season.Id == season.Id
                                       orderby g.GameDate, g.Number
                                       select pg
                           ).ToList();
                    foreach (var playedGames in playerGames)
                    {
                        GameLogGame logGame = new GameLogGame();
                        logGame.Game = playedGames.Game;
                        logGame.Player = playedGames.Player;
                        var statPlayers = GetStatPlayers(playerType, season.Id, logGame.Game.GameDate, logGame.Game.GameDate, true);
                        logGame.StatPlayer = statPlayers.Find(p => p.Player.Id == playedGames.Player.Id);

                        gameLogGames.Add(logGame);
                    }
                }   // end kickers

                if (playerType.Title == "Kickers")
                {
                    var playerGames = (from pg in db.NFLKickerGames.AsNoTracking()
                                       .Include(g => g.Game).ThenInclude(g2 => g2.AwayTeam)
                                       .Include(g => g.Game).ThenInclude(g2 => g2.HomeTeam)
                                       .Include(p => p.Player)
                                       join g in db.Games.AsNoTracking() on pg.GameId equals g.Id
                                       where pg.PlayerId == player.Id && g.Season.Id == season.Id
                                       orderby g.GameDate, g.Number
                                       select pg
                           ).ToList();
                    foreach (var playedGames in playerGames)
                    {
                        GameLogGame logGame = new GameLogGame();
                        logGame.Game = playedGames.Game;
                        logGame.Player = playedGames.Player;
                        var statPlayers = GetStatPlayers(playerType, season.Id, logGame.Game.GameDate, logGame.Game.GameDate, true);
                        logGame.StatPlayer = statPlayers.Find(p => p.Player.Id == playedGames.Player.Id);

                        gameLogGames.Add(logGame);
                    }
                }   // end kickers

            }

            if (Sport.IsNHL && playerType.Title == "Skater")
            {
                var playerGames = (from pg in db.NHLSkaterGames.AsNoTracking()
                                   .Include(g => g.Game).ThenInclude(g2 => g2.AwayTeam)
                                   .Include(g => g.Game).ThenInclude(g2 => g2.HomeTeam)
                                   .Include(p => p.Player)
                                   join g in db.Games.AsNoTracking() on pg.GameId equals g.Id
                                   where pg.PlayerId == player.Id && g.Season.Id == season.Id
                                   orderby g.GameDate, g.Number
                                   select pg
                       ).ToList();
                foreach (var playedGames in playerGames)
                {
                    GameLogGame logGame = new GameLogGame();
                    logGame.Game = playedGames.Game;
                    logGame.Player = playedGames.Player;
                    var statPlayers = GetStatPlayers(playerType, season.Id, logGame.Game.GameDate, logGame.Game.GameDate, true);
                    logGame.StatPlayer = statPlayers.Find(p => p.Player.Id == playedGames.Player.Id);

                    gameLogGames.Add(logGame);
                }
            }

            if (Sport.IsNHL && playerType.Title == "Goalie")
            {
                var playerGames = (from pg in db.NHLGoalieGames.AsNoTracking()
                                   .Include(g => g.Game).ThenInclude(g2 => g2.AwayTeam)
                                   .Include(g => g.Game).ThenInclude(g2 => g2.HomeTeam)
                                   .Include(p => p.Player)
                                   join g in db.Games.AsNoTracking() on pg.GameId equals g.Id
                                   where pg.PlayerId == player.Id && g.Season.Id == season.Id
                                   orderby g.GameDate, g.Number
                                   select pg
                       ).ToList();
                foreach (var playedGames in playerGames)
                {
                    GameLogGame logGame = new GameLogGame();
                    logGame.Game = playedGames.Game;
                    logGame.Player = playedGames.Player;
                    var statPlayers = GetStatPlayers(playerType, season.Id, logGame.Game.GameDate, logGame.Game.GameDate, true);
                    logGame.StatPlayer = statPlayers.Find(p => p.Player.Id == playedGames.Player.Id);

                    gameLogGames.Add(logGame);
                }
            }

            // calculate game values
            var catSettings = userLeague.GetCategorySettings(playerType);
            foreach (var pg in gameLogGames)
            {
                if (pg.StatPlayer == null)
                    continue;

                var statPlayers = new List<StatPlayer>();
                statPlayers.Add(pg.StatPlayer);
                pg.ValuePlayer = new ValuePlayer();
                pg.ValuePlayer.StatPlayer = pg.StatPlayer;
                var activeRosterSpot = GetEaseActiveRosterSpot(GetPlayerDefaultPosition(pg.StatPlayer.Player.Id).Position);
                pg.EaseValuePlayer = valuePlayerLib.GetTeamValuePlayer(teamEaseValuePlayers, pg.Game.GetOpponent(pg.StatPlayer.Team), activeRosterSpot);

                if (userLeague.ScoringSystem == "C")
                {
                    var valuePlayers = new List<ValuePlayer>();
                    valuePlayers.Add(pg.ValuePlayer);
                    valuePlayerLib.FillCategoryValuePlayersAndColors(valuePlayers, statPlayers, catSettings, perGamePerValue, perGameValueAverages);
                }
                else
                {
                    pg.ValuePlayer = valuePlayerLib.GetPointsValuePlayer(pg.StatPlayer, perGamePerValue, catSettings);
                    var valuePlayers = new List<ValuePlayer>();
                    valuePlayers.Add(pg.ValuePlayer);
                    valuePlayerLib.FillPointsValuePlayersAndColors(valuePlayers, perGameValueAverages, catSettings);
                }
            }

            var playerMissedGames = (from pg in db.PlayedGamesMissed.AsNoTracking()
                   .Include(g => g.Game).ThenInclude(g2 => g2.AwayTeam)
                   .Include(g => g.Game).ThenInclude(g2 => g2.HomeTeam)
                   .Include(p => p.Player)
                                     join g in db.Games.AsNoTracking() on pg.GameId equals g.Id
                                     where pg.PlayerId == player.Id && g.Season.Id == season.Id
                                     orderby g.GameDate, g.Number
                                     select pg
                    ).ToList();
            foreach (var missedGames in playerMissedGames)
            {
                GameLogGame logGame = new GameLogGame();
                logGame.Game = missedGames.Game;
                logGame.Player = missedGames.Player;
                logGame.PlayerGameMissed = missedGames;
                gameLogGames.Add(logGame);
            }

            string scoringSystem = GetUserLeagueScoringSystem(userLeague);
            int leagueSize = GetUserLeagueLeagueSize(userLeague, playerType);

            var logStatPlayers = new List<StatPlayer>();
            foreach (var game in games)
            {
                var findGame = gameLogGames.Find(g => g.Game.Id == game.Id);
                if (findGame == null)
                {
                    GameLogGame logGame = new GameLogGame();
                    logGame.Game = game;
                    logGame.Player = player;
                    logGame.PlayerGameMissed = new PlayerGameMissed();
                    logGame.PlayerGameMissed.Player = player;
                    logGame.PlayerGameMissed.Game = game;
                    logStatPlayers.Add(logGame.StatPlayer);
                    gameLogGames.Add(logGame);
                }
            }

            gameLogGames = (from lg in gameLogGames orderby lg.Game.GameDate descending, lg.Game.Number descending select lg).ToList();

            return gameLogGames;
        }

        public Season GetSeason(int seasonId)
        {
            string cacheId = "GetSeason" + seasonId.ToString();
            if (CacheItemExists(cacheId))
                return (Season)GetCacheItem(cacheId);

            var season = (from s in db.Seasons.AsNoTracking() where s.Id == seasonId select s)
                .Include(a => a.SeasonTeams).ThenInclude(t => t.Team)
                .FirstOrDefault();

            if (season != null)
            {
                var games = GetGames(season);
                var startedGame = (from g in games where g.HasStarted orderby g.GameDate descending select g).FirstOrDefault();
                season.HasStarted = (startedGame != null);
                var finishedGames = (from g in games where g.IsFinished orderby g.GameDate descending select g).ToList();
                season.IsFinished = (finishedGames.Count == games.Count);
                season.UpdatedDate = (finishedGames.Count > 0 ? finishedGames.First().GameDate : season.StartDate);
                if (season.IsFinished)
                    season.State = "Finished";
                else if (!season.HasStarted)
                {
                    var timeUntil = games.First().GameDate - DateTime.Today;
                    season.State = "Starts in " + String.Format("{0:##0}", timeUntil.TotalDays) + " days";
                }
                else
                {
                    int finished = games.Where(g => g.IsFinished).Count();
                    int remaining = games.Count - finished;
                    double percent = 0;
                    if (games.Count > 0)
                        percent = (double)finished / (double)games.Count * 100;
                    season.State = String.Format("{0:##0}% Complete", percent);
                }
            }

            AddCacheItem(cacheId, season);

            return season;
        }

        public Category GetGamesCategory(int playerTypeId)
        {
            string cacheId = "GetGamesCategory" + playerTypeId.ToString();
            if (CacheItemExists(cacheId))
                return (Category)GetCacheItem(cacheId);

            var q = (from c in GetCategories() where c.PlayerType.Id == playerTypeId && c.Title == "Games" select c).FirstOrDefault();

            AddCacheItem(cacheId, q);

            return q;
        }

        public Category GetMeasureCategory(int playerTypeId)
        {
            string cacheId = "GetMeasureCategory" + playerTypeId.ToString();
            if (CacheItemExists(cacheId))
                return (Category)GetCacheItem(cacheId);

            var q = (from c in GetCategories() where c.PlayerType.Id == playerTypeId && c.IsMeasureCategory select c).FirstOrDefault();

            AddCacheItem(cacheId, q);

            return q;
        }

        public Category GetStartsCategory(int playerTypeId)
        {
            string cacheId = "GetStartsCategory" + playerTypeId.ToString();
            if (CacheItemExists(cacheId))
                return (Category)GetCacheItem(cacheId);

            var q = (from c in GetCategories() where c.PlayerType.Id == playerTypeId && (c.Title == "Starts" || c.Title == "Games Started") select c).FirstOrDefault();

            AddCacheItem(cacheId, q);

            return q;
        }

        public Season GetDefaultSeason()
        {
            string cacheId = "GetDefaultSeason";
            if (CacheItemExists(cacheId))
                return (Season)GetCacheItem(cacheId);

            var seasonId = (from s in db.Seasons.AsNoTracking() where s.IsRegularSeason.HasValue && s.IsEnabled orderby s.Year descending select s.Id)
                .FirstOrDefault();

            var season = GetSeason(seasonId);

            AddCacheItem(cacheId, season);

            return season;
        }

        public bool IsSeasonComplete(Season season)
        {
            string cacheId = "IsInSeason" + season.Id.ToString();
            if (CacheItemExists(cacheId))
                return (bool)GetCacheItem(cacheId);
            var games = GetGames(season);
            var unfinishedGames = (from g in games where !g.IsFinished select g).ToList();

            bool isComplete = (unfinishedGames.Count == 0);

            AddCacheItem(cacheId, isComplete);

            return isComplete;
        }

        public UserLeague GetDefaultUserLeague()
        {
            var userLeague = (from ul in db.UserLeagues where ul.IsDefault select ul).FirstOrDefault();

            if (userLeague != null)
                return GetUserLeague(userLeague.Id);

            return null;
        }

        public UserLeague GetUserLeague(int id)
        {
            var userLeague = (from ul in db.UserLeagues.Include(i => i.FantasyProvider).AsNoTracking()
                              where ul.Id == id
                              select ul).FirstOrDefault();

            if (userLeague != null)
            {
                userLeague.UserLeagueActiveRosterSpots = (from ars in db.UserLeagueActiveRosterSpots
                                                      .Include(i => i.ActiveRosterSpot).ThenInclude(t => t.ActiveRosterSpotPositions).ThenInclude(t2 => t2.Position)
                                                          where ars.UserLeagueId == userLeague.Id
                                                          orderby ars.ActiveRosterSpot.DisplayOrder
                                                          select ars).ToList();
                userLeague.UserLeagueCategories = (from ulc in db.UserLeagueCategories
                                               .Include(i => i.Category).ThenInclude(t => t.PlayerType)
                                               .Include(i => i.Category).ThenInclude(t => t.WeightCategory)
                                               .Include(i => i.Category).ThenInclude(t => t.CategoryPerValues)
                                                   where ulc.UserLeagueId == userLeague.Id
                                                   orderby ulc.Category.DisplayOrder
                                                   select ulc).ToList();
                userLeague.UserLeaguePlayerTypes = (from pt in db.UserLeaguePlayerTypes.Include(i => i.PlayerType).Include(i => i.CategoriesString)
                                                    where pt.UserLeagueId == userLeague.Id
                                                    orderby pt.PlayerType.DisplayOrder
                                                    select pt
                                              ).ToList();
                userLeague.UserLeagueTeams = GetUserLeagueTeams(userLeague);
                var teamPlayers = GetUserLeagueTeamPlayers(userLeague);
                foreach (var t in userLeague.UserLeagueTeams)
                    t.UserLeagueTeamPlayers = (from p in teamPlayers where p.UserLeagueTeamId == t.Id select p).ToList();
            }

            return userLeague;
        }


        public List<UserLeague> GetUserLeagues(string userId)
        {
            if (userId == null)
                return new List<UserLeague>();

            var leagues = (from l in db.UserLeagues where l.SeasonId == GetDefaultSeason().Id && l.UserId == userId orderby l.DisplayTitle, l.Title select l)
                .Include(a => a.FantasyProvider)
                .ToList();

            var outLeagues = new List<UserLeague>();
            foreach (var league in leagues)
                outLeagues.Add(GetUserLeague(league.Id));

            return outLeagues;
        }

        public List<UserLeague> GetTrackedUserLeagues(string userId)
        {
            return (from ul in GetUserLeagues(userId) where ul.TrackLeague select ul).ToList();
        }

        public List<UserLeague> GetUserLeagues()
        {
            return db.UserLeagues.AsNoTracking()
                .Include(a => a.Season)
                .Include(a => a.FantasyProvider)
                .ToList();
        }

        public FantasyProvider GetFantasyProvider(int id)
        {
            string cacheId = "GetFantasyProvider" + id.ToString();
            if (CacheItemExists(cacheId))
                return (FantasyProvider)GetCacheItem(cacheId);

            var provider = (from p in db.FantasyProviders.AsNoTracking() where p.Id == id select p).FirstOrDefault();

            AddCacheItem(cacheId, provider);

            return provider;
        }

        public FantasyProvider GetFantasyProvider(string providerName)
        {
            string cacheId = "GetFantasyProvider" + providerName;
            if (CacheItemExists(cacheId))
                return (FantasyProvider)GetCacheItem(cacheId);

            var provider = (from p in db.FantasyProviders where p.Name.ToLower().Contains(providerName.ToLower()) select p).FirstOrDefault();

            if (provider != null)
            {
                AddCacheItem(cacheId, provider);

                return GetFantasyProvider(provider.Id);
            }
            else
                return null;
        }

        public List<ActiveRosterSpot> GetActiveRosterSpots()
        {
            string cacheId = "GetActiveRosterSpots";
            if (CacheItemExists(cacheId))
                return (List<ActiveRosterSpot>)GetCacheItem(cacheId);

            var activeRosterSpots = db.ActiveRosterSpots.AsNoTracking().ToList();

            AddCacheItem(cacheId, activeRosterSpots);

            return activeRosterSpots;
        }

        public List<ActiveRosterSpotPosition> GetActiveRosterSpotPositions()
        {
            string cacheId = "GetActiveRosterSpotPositions";
            if (CacheItemExists(cacheId))
                return (List<ActiveRosterSpotPosition>)GetCacheItem(cacheId);

            var activeRosterSpotPositions = db.ActiveRosterSpotPositions.AsNoTracking()
                .Include(i => i.ActiveRosterSpot).AsNoTracking()
                .Include(i => i.Position).AsNoTracking().ToList();

            AddCacheItem(cacheId, activeRosterSpotPositions);

            return activeRosterSpotPositions;
        }

        public List<Category> GetCategories()
        {
            string cacheId = "GetCategories";
            if (CacheItemExists(cacheId))
                return (List<Category>)GetCacheItem(cacheId);

            var cats = (from c in db.Categories.AsNoTracking() orderby c.DisplayOrder ascending select c)
                .Include(i => i.PlayerType).AsNoTracking()
                .Include(a => a.CategoryPerValues).AsNoTracking()
                .ToList();

            AddCacheItem(cacheId, cats);

            return cats;
        }

        public List<Category> GetValueCategories()
        {
            string cacheId = "GetValueCategories";
            if (CacheItemExists(cacheId))
                return (List<Category>)GetCacheItem(cacheId);

            var categories = (from c in GetCategories() where c.UseAsValue.HasValue && c.UseAsValue.Value orderby c.PlayerType.DisplayOrder, c.DisplayOrder ascending select c).ToList();

            AddCacheItem(cacheId, categories);

            return categories;
        }

        public List<Category> GetPointCategories()
        {
            string cacheId = "GetPointCategories";
            if (CacheItemExists(cacheId))
                return (List<Category>)GetCacheItem(cacheId);

            var categories = (from c in GetCategories() where c.UseAsValue.HasValue && c.UseAsValue.Value && c.WeightCategoryId == null orderby c.PlayerType.DisplayOrder, c.DisplayOrder ascending select c).ToList();

            AddCacheItem(cacheId, categories);

            return categories;
        }

        public List<Player> GetPlayers()
        {
            string cacheId = "GetPlayers";
            if (CacheItemExists(cacheId))
                return (List<Player>)GetCacheItem(cacheId);

            var players = (from p in db.Players.AsNoTracking()
                           .Include(i => i.PlayerDefaultPositions)
                           .ThenInclude(t => t.Position).AsNoTracking()
                           orderby p.LastName, p.FirstName, p.Birthdate
                           select p).ToList();

            AddCacheItem(cacheId, players);

            return players;
        }

        public Player GetPlayer(int playerId)
        {
            var player = (from p in GetPlayers() where p.Id == playerId select p).FirstOrDefault();

            return player;
        }

        public List<FantasyProviderPlayer> GetFantasyProviderPlayers(FantasyProvider fantasyProvider)
        {
            string cacheId = "GetFantasyProviderPlayers" + fantasyProvider.Id.ToString();
            if (CacheItemExists(cacheId))
                return (List<FantasyProviderPlayer>)GetCacheItem(cacheId);

            var players = (from p in db.FantasyProviderPlayers
                           .Include(i => i.Player)
                           .AsNoTracking()
                           where p.FantasyProvider.Id == fantasyProvider.Id
                           select p).Include(a => a.FantasyProvider).Include(p => p.Player).ToList();
            AddCacheItem(cacheId, players);

            return players;
        }

        public void DeleteUserLeague(int userLeagueId)
        {
            foreach (var ulc in (from ulc1 in db.UserLeagueCategories where ulc1.UserLeagueId == userLeagueId select ulc1))
                db.Remove(ulc);
            foreach (var ars in (from ars1 in db.UserLeagueActiveRosterSpots where ars1.UserLeagueId == userLeagueId select ars1))
                db.Remove(ars);
            foreach (var pt in (from ars1 in db.UserLeaguePlayerTypes where ars1.UserLeagueId == userLeagueId select ars1))
                db.Remove(pt);
            foreach (var err in (from ars1 in db.UserLeagueImportErrors where ars1.UserLeagueId == userLeagueId select ars1))
                db.Remove(err);
            foreach (var o in (from ars1 in db.UserLeagueMissingPlayers where ars1.UserLeagueId == userLeagueId select ars1))
                db.Remove(o);
            foreach (var o in (from ars1 in db.UserLeagueWaiverPlayers where ars1.UserLeagueId == userLeagueId select ars1))
                db.Remove(o);
            db.SaveChanges();

            UserLeague ul = (from u in db.UserLeagues where u.Id == userLeagueId select u).FirstOrDefault();
            if (ul != null)
            {
                db.UserLeagues.Remove(ul);
                db.SaveChanges();
            }
        }

        public UserLeague GetUserLeague(string userId, int id)
        {
            if (userId == null || id == 0)
                return null;

            var league = (from u in GetUserLeagues(userId) where u.Id == id select u).FirstOrDefault();

            return league;
        }

        public UserLeague AddUserLeague(UserLeague userLeague)
        {
            userLeague.CreatedDate = DateTime.UtcNow;
            userLeague.FillUserLeagueCategoriesCode(GetCategories());
            db.UserLeagues.Add(userLeague);
            db.SaveChanges();
            UpdateUserLeagueUpdatedDate(userLeague.Id, userLeague.CreatedDate.GetValueOrDefault(), false);

            foreach (var ulc in userLeague.UserLeagueCategories)
            {
                ulc.UserLeagueId = userLeague.Id;
                db.UserLeagueCategories.Add(ulc);
            }
            foreach (var ars in userLeague.UserLeagueActiveRosterSpots)
            {
                ars.UserLeague = null;
                ars.UserLeagueId = userLeague.Id;
                db.UserLeagueActiveRosterSpots.Add(ars);
            }
            foreach (var ult in userLeague.UserLeagueTeams)
            {
                ult.UserLeague = null;
                ult.Id = 0;
                ult.UserLeagueId = userLeague.Id;
                db.UserLeagueTeams.Add(ult);
            }
            foreach (var pt in userLeague.UserLeaguePlayerTypes)
            {
                pt.UserLeague = null;
                pt.UserLeagueId = userLeague.Id;
                pt.CategoriesStringId = GetCategoriesString(pt.CategoriesCode1).Id;
                db.UserLeaguePlayerTypes.Add(pt);
            }
            foreach (var err in userLeague.UserLeagueImportErrors)
            {
                err.UserLeague = null;
                err.UserLeagueId = userLeague.Id;
                db.UserLeagueImportErrors.Add(err);
            }
            db.SaveChanges();

            return userLeague;
        }

        public UserLeague UpdateUserLeague(UserLeague userLeague)
        {
            userLeague.FillUserLeagueCategoriesCode(GetCategories());
            db.Update(userLeague);
            db.SaveChanges();
            UpdateUserLeagueUpdatedDate(userLeague.Id, DateTime.UtcNow, false);

            foreach (var ulc in (from ulc1 in db.UserLeagueCategories where ulc1.UserLeagueId == userLeague.Id select ulc1))
                db.Remove(ulc);
            foreach (var ars in (from ars1 in db.UserLeagueActiveRosterSpots where ars1.UserLeagueId == userLeague.Id select ars1))
                db.Remove(ars);
            foreach (var pt in (from ars1 in db.UserLeaguePlayerTypes where ars1.UserLeagueId == userLeague.Id select ars1))
                db.Remove(pt);
            db.SaveChanges();

            foreach (var ulc in userLeague.UserLeagueCategories)
            {
                ulc.UserLeagueId = userLeague.Id;
                db.UserLeagueCategories.Add(ulc);
            }
            foreach (var ars in userLeague.UserLeagueActiveRosterSpots)
            {
                ars.UserLeagueId = userLeague.Id;
                db.UserLeagueActiveRosterSpots.Add(ars);
            }
            foreach (var pt in userLeague.UserLeaguePlayerTypes)
            {
                pt.UserLeagueId = userLeague.Id;
                pt.CategoriesStringId = GetCategoriesString(pt.CategoriesCode1).Id;
                db.UserLeaguePlayerTypes.Add(pt);
            }

            db.SaveChanges();

            return userLeague;
        }

        public List<Team> GetTeams()
        {
            string cacheId = "GetTeams";
            if (CacheItemExists(cacheId))
                return (List<Team>)GetCacheItem(cacheId);

            var teams = (from t in db.Teams.AsNoTracking()
                         .Include(i => i.TeamAliases).AsNoTracking()
                         orderby t.Code
                         select t).ToList();

            AddCacheItem(cacheId, teams);

            return teams;
        }

        public Team GetTeam(string code)
        {
            string matchCode = code.ToUpper().Trim();

            string cacheId = "GetTeam" + matchCode;
            if (CacheItemExists(cacheId))
                return (Team)GetCacheItem(cacheId);

            Team team = null;
            foreach (var t in GetTeams())
            {
                if (t.Code.ToUpper() == matchCode)
                    team = t;
                if (team == null)
                {
                    foreach (var alias in t.TeamAliases)
                    {
                        if (alias.Alias.ToUpper() == matchCode)
                        {
                            team = t;
                            break;
                        }
                    }
                }
                if (team != null)
                    break;
            }

            AddCacheItem(cacheId, team);

            return team;
        }

        public List<PerValue> GetPerValues(int playerTypeId)
        {
            string cacheId = "GetPerValues" + playerTypeId.ToString();
            if (CacheItemExists(cacheId))
                return (List<PerValue>)GetCacheItem(cacheId);

            var pvs = (from pv in db.PerValues.AsNoTracking() where pv.PlayerTypeId == playerTypeId orderby pv.DisplayOrder select pv).Include(c => c.Category).ToList();

            AddCacheItem(cacheId, pvs);

            return pvs;
        }

        public List<PlayerType> GetPlayerTypes()
        {
            string cacheId = "GetPlayerTypes";
            if (CacheItemExists(cacheId))
                return (List<PlayerType>)GetCacheItem(cacheId);

            var playerTypes = (from pt in db.PlayerTypes.AsNoTracking() where !pt.IsDisabled orderby pt.DisplayOrder select pt).ToList();

            AddCacheItem(cacheId, playerTypes);

            return playerTypes;
        }

        public void FillDisplayPlayerUserLeagueTeams(UserLeague userLeague, List<DisplayPlayer> displayPlayers)
        {
            if (userLeague == null)
                return;

            var leaguePlayers = GetUserLeagueTeamPlayers(userLeague);

            foreach (var dp in displayPlayers)
            {
                var tp = (from p in leaguePlayers
                          where p.PlayerId == dp.SeasonPlayer.Player.Id
                          select p).FirstOrDefault();
                if (tp != null)
                {
                    dp.UserLeagueTeam = tp.UserLeagueTeam;
                    dp.IsMyPlayer = (dp.UserLeagueTeam.ProviderId == userLeague.MyProviderTeamId);
                    dp.IsActive = tp.IsActive;
                    dp.IsIR = tp.IsIR;
                }
            }
        }

        public List<UserLeagueTeam> GetUserLeagueTeams(UserLeague userLeague)
        {
            if (userLeague == null)
                return new List<UserLeagueTeam>();

            var leagueTeams = (from t in db.UserLeagueTeams.AsNoTracking() where t.UserLeagueId == userLeague.Id orderby t.Title select t).ToList();

            return leagueTeams;
        }

        public List<UserLeagueTeamPlayer> GetUserLeagueTeamPlayers(UserLeague userLeague)
        {
            if (userLeague == null)
                return new List<UserLeagueTeamPlayer>();

            var leaguePlayers = (from p in db.UserLeagueTeamPlayers.AsNoTracking()
                                 .Include(i => i.Player)
                                 join team in db.UserLeagueTeams on p.UserLeagueTeamId equals team.Id
                                 where team.UserLeagueId == userLeague.Id
                                 select p).Include(t => t.UserLeagueTeam).ToList();

            return leaguePlayers;
        }


        public List<DisplayCategory> GetBeforeDisplayCategories(PlayerType playerType)
        {
            string cacheId = "GetBeforeDisplayCategories" + playerType.Id.ToString();
            if (CacheItemExists(cacheId))
                return (List<DisplayCategory>)GetCacheItem(cacheId);

            var displayCategories = (from dp in db.DisplayCategories.AsNoTracking()
                     .Include(d => d.Category).ThenInclude(c => c.PlayerType)
                     .Include(d => d.Category).ThenInclude(c => c.CategoryPerValues)
                                     where dp.IsBeforeStats && dp.Category.PlayerType.Id == playerType.Id
                                     orderby dp.DisplayOrder
                                     select dp).ToList();

            AddCacheItem(cacheId, displayCategories);

            return displayCategories;
        }

        public List<DisplayCategory> GetAfterDisplayCategories(PlayerType playerType)
        {
            string cacheId = "GetAfterDisplayCategories" + playerType.Id.ToString();
            if (CacheItemExists(cacheId))
                return (List<DisplayCategory>)GetCacheItem(cacheId);

            var displayCategories = (from dp in db.DisplayCategories.AsNoTracking()
                      .Include(d => d.Category).ThenInclude(c => c.PlayerType)
                      .Include(d => d.Category).ThenInclude(c => c.CategoryPerValues)
                                     where dp.IsAfterStats && dp.Category.PlayerType.Id == playerType.Id
                                     orderby dp.DisplayOrder
                                     select dp).ToList();

            AddCacheItem(cacheId, displayCategories);

            return displayCategories;
        }

        public UserLeague SelectUserLeague(string userId, UserLeague selectThisUserLeague)
        {
            if (selectThisUserLeague == null)
            {
                selectThisUserLeague = (from u in GetUserLeagues(userId)
                                        where u.TrackLeague == true
                                        orderby u.LastSelectedDate descending, u.Title ascending
                                        select u
                            ).FirstOrDefault();
            }

            if (selectThisUserLeague == null)
            {
                selectThisUserLeague = (from u in GetUserLeagues(userId)
                                        orderby u.LastSelectedDate descending, u.Title ascending
                                        select u
                            ).FirstOrDefault();
            }

            if (selectThisUserLeague != null)
            {
                selectThisUserLeague.LastSelectedDate = DateTime.UtcNow;
                db.SaveChanges();
            }

            if (selectThisUserLeague == null)
            {
                selectThisUserLeague = GetDefaultUserLeague();
            }

            return selectThisUserLeague;
        }

        public UserLeague GetNewCustomUserLeague()
        {
            var defaultLeague = GetDefaultUserLeague();
            defaultLeague.Id = 0;
            defaultLeague.Title = "New Custom League";
            defaultLeague.DisplayTitle = defaultLeague.Title;
            defaultLeague.ProviderLeagueId = "";
            defaultLeague.IsProLeague = false;

            defaultLeague.UserLeagueTeams.Clear();

            defaultLeague.FantasyProvider = null;

            return defaultLeague;
        }

        public List<CategorySetting> GetDefaultCategorySettings(PlayerType playerType)
        {
            string cacheId = "GetDefaultCategorySettings" + playerType.Id.ToString();
            if (CacheItemExists(cacheId))
                return (List<CategorySetting>)GetCacheItem(cacheId);

            List<CategorySetting> catSetttings = new List<CategorySetting>();
            foreach (var cat in GetCategories().Where(c => c.IsDefault.HasValue && c.PlayerType.Id == playerType.Id))
            {
                if (cat.IsDefault.GetValueOrDefault(false))
                {
                    CategorySetting catSetting = new CategorySetting();
                    catSetting.Category = cat;
                    catSetting.PointsPerStat = cat.DefaultPointsPerStat.GetValueOrDefault();
                    catSetttings.Add(catSetting);
                }
            }

            AddCacheItem(cacheId, catSetttings);

            return catSetttings;
        }

        public List<SelectListItem> GetPerValuesSelectItems(PlayerType playerType)
        {
            var perValueItems = new List<SelectListItem>();
            foreach (var perType in GetPerValues(playerType.Id))
            {
                if (perType.Category == null)
                    perValueItems.Add(new SelectListItem(perType.Title + " Stats", perType.Id.ToString()));
                else
                    perValueItems.Add(new SelectListItem("Per " + perType.Title + " Stats", perType.Id.ToString()));
            }

            return perValueItems;
        }

        public List<SelectListItem> GetTeamsSelectItems(Season season)
        {
            var teamItems = new List<SelectListItem>();
            teamItems.Add(new SelectListItem("All", "-1"));
            foreach (var team in season.SeasonTeams.OrderBy(s => s.Team.Code))
            {
                teamItems.Add(new SelectListItem(team.Team.Code, team.Team.Id.ToString()));
            }

            return teamItems;
        }

        public async Task<List<SelectListItem>> GetPlayerFilterSelectItems(UserLeague userLeague)
        {
            var filterItems = new List<SelectListItem>();
            filterItems.Add(new SelectListItem("Top Players", "1"));
            filterItems.Add(new SelectListItem("All Players", "2"));
            filterItems.Add(new SelectListItem("Available Players", "3"));
            filterItems.Add(new SelectListItem("My Players", "4"));
            filterItems.Add(new SelectListItem("Available+My Players", "5"));
            foreach (var ult in await GetUserLeagueTeamsAsync(userLeague))
            {
                long listId = ult.Id + 100; // make sure it's higher than regular filter Ids
                filterItems.Add(new SelectListItem("Team: " + ult.Title, listId.ToString()));
            }
            return filterItems;
        }

        public List<SelectListItem> GetProjectionSourceSelectItems()
        {
            var selectItems = new List<SelectListItem>();
            selectItems.Add(new SelectListItem("No Projections", "0"));
            selectItems.Add(new SelectListItem("Project using Full Season averages", "1"));
            selectItems.Add(new SelectListItem("Project using Past Month averages", "2"));
            selectItems.Add(new SelectListItem("Project using Past 3 Week averages", "3"));
            selectItems.Add(new SelectListItem("Project using Past 2 Week averages", "4"));
            selectItems.Add(new SelectListItem("Project using Past Week averages", "5"));

            return selectItems;
        }

        public List<SelectListItem> GetDayOfWeekSelectItems(DateTime startDate, DateTime endDate)
        {
            var dayItems = new List<SelectListItem>();
            dayItems.Add(new SelectListItem("", "0"));

            int cnt = 0;
            var current = startDate;
            while (current <= endDate)
            {
                cnt++;
                dayItems.Add(new SelectListItem(current.DayOfWeek.ToString().Substring(0, 3), cnt.ToString()));
                current = current.AddDays(1);
            }

            return dayItems;
        }

        public List<PositionSourcePlayer> GetPlayerSeasonPositions(FantasyProvider provider, Season season)
        {
            string cacheId = "GetPlayerSeasonPositions"
                + ":P" + provider.Id.ToString()
                + ":S" + season.Id.ToString();
            if (CacheItemExists(cacheId))
                return (List<PositionSourcePlayer>)GetCacheItem(cacheId);

            var positionSource = (from ps1 in db.PositionSources.AsNoTracking() where ps1.ProviderId == provider.Id select ps1).FirstOrDefault();
            var psPositionIds = (from p in db.PositionSourcePositions where p.PositionSourceId == positionSource.Id select p.PositionId);

            var playerPositions = (from p in db.PositionSourcePlayers.AsNoTracking()
                                   join ps in db.PositionSources on p.PositionSourceId equals ps.Id
                                   where p.SeasonId == season.Id && ps.ProviderId == provider.Id
                                   orderby p.Position.DisplayOrder ascending
                                   select p)
                                   .Include(p1 => p1.Position)
                                   .ToList();

            var defaultPositions = GetPlayerDefaultPositions();

            foreach (var sp in GetSeasonPlayers(season, null))
            {
                if ((from p in playerPositions where p.PlayerId == sp.PlayerId select p).FirstOrDefault() == null)
                {
                    foreach (var dp in from p2 in defaultPositions where p2.PlayerId == sp.PlayerId select p2)
                    {
                        var pos = (from p1 in psPositionIds where p1 == dp.PositionId select p1).FirstOrDefault();
                        if (pos != 0)
                        {
                            var psp = new PositionSourcePlayer();
                            psp.PlayerId = sp.PlayerId;
                            psp.PositionId = dp.PositionId;
                            psp.Position = dp.Position;
                            psp.SeasonId = season.Id;
                            psp.PositionSourceId = positionSource.Id;
                            playerPositions.Add(psp);
                            break;
                        }
                    }
                }
            }

            AddCacheItem(cacheId, playerPositions);

            return playerPositions;
        }

        public Draft AddDraft(Draft draft)
        {
            if (draft == null || draft.DraftPlayers.Count == 0)
                return draft;

            var match = (from d in db.Drafts.AsNoTracking() where d.FantasyProviderId == draft.FantasyProviderId && d.ProviderLeagueId == draft.ProviderLeagueId select d).FirstOrDefault();
            if (match == null)
            {
                db.Drafts.Add(draft);
                db.SaveChanges();
                foreach (var pt in draft.DraftPlayerTypes)
                {
                    pt.DraftId = draft.Id;
                    db.DraftPlayerTypes.Add(pt);
                }
                db.SaveChanges();
            }

            return draft;
        }

        public void UpdateUserLeagueTeams(int userLeagueId,
            List<UserLeagueTeam> userLeagueTeams,
            List<UserLeagueMissingPlayer> userLeagueMissingPlayers,
            List<UserLeagueWaiverPlayer> userLeagueWaiverPlayers)
        {
            List<UserLeagueTeam> oldTeams = (from t in db.UserLeagueTeams where t.UserLeagueId == userLeagueId select t)
                .Include(a => a.UserLeagueTeamPlayers)
                .ToList();

            Dictionary<int, bool> oldPlayerHash = new Dictionary<int, bool>();
            foreach (var oldTeam in oldTeams)
            {
                foreach (var p in oldTeam.UserLeagueTeamPlayers)
                {
                    oldPlayerHash[p.PlayerId] = true;
                    db.UserLeagueTeamPlayers.Remove(p);
                }
            }

            bool rostersChanged = false;

            foreach (var newTeam in userLeagueTeams)
            {
                if (!rostersChanged)
                {
                    foreach (var p in newTeam.UserLeagueTeamPlayers)
                    {
                        if (!oldPlayerHash.ContainsKey(p.PlayerId))
                            rostersChanged = true;
                    }
                }

                var match = (from t in oldTeams where t.ProviderId == newTeam.ProviderId select t).FirstOrDefault();
                if (match == null)
                {
                    db.UserLeagueTeams.Add(newTeam);
                }
                else
                {
                    match.UserLeagueTeamPlayers = newTeam.UserLeagueTeamPlayers;
                }
            }

            foreach (var oldTeam in oldTeams)
            {
                var match = (from t in userLeagueTeams where t.ProviderId == oldTeam.ProviderId select t).FirstOrDefault();
                if (match == null)
                {
                    db.Remove(oldTeam);
                }
            }

            db.SaveChanges();

            try
            {
                if (userLeagueMissingPlayers != null)
                {
                    foreach (var mp in (from mp1 in db.UserLeagueMissingPlayers where mp1.UserLeagueId == userLeagueId select mp1))
                        db.Remove(mp);
                    db.SaveChanges();
                    foreach (var mp in userLeagueMissingPlayers)
                    {
                        mp.UserLeagueId = userLeagueId;
                        db.Add(mp);
                    }
                    db.SaveChanges();
                }
            }
            catch
            {
            }

            //var userLeague = (db.UserLeagues.Find(userLeagueId));
            //if (userLeagueTeams.Count > 0 && userLeague.NumberOfTeams != userLeagueTeams.Count)
            //    userLeague.NumberOfTeams = userLeagueTeams.Count;
            //userLeague.UpdatedDate = DateTime.UtcNow;
            //db.SaveChanges();

            try
            {
                if (userLeagueWaiverPlayers != null)
                {
                    foreach (var ww in (from ww1 in db.UserLeagueWaiverPlayers where ww1.UserLeagueId == userLeagueId select ww1))
                        db.Remove(ww);
                    db.SaveChanges();
                    foreach (var ww in userLeagueWaiverPlayers)
                    {
                        ww.UserLeagueId = userLeagueId;
                        db.Add(ww);
                    }
                    db.SaveChanges();
                }
            }
            catch
            {

            }

            UpdateUserLeagueUpdatedDate(userLeagueId, DateTime.UtcNow, rostersChanged);
        }

        public List<OwnershipPlayer> GetOwnershipPlayers(string categoriesCode, DateTime gameDate, string lineupFrequency = "")
        {
            string cacheId = "GetOwnershipPlayers"
                + "C:" + categoriesCode
                + "L:" + lineupFrequency
                + "D:" + gameDate.ToShortDateString() + gameDate.ToShortTimeString();
            if (CacheItemExists(cacheId))
                return (List<OwnershipPlayer>)GetCacheItem(cacheId);

            CategoriesString categoriesString = GetCategoriesString(categoriesCode);

            DateTime? maxDate = (from op in db.OwnershipPlayers
                                 where op.GameDate <= gameDate && op.CategoriesStringId == categoriesString.Id
                                 select (DateTime?)op.GameDate).Max();
            if (maxDate == null)
                return new List<OwnershipPlayer>();

            List<OwnershipPlayer> ownershipPlayers = (from op in db.OwnershipPlayers.AsNoTracking()
                                                      .Include(p => p.Player).AsNoTracking()
                                                      where op.GameDate == maxDate && op.CategoriesStringId == categoriesString.Id
                                                      select op).ToList();

            List<OwnershipPlayer> outOP = new List<OwnershipPlayer>();
            foreach (var ownershipPlayer in ownershipPlayers)
            {
                var currentOP = (from op in outOP where op.PlayerId == ownershipPlayer.PlayerId select op).FirstOrDefault();
                if (currentOP != null)
                {
                    currentOP.OwnCount += ownershipPlayer.OwnCount;
                    currentOP.ActiveCount += ownershipPlayer.ActiveCount;
                    currentOP.IRCount += ownershipPlayer.IRCount;
                    currentOP.LeagueCount += ownershipPlayer.LeagueCount;
                }
                else
                {
                    outOP.Add(ownershipPlayer);
                }
            }

            if (ownershipPlayers.Count > 0)
            {
                int maxLeagueCount = ownershipPlayers.Max(op => op.LeagueCount);
                foreach (var ownershipPlayer in ownershipPlayers)
                    ownershipPlayer.LeagueCount = maxLeagueCount;
            }

            AddCacheItem(cacheId, outOP);

            return outOP;
        }

        public List<OwnershipPlayer> GetAllDefaultOwnershipPlayers(DateTime gameDate)
        {
            string cacheId = "GetAllDefaultOwnershipPlayers"
                + ":D" + gameDate.ToShortDateString() + gameDate.ToShortTimeString();
            if (CacheItemExists(cacheId))
                return (List<OwnershipPlayer>)GetCacheItem(cacheId);

            var ownershipPlayers = new List<OwnershipPlayer>();
            foreach (var pt in GetPlayerTypes())
            {
                foreach (var op in GetOwnershipPlayers(GetDefaultCategoriesString(pt).Code, gameDate))
                    ownershipPlayers.Add(op);
            }

            AddCacheItem(cacheId, ownershipPlayers);

            return ownershipPlayers;
        }

        public List<OwnershipPlayer> GetAllOwnershipPlayers(UserLeague userLeague, DateTime gameDate)
        {
            string cacheId = "GetAllOwnershipPlayers"
                + "UL" + userLeague.Id.ToString()
                + ":D" + gameDate.ToShortDateString() + gameDate.ToShortTimeString();
            if (CacheItemExists(cacheId))
                return (List<OwnershipPlayer>)GetCacheItem(cacheId);

            var ownershipPlayers = new List<OwnershipPlayer>();
            foreach (var pt in GetPlayerTypes())
            {
                foreach (var op in GetOwnershipPlayers(GetUserLeagueCategoryCode(userLeague, pt), gameDate))
                    ownershipPlayers.Add(op);
            }

            AddCacheItem(cacheId, ownershipPlayers);

            return ownershipPlayers;
        }

        public List<OwnershipPlayer> GetOwnershipPlayersWithChange(string categoriesCode, DateTime gameDate, int hoursBack)
        {
            string cacheId = "GetOwnershipPlayersWithChange"
                + "C:" + categoriesCode
                + "D:" + gameDate.ToShortDateString() + gameDate.ToShortTimeString()
                + "H:" + hoursBack.ToString();
            if (CacheItemExists(cacheId))
                return (List<OwnershipPlayer>)GetCacheItem(cacheId);

            DateTime prevDate = gameDate.AddHours(-1 * hoursBack);
            var ownPlayers = GetOwnershipPlayers(categoriesCode, gameDate);
            var prevOwnPlayers = GetOwnershipPlayers(categoriesCode, prevDate);
            var activePlayers = GetOwnershipPlayers(categoriesCode, gameDate);
            var prevActivePlayers = GetOwnershipPlayers(categoriesCode, prevDate);
            if (prevOwnPlayers.Count > 0)
            {
                foreach (var ownPlayer in ownPlayers)
                {
                    var activePlayer = activePlayers.Find(p => p.PlayerId == ownPlayer.PlayerId);
                    if (activePlayer == null)
                        continue;
                    var prevOwnPlayer = prevOwnPlayers.Find(p => p.PlayerId == ownPlayer.PlayerId);
                    if (prevOwnPlayer != null)
                    {
                        var prevActivePlayer = prevActivePlayers.Find(p => p.PlayerId == ownPlayer.PlayerId);
                        if (prevActivePlayer == null)
                            continue;
                        ownPlayer.PercentOwnershipChange = ownPlayer.OwnershipPercent - prevOwnPlayer.OwnershipPercent;
                        ownPlayer.PercentActiveChange = activePlayer.ActivePercent - prevActivePlayer.ActivePercent;
                    }
                    else
                    {
                        ownPlayer.PercentOwnershipChange = ownPlayer.OwnershipPercent;
                        ownPlayer.PercentActiveChange = activePlayer.ActivePercent;
                    }
                }
            }

            AddCacheItem(cacheId, ownPlayers);

            return ownPlayers;
        }

        public void FillOwnershipPlayers(string categoriesCode, List<UserLeague> sourceUserLeagues)
        {
            DateTime gameDate = GetCurrentOwnershipGameDate(categoriesCode, false);
            CategoriesString categoriesString = GetCategoriesString(categoriesCode);

            var processUserLeagues = (from ul in sourceUserLeagues select ul).ToList();

            if (processUserLeagues.Count == 0)
                return;

            List<OwnershipPlayer> ownershipPlayers = new List<OwnershipPlayer>();
            Dictionary<string, bool> processed = new Dictionary<string, bool>();

            int leagueSize = 0;
            int validLeagueCount = 0;

            foreach (var ul in processUserLeagues)
            {
                if (!processed.ContainsKey(ul.ProviderLeagueId))
                {
                    Dictionary<int, bool> playerUsed = new Dictionary<int, bool>();

                    if (leagueSize == 0)
                        leagueSize = ul.NumberOfTeams * ul.PlayersPerTeam;

                    var teamPlayers = GetUserLeagueTeamPlayers(ul);
                    if (teamPlayers.Count > 0)
                    {
                        validLeagueCount++;
                        foreach (var p in teamPlayers)
                        {
                            if (!playerUsed.ContainsKey(p.PlayerId))
                            {
                                var ownershipPlayer = (from op in ownershipPlayers where op.PlayerId == p.PlayerId select op).FirstOrDefault();
                                if (ownershipPlayer == null)
                                {
                                    ownershipPlayer = new OwnershipPlayer();
                                    ownershipPlayer.CategoriesStringId = categoriesString.Id;
                                    ownershipPlayer.PlayerId = p.PlayerId;
                                    ownershipPlayer.LeagueSize = leagueSize;
                                    ownershipPlayer.GameDate = gameDate;
                                    ownershipPlayers.Add(ownershipPlayer);
                                }
                                ownershipPlayer.OwnCount++;
                                if (p.IsActive)
                                    ownershipPlayer.ActiveCount++;
                                if (p.IsIR)
                                    ownershipPlayer.IRCount++;
                                playerUsed[p.PlayerId] = true;
                            }
                        }
                    }

                    processed[ul.ProviderLeagueId] = true;
                }
            }

            foreach (var op in ownershipPlayers)
                op.LeagueCount = validLeagueCount;

            // delete old matches
            foreach (var op in (from op1 in db.OwnershipPlayers where op1.GameDate == gameDate && op1.CategoriesStringId == categoriesString.Id select op1))
                db.OwnershipPlayers.Remove(op);
            db.SaveChanges();

            foreach (var op in ownershipPlayers)
                db.OwnershipPlayers.Add(op);

            db.SaveChanges();
        }

        public DateTime GetCurrentOwnershipGameDate(string categoriesCode, bool existingOnly)
        {
            string cacheId = "GetCurrentOwnershipGameDate"
                + "C:" + categoriesCode
                + "E:" + existingOnly.ToString();
            if (CacheItemExists(cacheId))
                return (DateTime)GetCacheItem(cacheId);

            CategoriesString categoriesString = GetCategoriesString(categoriesCode);

            DateTime outDate;
            if (existingOnly && db.OwnershipPlayers.Count() > 0)
            {
                outDate = db.OwnershipPlayers.Where(i => i.CategoriesStringId == categoriesString.Id).Max(i => i.GameDate);
                DateTime utcNow = DateTime.UtcNow;
                outDate = new DateTime(utcNow.Year, utcNow.Month, utcNow.Day).AddHours(utcNow.Hour);
            }
            else
            {
                DateTime utcNow = DateTime.UtcNow;

                outDate = new DateTime(utcNow.Year, utcNow.Month, utcNow.Day).AddHours(utcNow.Hour);
            }

            AddCacheItem(cacheId, outDate);

            return outDate;
        }


        public FantasyProvider GetDefaultFantasyProvider()
        {
            string cacheId = "GetDefaultFantasyProvider";
            if (CacheItemExists(cacheId))
                return (FantasyProvider)GetCacheItem(cacheId);

            var provider = (from fp in db.FantasyProviders.AsNoTracking() where fp.IsDefault select fp).FirstOrDefault();

            AddCacheItem(cacheId, provider);

            return provider;
        }

        public List<PositionSourcePlayer> GetPlayerPositionSourcePlayers(FantasyProvider provider, Player player, Season season)
        {
            string cacheId = "GetPlayerPositionSourcePlayers"
                + "P:" + provider.Id.ToString()
                + "P:" + player.Id.ToString()
                + "S:" + season.Id.ToString();
            if (CacheItemExists(cacheId))
                return (List<PositionSourcePlayer>)GetCacheItem(cacheId);

            var psp = GetPlayerSeasonPositions(provider, season);

            List<PositionSourcePlayer> outPsp = null;
            if (psp != null)
            {
                outPsp = (from p in psp where p.PlayerId == player.Id select p).ToList();
            }

            AddCacheItem(cacheId, outPsp);

            return outPsp;
        }

        public GetPositionValuePlayersResult GetPositionValuePlayers2(
            PlayerType playerType,
            List<ValuePlayer> valuePlayers,
            UserLeague userLeague,
            List<PositionSourcePlayer> positionSourcePlayers,
            List<OwnershipPlayer> ownershipPlayers)
        {
            var getPositionValuePlayersResult = new GetPositionValuePlayersResult();
            var sortedValuePlayers = (from vp in valuePlayers
                                      join op in ownershipPlayers on vp.Player.Id equals op.PlayerId
                                      where op.OwnershipPercent >= 40
                                      orderby vp.LeagueValue descending
                                      select vp).ToList();

            var allMatchValuePlayers = new List<ValuePlayer>();
            foreach (var userLeagueActiveRosterSpot in userLeague.UserLeagueActiveRosterSpots)
            {
                if (userLeagueActiveRosterSpot.ActiveRosterSpot.PlayerType.Id != playerType.Id)
                    continue;

                int benchSize = (int)Math.Round((double)userLeague.BenchPlayersPerTeam / (double)GetPlayerTypes().Count, 0);
                int totalPlayers = (userLeagueActiveRosterSpot.NumberOfPlayers + 1) * userLeague.NumberOfTeams;
                var matchValuePlayers = new List<ValuePlayer>();
                foreach (var valuePlayer in sortedValuePlayers)
                {
                    bool isMatch = false;
                    var playerPsps = (from psp in positionSourcePlayers where psp.PlayerId == valuePlayer.Player.Id select psp).ToList();
                    foreach (var playerPsp in playerPsps)
                    {
                        if (userLeagueActiveRosterSpot.ActiveRosterSpot.PositionQualifies(playerPsp.Position))
                            isMatch = true;
                    }
                    if (isMatch)
                    {
                        matchValuePlayers.Add(valuePlayer);
                        allMatchValuePlayers.Add(valuePlayer);
                        if (matchValuePlayers.Count == totalPlayers)
                            break;
                    }
                }
            }

            double allAvg = (from vp in allMatchValuePlayers select vp.LeagueValue).Average();

            foreach (var userLeagueActiveRosterSpot in userLeague.UserLeagueActiveRosterSpots)
            {
                if (userLeagueActiveRosterSpot.ActiveRosterSpot.PlayerType.Id != playerType.Id)
                    continue;

                var matchValuePlayers = new List<ValuePlayer>();
                foreach (var valuePlayer in allMatchValuePlayers)
                {
                    bool isMatch = false;
                    var playerPsps = (from psp in positionSourcePlayers where psp.PlayerId == valuePlayer.Player.Id select psp).ToList();
                    foreach (var playerPsp in playerPsps)
                    {
                        if (userLeagueActiveRosterSpot.ActiveRosterSpot.PositionQualifies(playerPsp.Position))
                            isMatch = true;
                    }
                    if (isMatch)
                        matchValuePlayers.Add(valuePlayer);
                }

                if (matchValuePlayers.Count == 0)
                    continue;

                var topValuePlayers = (from vp in matchValuePlayers orderby vp.LeagueValue descending select vp).ToList();
                double avg = (from vp in topValuePlayers select vp.LeagueValue).Average();
                double boost = (allAvg - avg);

                getPositionValuePlayersResult.ActiveRosterSpotBoostHash[userLeagueActiveRosterSpot.ActiveRosterSpotId] = boost;

                foreach (var valuePlayer in matchValuePlayers)
                {
                    var positionValuePlayer = (from pvp in getPositionValuePlayersResult.PositionValuePlayers where pvp.DefaultValuePlayer.Player.Id == valuePlayer.Player.Id select pvp).FirstOrDefault();
                    if (positionValuePlayer == null)
                    {
                        positionValuePlayer = new PositionValuePlayer();
                        positionValuePlayer.DefaultValuePlayer = valuePlayer;
                        positionValuePlayer.MostValuableActiveRosterSpot = userLeagueActiveRosterSpot.ActiveRosterSpot;
                        positionValuePlayer.IsStartable = (from tvp in topValuePlayers where tvp.Player.Id == valuePlayer.Player.Id select tvp).FirstOrDefault() != null;
                        positionValuePlayer.IsOwnable = positionValuePlayer.IsStartable;
                        positionValuePlayer.PositionValue = positionValuePlayer.DefaultValuePlayer.LeagueValue + boost;
                        getPositionValuePlayersResult.PositionValuePlayers.Add(positionValuePlayer);
                    }
                    else
                    {
                        double positionValue = positionValuePlayer.DefaultValuePlayer.LeagueValue + boost;
                        if (positionValue > positionValuePlayer.PositionValue)
                        {
                            positionValuePlayer.MostValuableActiveRosterSpot = userLeagueActiveRosterSpot.ActiveRosterSpot;
                            positionValuePlayer.IsStartable = (from tvp in topValuePlayers where tvp.Player.Id == valuePlayer.Player.Id select tvp).FirstOrDefault() != null;
                            positionValuePlayer.IsOwnable = positionValuePlayer.IsStartable;
                            positionValuePlayer.PositionValue = positionValue;
                        }
                    }
                }
            }

            return getPositionValuePlayersResult;
        }

        public GetPositionValuePlayersResult GetPositionValuePlayers(
            PlayerType playerType,
            List<ValuePlayer> valuePlayers,
            UserLeague userLeague,
            List<Position> positionSourcePositions,
            List<PositionSourcePlayer> positionSourcePlayers,
            List<OwnershipPlayer> ownershipPlayers)
        {
            var getPositionValuePlayersResult = new GetPositionValuePlayersResult();

            try
            {
                // determine # of starters at each position
                var positionTotalRostersHash = new Dictionary<int, double>();
                foreach (var position in positionSourcePositions)
                {
                    double total = 0;
                    foreach (var userLeagueActiveRosterSpot in userLeague.UserLeagueActiveRosterSpots)
                    {
                        if (userLeagueActiveRosterSpot.ActiveRosterSpot.PositionQualifies(position))
                        {
                            double qualifyCount = 0;
                            foreach (var position2 in positionSourcePositions)
                                if (userLeagueActiveRosterSpot.ActiveRosterSpot.PositionQualifies(position2))
                                    qualifyCount += 1d;
                            total += (double)userLeagueActiveRosterSpot.NumberOfPlayers / qualifyCount;
                        }
                    }
                    positionTotalRostersHash[position.Id] = total * (double)userLeague.NumberOfTeams;
                }

                // find starting players for each position
                var season = GetDefaultSeason();
                var sortedValuePlayers = (from vp in valuePlayers
                                          join op in ownershipPlayers on vp.Player.Id equals op.PlayerId
                                          where op.OwnershipPercent >= 35
                                          orderby vp.LeagueValue descending
                                          select vp).ToList();

                if (sortedValuePlayers.Count() == 0)
                    return new GetPositionValuePlayersResult();

                var positionValuePlayersHash = new Dictionary<int, List<PositionValuePlayer>>();
                var positionTotalValuesHash = new Dictionary<int, double>();
                var positionTotalWeightsHash = new Dictionary<int, double>();
                double allTotalValue = 0;
                double allTotalWeight = 0;
                foreach (var position in positionSourcePositions)
                {
                    List<PositionValuePlayer> positionValuePlayers = new List<PositionValuePlayer>();
                    positionValuePlayersHash[position.Id] = positionValuePlayers;
                    double positionRosterTotal = positionTotalRostersHash[position.Id];
                    double rosterTotal = 0;
                    positionTotalValuesHash[position.Id] = 0;
                    positionTotalWeightsHash[position.Id] = 0;
                    foreach (var valuePlayer in sortedValuePlayers)
                    {
                        var playerPositions = GetPlayerPositionSourcePlayers(userLeague.FantasyProvider, valuePlayer.Player, season);
                        if ((from pp in playerPositions where pp.PositionId == position.Id select pp).FirstOrDefault() != null)
                        {
                            var positionValuePlayer = new PositionValuePlayer();
                            positionValuePlayer.DefaultValuePlayer = valuePlayer;
                            positionValuePlayer.MostValuablePosition = position;
                            positionValuePlayer.IsStartable = (rosterTotal < positionRosterTotal);
                            positionValuePlayer.IsOwnable = positionValuePlayer.IsStartable || (positionValuePlayer.DefaultValuePlayer.Rank <= userLeague.Size);
                            positionValuePlayer.Weight = (1 / (double)playerPositions.Count());
                            positionValuePlayers.Add(positionValuePlayer);
                            rosterTotal += positionValuePlayer.Weight;
                            if (positionValuePlayer.IsStartable)
                            {
                                double weightedValue = valuePlayer.LeagueValue * positionValuePlayer.Weight;
                                positionTotalValuesHash[position.Id] = positionTotalValuesHash[position.Id] + weightedValue;
                                positionTotalWeightsHash[position.Id] = positionTotalWeightsHash[position.Id] + positionValuePlayer.Weight;
                                allTotalValue += weightedValue;
                                allTotalWeight += positionValuePlayer.Weight;
                            }
                        }
                    }
                }

                double allAvgValue = (allTotalWeight == 0) ? 0 : allTotalValue / allTotalWeight;
                var positionAvgValuesHash = new Dictionary<int, double>();
                foreach (var position in positionSourcePositions)
                {
                    if (positionTotalWeightsHash[position.Id] > 0)
                        positionAvgValuesHash[position.Id] = positionTotalValuesHash[position.Id] / positionTotalWeightsHash[position.Id];
                    var allPositionPlayers = positionValuePlayersHash[position.Id];
                    double positionBoost = -1 * (positionAvgValuesHash[position.Id] - allAvgValue);
                    getPositionValuePlayersResult.PositionBoostHash[position.Id] = positionBoost;
                    foreach (var positionValuePlayer in allPositionPlayers)
                        positionValuePlayer.PositionValue = positionValuePlayer.DefaultValuePlayer.LeagueValue + positionBoost;
                }

                foreach (var valuePlayer in valuePlayers)
                {
                    PositionValuePlayer bestPositionValuePlayer = null;
                    foreach (var position in positionSourcePositions)
                    {
                        var allPositionPlayers = positionValuePlayersHash[position.Id];
                        var matchPlayer = (from p in allPositionPlayers where p.DefaultValuePlayer.Player.Id == valuePlayer.Player.Id select p).FirstOrDefault();
                        if (matchPlayer != null)
                        {
                            if (bestPositionValuePlayer == null)
                                bestPositionValuePlayer = matchPlayer;
                            else if (matchPlayer.PositionValue > bestPositionValuePlayer.PositionValue)
                                bestPositionValuePlayer = matchPlayer;
                        }
                    }
                    if (bestPositionValuePlayer != null)
                        getPositionValuePlayersResult.PositionValuePlayers.Add(bestPositionValuePlayer);
                }
            }
            catch
            {

            }

            return getPositionValuePlayersResult;
        }

        public List<ValuePlayer> GetValuePlayers(
            PlayerType playerType,
            Season season,
            DateTime startDate,
            DateTime endDate,
            int pastGames,
            List<CategorySetting> categorySettings,
            string scoringSystem,
            PerValue perValue,
            int leagueSize,
            bool finishedOnly,
            out ValueAverages outValueAverages)
        {
            string cacheId = "GetValuesPlayers"
                + ":PT" + playerType.Id.ToString()
                + ":S" + season.Id.ToString()
                + ":SD" + startDate.ToShortDateString()
                + ":ED" + endDate.ToShortDateString()
                + ":PG" + pastGames.ToString()
                + ":SC" + scoringSystem
                + ":PV" + perValue.Id.ToString()
                + ":F" + finishedOnly.ToString()
                + ":LS" + leagueSize.ToString();
            foreach (var cs in categorySettings)
                cacheId += "CS" + cs.Category.Id.ToString() + "|" + cs.PointsPerStat.ToString() + "|" + cs.IsActive.ToString();
            string valueAveragesCacheId = "ValueAverages" + cacheId;

            if (CacheItemExists(cacheId) && CacheItemExists(valueAveragesCacheId))
            {
                outValueAverages = (ValueAverages)GetCacheItem(valueAveragesCacheId);
                return (List<ValuePlayer>)GetCacheItem(cacheId);
            }

            var statPlayers = GetStatPlayers(playerType, season.Id, startDate, endDate, finishedOnly);
            ValuePlayerLib lib = new ValuePlayerLib();
            var valuePlayers = lib.GetValuePlayers(statPlayers, categorySettings, GetGamesCategory(playerType.Id), scoringSystem, perValue, playerType, GetDisplayCategories(), leagueSize, out outValueAverages);

            AddCacheItem(valueAveragesCacheId, outValueAverages);
            AddCacheItem(cacheId, valuePlayers);

            return valuePlayers;
        }

        public PerValue GetDefaultPerValue(int playerTypeId)
        {
            string cacheId = "GetDefaultPerValue" + playerTypeId.ToString();
            if (CacheItemExists(cacheId))
                return (PerValue)GetCacheItem(cacheId);

            var pts = GetPerValues(playerTypeId);
            var pt = (from p in pts where p.IsDefault == true select p).FirstOrDefault();

            AddCacheItem(cacheId, pt);

            return pt;
        }

        public PerValue GetDefaultDisplayPerValue(int playerTypeId)
        {
            string cacheId = "GetDefaultDisplayPerValue" + playerTypeId.ToString();
            if (CacheItemExists(cacheId))
                return (PerValue)GetCacheItem(cacheId);

            var pts = GetPerValues(playerTypeId);
            var pt = (from p in pts where p.IsDefaultDisplay == true select p).FirstOrDefault();

            AddCacheItem(cacheId, pt);

            return pt;
        }

        public PerValue GetSkillPerValue(int playerTypeId)
        {
            string cacheId = "GetSkillPerValue" + playerTypeId.ToString();
            if (CacheItemExists(cacheId))
                return (PerValue)GetCacheItem(cacheId);

            var pts = GetPerValues(playerTypeId);
            var pt = (from p in pts where p.SkillCategoryValue != null select p).FirstOrDefault();

            if (pt == null)
                pt = GetDefaultPerValue(playerTypeId);

            AddCacheItem(cacheId, pt);

            return pt;
        }

        public List<CategorySetting> GetUserLeagueCategorySettings(UserLeague userLeague, PlayerType playerType)
        {
            if (userLeague == null)
                return GetDefaultCategorySettings(playerType);

            return userLeague.GetCategorySettings(playerType);
        }

        public int GetUserLeagueLeagueSize(UserLeague userLeague, PlayerType playerType)
        {
            if (userLeague != null)
            {
                int playerTypeCount = GetPlayerTypes().Count;
                double percentBench = 1 / (double)playerTypeCount;
                int playerTypeActive = 0;
                foreach (var uars in userLeague.UserLeagueActiveRosterSpots)
                {
                    bool isMatch = false;
                    foreach (var arsp in uars.ActiveRosterSpot.ActiveRosterSpotPositions)
                    {
                        if (arsp.Position.PlayerType.Id == playerType.Id)
                            isMatch = true;
                    }
                    if (isMatch)
                        playerTypeActive += uars.NumberOfPlayers;
                }
                int leagueBenchPlayers = (int)Math.Round((double)(userLeague.BenchPlayersPerTeam * userLeague.NumberOfTeams) * percentBench, 0);
                return playerTypeActive * userLeague.NumberOfTeams + leagueBenchPlayers;
            }
            else
            {
                return 12 * playerType.DefaultPerTeam;
            }
        }

        public List<PositionSourcePlayer> GetUserLeagueSeasonPlayerPositions(UserLeague userLeague, Season season)
        {
            return GetPlayerSeasonPositions(userLeague != null ? userLeague.FantasyProvider : GetDefaultFantasyProvider(), season);
        }

        public string GetUserLeagueScoringSystem(UserLeague userLeague)
        {
            return (userLeague != null ? userLeague.ScoringSystem : Sport.DefaultScoringSystem);
        }

        public Season GetPreviousSeason(int maxYear)
        {
            string cacheId = "GetPreviousSeason" + maxYear.ToString();
            if (CacheItemExists(cacheId))
                return (Season)GetCacheItem(cacheId);

            var seasonId = (from s in db.Seasons.AsNoTracking() where s.Year <= maxYear && s.IsRegularSeason.HasValue orderby s.Year descending select s.Id)
                .FirstOrDefault();

            Season season = null;
            if (seasonId > 0)
                season = GetSeason(seasonId);

            AddCacheItem(cacheId, season);

            return season;
        }

        public DateTime GetOwnershipPlayersDate(string categoriesCode, DateTime maxDate)
        {
            string cacheId = "GetOwnershipPlayersDate"
                + "C:" + categoriesCode
                + "D:" + maxDate.ToShortDateString() + maxDate.ToShortTimeString();
            if (CacheItemExists(cacheId))
                return (DateTime)GetCacheItem(cacheId);

            CategoriesString categoriesString = GetCategoriesString(categoriesCode);

            var op = (from op1 in db.OwnershipPlayers.AsNoTracking() where op1.CategoriesStringId == categoriesString.Id select op1).ToList();

            if (op.Count() == 0)
                return maxDate;

            var ownDate = (from d in op where d.GameDate <= maxDate orderby d.GameDate descending select d.GameDate).FirstOrDefault();

            AddCacheItem(cacheId, ownDate);

            return ownDate;
        }

        public List<SeasonPlayer> GetSeasonPlayers(Season season, PlayerType playerType)
        {
            string cacheId = "GetSeasonPlayers" + season.Id.ToString();
            if (playerType != null)
                cacheId += ":PT" + playerType.Id.ToString();

            if (CacheItemExists(cacheId))
                return (List<SeasonPlayer>)GetCacheItem(cacheId);

            var seasonPlayers = (from p in db.SeasonPlayers
                                 .Include(i => i.Player).ThenInclude(i2 => i2.PlayerDefaultPositions)
                                 .Include(i => i.Player).ThenInclude(i2 => i2.PlayerDefaultPositions).ThenInclude(i3 => i3.Position)
                                 .Include(i => i.PlayerType)
                                 .Include(i => i.Team).Include(i => i.Season).Include(i => i.PlayerType)
                                 .AsNoTracking()
                                 where p.SeasonId == season.Id && (playerType != null ? p.PlayerTypeId == playerType.Id : true)
                                 orderby p.Season.EndDate descending
                                 select p).ToList();

            AddCacheItem(cacheId, seasonPlayers);

            return seasonPlayers;
        }

        public List<SeasonPlayer> GetAllSeasonPlayers(Season season)
        {
            string cacheId = "GetAllSeasonPlayers" + season.Id.ToString();
            if (CacheItemExists(cacheId))
                return (List<SeasonPlayer>)GetCacheItem(cacheId);

            var seasonPlayers = new List<SeasonPlayer>();
            foreach (var pt in GetPlayerTypes())
            {
                foreach (var sp in GetSeasonPlayers(season, pt))
                    seasonPlayers.Add(sp);
            }

            AddCacheItem(cacheId, seasonPlayers);

            return seasonPlayers;
        }

        public List<PlayerDefaultPosition> GetPlayerDefaultPositions()
        {
            string cacheId = "GetPlayerDefaultPositions";
            if (CacheItemExists(cacheId))
                return (List<PlayerDefaultPosition>)GetCacheItem(cacheId);

            var dp = db.PlayerDefaultPositions.AsNoTracking().Include(i => i.Position).ThenInclude(i2 => i2.PlayerType).ToList();

            AddCacheItem(cacheId, dp);

            return dp;
        }

        public PlayerDefaultPosition GetPlayerDefaultPosition(int playerId)
        {
            string cacheId = "GetPlayerDefaultPosition" + playerId.ToString();
            if (CacheItemExists(cacheId))
                return (PlayerDefaultPosition)GetCacheItem(cacheId);

            var playerDefaultPosition = (from dp in db.PlayerDefaultPositions.AsNoTracking()
                                         .Include(i => i.Position)
                                         where dp.PlayerId == playerId && dp.Position.IsActualPosition
                                         select dp).FirstOrDefault();

            AddCacheItem(cacheId, playerDefaultPosition);

            return playerDefaultPosition;
        }

        public List<Position> GetPositionSourcePositions(PositionSource positionSource)
        {
            string cacheId = "GetPositionSourcePositions" + positionSource.Id.ToString();
            if (CacheItemExists(cacheId))
                return (List<Position>)GetCacheItem(cacheId);

            var positions = (from p in db.PositionSourcePositions.AsNoTracking()
                             .Include(i => i.Position).ThenInclude(i2 => i2.PlayerType)
                             .Include(i => i.PositionSource)
                             where p.PositionSourceId == positionSource.Id
                             orderby p.Position.PlayerType.DisplayOrder, p.Position.DisplayOrder
                             select p.Position).ToList();

            AddCacheItem(cacheId, positions);

            return positions;
        }

        public PositionSource GetPositionSource(FantasyProvider fantasyProvider)
        {
            string cacheId = "GetPositionSource" + fantasyProvider.Id.ToString();
            if (CacheItemExists(cacheId))
                return (PositionSource)GetCacheItem(cacheId);

            var positionSource = (from ps in db.PositionSources.AsNoTracking() where ps.FantasyProviderId == fantasyProvider.Id select ps).FirstOrDefault();

            AddCacheItem(cacheId, positionSource);

            return positionSource;
        }

        public List<UserDisplayCategory> GetUserDisplayCategories(string userId, UserLeague userLeague)
        {
            if (userId == null)
                return GetDefaultDisplayCategories();

            var userDisplayCategories = (from udc in db.UserDisplayCategories.AsNoTracking()
                                         .Include(i => i.Category).ThenInclude(i => i.PlayerType)
                                         .Include(i => i.Category).ThenInclude(i => i.CategoryPerValues)
                                         where udc.UserId == userId
                                         orderby udc.DisplayOrder ascending
                                         select udc).ToList();

            if (userLeague == null)
                return userDisplayCategories;

            var filteredUserDisplayCategories = new List<UserDisplayCategory>();
            foreach (var userDisplayCategory in userDisplayCategories)
            {
                var match = (from c in userLeague.UserLeagueCategories where c.CategoryId == userDisplayCategory.CategoryId select c).FirstOrDefault();
                if (match == null)
                    filteredUserDisplayCategories.Add(userDisplayCategory);
            }

            return filteredUserDisplayCategories;
        }

        public List<UserDisplayCategory> GetDefaultDisplayCategories()
        {
            string cacheId = "GetDefaultDisplayCategories";
            if (CacheItemExists(cacheId))
                return (List<UserDisplayCategory>)GetCacheItem(cacheId);

            var displayCategories = new List<UserDisplayCategory>();
            foreach (var c in GetCategories())
            {
                if (c.IsDefaultDisplayCategory)
                {
                    var displayCategory = new UserDisplayCategory();
                    displayCategory.Category = c;
                    displayCategory.CategoryId = c.Id;
                    displayCategory.DisplayOrder = c.DisplayOrder;
                    displayCategories.Add(displayCategory);
                }
            }

            AddCacheItem(cacheId, displayCategories);

            return displayCategories;
        }

        public List<UserDisplayCategory> GetUserDisplayCategories(string userId, UserLeague userLeague, PlayerType playerType)
        {
            if (userId == null)
                return (from dc in GetDefaultDisplayCategories() where dc.Category.PlayerType.Id == playerType.Id orderby dc.Category.DisplayOrder select dc).ToList();

            var userDisplayCategories = (from udc in db.UserDisplayCategories.AsNoTracking()
                                         .Include(i => i.Category).ThenInclude(i => i.PlayerType)
                                         .Include(i => i.Category).ThenInclude(i => i.CategoryPerValues)
                                         where udc.UserId == userId && udc.Category.PlayerType.Id == playerType.Id
                                         orderby udc.DisplayOrder ascending
                                         select udc).ToList();

            foreach (var lcat in (from ulc in userLeague.UserLeagueCategories where ulc.Category.PlayerType.Id == playerType.Id select ulc))
            {
                if (userDisplayCategories.Find(dc => dc.CategoryId == lcat.CategoryId) == null)
                {
                    var userDisplayCategory = new UserDisplayCategory();
                    userDisplayCategory.Category = lcat.Category;
                    userDisplayCategory.CategoryId = lcat.CategoryId;
                    userDisplayCategory.DisplayOrder = lcat.Category.DisplayOrder;
                    userDisplayCategories.Add(userDisplayCategory);
                }
            }

            var sortedUserDisplayCategories = (from dc in userDisplayCategories orderby dc.Category.DisplayOrder select dc).ToList();

            //var filteredUserDisplayCategories = new List<UserDisplayCategory>();
            //foreach (var userDisplayCategory in userDisplayCategories)
            //{
            //    var match = (from c in userLeague.UserLeagueCategories where c.CategoryId == userDisplayCategory.CategoryId select c).FirstOrDefault();
            //    if (match == null)
            //        filteredUserDisplayCategories.Add(userDisplayCategory);
            //}

            return sortedUserDisplayCategories;
        }

        public List<Category> GetDisplayCategories()
        {
            string cacheId = "GetDisplayCategories";
            if (CacheItemExists(cacheId))
                return (List<Category>)GetCacheItem(cacheId);

            var displayCategories = (from c in db.Categories
                                     .Include(i => i.PlayerType)
                                     .AsNoTracking()
                                     where c.IsDisplayCategory
                                     orderby c.PlayerType.DisplayOrder, c.DisplayOrder
                                     select c).ToList();

            AddCacheItem(cacheId, displayCategories);

            return displayCategories;
        }

        public List<UserDisplayCategory> UpdateUserDisplayCategories(string userId, List<UserDisplayCategory> userDisplayCategories)
        {
            foreach (var u in (from udc in db.UserDisplayCategories where udc.UserId == userId select udc))
            {
                db.UserDisplayCategories.Remove(u);
            }
            db.SaveChanges();

            foreach (var udc in userDisplayCategories)
            {
                db.UserDisplayCategories.Add(udc);
            }
            db.SaveChanges();

            return userDisplayCategories;
        }

        public PlayerType GetPlayerType(int playerTypeId)
        {
            return (from pt in GetPlayerTypes() where pt.Id == playerTypeId select pt).FirstOrDefault();
        }

        public PlayerType GetDefaultPlayerType()
        {
            return (from pt in GetPlayerTypes() where pt.IsDefault select pt).FirstOrDefault();
        }

        public PlayerType GetPlayerType(string playerTypeTitle)
        {
            return (from pt in GetPlayerTypes() where pt.Title == playerTypeTitle select pt).FirstOrDefault();
        }

        public List<Category> GetCategories(PlayerType playerType)
        {
            var cats = (from c in GetCategories() where c.PlayerType.Id == playerType.Id select c).ToList();

            return cats;
        }

        public List<Position> GetPositions()
        {
            var positions = (from p in db.Positions.AsNoTracking().Include(i => i.PlayerType) select p).ToList();

            return positions;
        }

        public List<Position> GetActualPositions(PlayerType playerType)
        {
            return (from p in GetPositions() where p.IsActualPosition && p.PlayerType.Id == playerType.Id orderby p.DisplayOrder select p).ToList();
        }

        public List<Category> GetDisplayCategories(PlayerType playerType)
        {
            var dc = (from c in GetDisplayCategories() where c.PlayerType.Id == playerType.Id select c).ToList();

            return dc;
        }

        public async Task<List<UserLeagueActiveRosterSpot>> GetDefaultUserLeagueActiveRosterSpots()
        {
            var userLeagueActiveRosterSpots = new List<UserLeagueActiveRosterSpot>();

            foreach (var ars in await (from a in db.ActiveRosterSpots.AsNoTracking()
                                 .Include(i => i.ActiveRosterSpotPositions)
                                 .ThenInclude(t => t.Position)
                                 .ThenInclude(t => t.PlayerType)
                                 orderby a.DisplayOrder
                                 select a).ToListAsync())
            {
                var userLeagueActiveRosterSpot = new UserLeagueActiveRosterSpot();
                userLeagueActiveRosterSpot.ActiveRosterSpotId = ars.Id;
                userLeagueActiveRosterSpot.ActiveRosterSpot = ars;
                userLeagueActiveRosterSpot.NumberOfPlayers = ars.DefaultNumberOf;
                userLeagueActiveRosterSpots.Add(userLeagueActiveRosterSpot);
            }

            return userLeagueActiveRosterSpots;
        }

        public List<DisplayActiveRosterSpot> GetDisplayActiveRosterSpots(List<UserLeagueActiveRosterSpot> userLeagueActiveRosterSpots, List<Position> positionSourcePositions)
        {
            var displayActiveRosterSpots = new List<DisplayActiveRosterSpot>();

            var playerTypePositions = new Dictionary<int, List<Position>>();

            foreach (var p in positionSourcePositions)
            {
                var displayActiveRosterSpot = new DisplayActiveRosterSpot();
                displayActiveRosterSpot.Positions.Add(p);
                displayActiveRosterSpot.PlayerType = p.PlayerType;
                displayActiveRosterSpot.DisplayOrder = p.DisplayOrder;
                displayActiveRosterSpots.Add(displayActiveRosterSpot);
                if (!playerTypePositions.ContainsKey(p.PlayerType.Id))
                    playerTypePositions[p.PlayerType.Id] = new List<Position>();
                playerTypePositions[p.PlayerType.Id].Add(p);
            }

            foreach (int playerTypeId in playerTypePositions.Keys)
            {
                if (playerTypePositions[playerTypeId].Count() > 1)
                {
                    var displayActiveRosterSpot = new DisplayActiveRosterSpot();
                    foreach (var p in playerTypePositions[playerTypeId])
                    {
                        displayActiveRosterSpot.Positions.Add(p);
                    }
                    displayActiveRosterSpot.PlayerType = GetPlayerType(playerTypeId);
                    displayActiveRosterSpot.DisplayOrder = 0;
                    displayActiveRosterSpots.Add(displayActiveRosterSpot);
                }
            }

            displayActiveRosterSpots = (from rs in displayActiveRosterSpots orderby rs.PlayerType.DisplayOrder, rs.DisplayOrder select rs).ToList();

            return displayActiveRosterSpots;
        }

        public List<AdpPlayer> GetAdpPlayers(List<Draft> drafts)
        {
            string cacheId = "GetAdpPlayers";
            foreach (var draft in drafts)
                cacheId += ":D" + draft.Id.ToString();
            //if (CacheItemExists(cacheId))
            //    return (List<AdpPlayer>)GetCacheItem(cacheId);

            var adpPlayers = new List<AdpPlayer>();

            foreach (var draft in drafts)
            {
                foreach (var draftPlayer in draft.DraftPlayers)
                {
                    var adp = (from a in adpPlayers where a.PlayerId == draftPlayer.PlayerId select a).FirstOrDefault();
                    if (adp == null)
                    {
                        adp = new AdpPlayer();
                        adp.PlayerId = draftPlayer.PlayerId;
                        adpPlayers.Add(adp);
                    }
                    adp.Picks.Add((double)draftPlayer.DraftOrder);
                    if (draftPlayer.Price > 0)
                        adp.Prices.Add((double)draftPlayer.Price);
                }
            }

            int draftCount = 0;
            foreach (var adpPlayer in adpPlayers)
            {
                if (adpPlayer.Picks.Count > 0)
                {
                    draftCount = Math.Max(draftCount, adpPlayer.Picks.Count);
                    adpPlayer.MinPick = adpPlayer.Picks.Min();
                    adpPlayer.MaxPick = adpPlayer.Picks.Max();
                }
                if (adpPlayer.Prices.Count > 0)
                {
                    adpPlayer.MinPrice = adpPlayer.Prices.Min();
                    adpPlayer.MaxPrice = adpPlayer.Prices.Max();
                }
            }

            double maxDraftPick = double.MaxValue;
            if (adpPlayers.Count > 0)
            {
                maxDraftPick = adpPlayers.Max(m => m.MaxPick);
            }

            foreach (var adpPlayer in adpPlayers)
            {
                adpPlayer.DraftCount = draftCount;
                adpPlayer.Adp = adpPlayer.Picks.Count > 0 ? adpPlayer.Picks.Average() : 0;
                adpPlayer.AveragePrice = adpPlayer.Prices.Count > 0 ? adpPlayer.Prices.Average() : 0;

                // adjust adp if not drafted in all drafts
                if (adpPlayer.Picks.Count < draftCount)
                {
                    //double assumedDraftPick = maxDraftPick + 1; // if not drafted assume they would have been next
                    //double newAdp = adpPlayer.Adp * adpPlayer.Picks.Count;
                    //newAdp += (assumedDraftPick * (draftCount - adpPlayer.Picks.Count));
                    //newAdp /= draftCount;
                    //adpPlayer.Adp = newAdp;
                    //adpPlayer.MaxPick = maxDraftPick;
                }

                adpPlayer.DraftPercent = (draftCount > 0 ? (double)adpPlayer.Picks.Count / draftCount * 100 : 0);

                double sumOfSquaresOfDifferences = adpPlayer.Picks.Select(val => (val - adpPlayer.Adp) * (val - adpPlayer.Adp)).Sum();
                adpPlayer.StdevPick = Math.Sqrt(sumOfSquaresOfDifferences / adpPlayer.Picks.Count());
                adpPlayer.ProjectedLowPick = Math.Max(1, adpPlayer.Adp - adpPlayer.StdevPick * 2);
                adpPlayer.ProjectedHighPick = adpPlayer.Adp + adpPlayer.StdevPick * 2;

                sumOfSquaresOfDifferences = adpPlayer.Prices.Select(val => (val - adpPlayer.AveragePrice) * (val - adpPlayer.AveragePrice)).Sum();
                adpPlayer.StdevPrice = Math.Sqrt(sumOfSquaresOfDifferences / adpPlayer.Prices.Count());
                adpPlayer.ProjectedLowPrice = Math.Max(1, adpPlayer.AveragePrice - adpPlayer.StdevPrice * 2);
                adpPlayer.ProjectedHighPrice = adpPlayer.AveragePrice + adpPlayer.StdevPrice * 2;
            }

            AddCacheItem(cacheId, adpPlayers);

            return adpPlayers;
        }

        public List<AdpPlayer> GetAdpPlayers(Season season, string categoriesCode, DateTime startDate, DateTime endDate)
        {
            string cacheId = "GetPlayerAdps"
                + ":S" + season.Id.ToString()
                + ":C" + categoriesCode
                + ":SD" + startDate.ToShortDateString() + startDate.ToShortTimeString()
                + ":ED" + endDate.ToShortDateString() + endDate.ToShortTimeString();
            if (CacheItemExists(cacheId))
                return (List<AdpPlayer>)GetCacheItem(cacheId);

            CategoriesString categoriesString = GetCategoriesString(categoriesCode);

            var drafts = (from draft in db.Drafts
                          .Include(i => i.DraftPlayers).ThenInclude(i2 => i2.Player)
                          join pt in db.DraftPlayerTypes.AsNoTracking() on draft.Id equals pt.DraftId
                          where draft.SeasonId == season.Id && pt.CategoriesStringId == categoriesString.Id
                                && !draft.IsAuction && draft.DraftDate >= startDate && draft.DraftDate <= endDate
                          select draft
                        ).ToList();

            drafts = drafts.Where(d => d.IsAnalysis).ToList();

            var adpPlayers = GetAdpPlayers(drafts);

            AddCacheItem(cacheId, adpPlayers);

            return adpPlayers;
        }

        public List<AdpPlayer> GetAdpPlayers(Season season, string categoriesCode, int pastNumberOfDrafts, DateTime earliestDate)
        {
            string cacheId = "GetPlayerAdps"
                + "C:" + categoriesCode
                + "D:" + pastNumberOfDrafts.ToString()
                + "ED" + earliestDate.ToShortDateString();
            //if (CacheItemExists(cacheId))
            //    return (List<AdpPlayer>)GetCacheItem(cacheId);

            CategoriesString categoriesString = GetCategoriesString(categoriesCode);

            var drafts = (from draft in db.Drafts
                          .Include(i => i.DraftPlayers).ThenInclude(i2 => i2.Player)
                          join pt in db.DraftPlayerTypes.AsNoTracking() on draft.Id equals pt.DraftId
                          where draft.SeasonId == season.Id && !draft.IsAuction && pt.CategoriesStringId == categoriesString.Id && draft.DraftDate >= earliestDate
                                && !draft.IsAuction && draft.IsFinished && draft.IsProLeague
                          orderby draft.DraftDate descending
                          select draft
                        ).Take(pastNumberOfDrafts).ToList();

            // drafts = drafts.Where(d => d.IsAnalysis && d.DraftPlayers.Count > 0).ToList();
            drafts = drafts.Where(d => d.DraftPlayers.Count > 0).ToList();

            var adpPlayers = GetAdpPlayers(drafts);

            AddCacheItem(cacheId, adpPlayers);

            return adpPlayers;
        }

        public async Task<Draft> GetDraft(FantasyProvider fantasyProvider, string providerLeagueId)
        {
            var draft = await (from d in db.Drafts
                         .Include(i => i.FantasyProvider)
                         .Include(i => i.Season)
                         .Include(i => i.DraftPlayers).ThenInclude(i2 => i2.Player)
                         .AsNoTracking()
                         where d.FantasyProviderId == fantasyProvider.Id && d.ProviderLeagueId == providerLeagueId
                         select d).FirstOrDefaultAsync();

            if (draft != null)
            {
                draft.DraftPlayerTypes = await (from pt in db.DraftPlayerTypes.AsNoTracking()
                                          .Include(i => i.PlayerType)
                                          where pt.DraftId == draft.Id
                                          orderby pt.PlayerType.DisplayOrder
                                          select pt).ToListAsync();
            }

            return draft;
        }

        public void DeleteDraft(int draftId)
        {
            foreach (var draftPlayer in db.DraftPlayers.Where(d => d.DraftId == draftId))
                db.Remove(draftPlayer);
            foreach (var draftPlayerType in db.DraftPlayerTypes.Where(d => d.DraftId == draftId))
                db.Remove(draftPlayerType);
            db.SaveChanges();

            foreach (var draft in db.Drafts.Where(d => d.Id == draftId))
                db.Remove(draft);
            db.SaveChanges();
        }

        public void DeleteDraft(FantasyProvider fantasyProvider, string providerLeagueId)
        {
            var draft = (from d in db.Drafts where d.FantasyProviderId == fantasyProvider.Id && d.ProviderLeagueId == providerLeagueId select d).FirstOrDefault();
            if (draft != null)
                DeleteDraft(draft.Id);
        }

        public List<Draft> GetDrafts(FantasyProvider fantasyProvider)
        {
            var drafts = (from d in db.Drafts
                           .Include(i => i.FantasyProvider)
                           .Include(i => i.Season)
                           .AsNoTracking()
                          where d.FantasyProviderId == fantasyProvider.Id
                          select d
                        ).ToList();

            return drafts;
        }

        public List<Draft> GetDrafts(Season season)
        {
            var drafts = (from d in db.Drafts
               .Include(i => i.FantasyProvider)
               .Include(i => i.Season)
               .AsNoTracking()
                          where d.SeasonId == season.Id
                          select d
            ).ToList();

            return drafts;
        }

        public List<DraftPlayer> GetDraftPlayers(Draft draft)
        {
            if (draft == null)
                return new List<DraftPlayer>();

            var draftPlayers = (from dp in db.DraftPlayers
                                .Include(i => i.Player)
                                .AsNoTracking()
                                where dp.DraftId == draft.Id
                                select dp).ToList();

            return draftPlayers;
        }

        public async Task<bool> IsDraftFinished(FantasyProvider fantasy, string fantasyProviderId)
        {
            var isFinished = await (from d in db.Drafts.AsNoTracking() where d.ProviderLeagueId == fantasyProviderId select d.IsFinished).FirstOrDefaultAsync();

            return isFinished;
        }

        public List<PlayerInjury> GetPlayerInjuries()
        {
            if (db.PlayerInjuries.Count() == 0)
            {
                return new List<PlayerInjury>();
            }

            List<PlayerInjury> playerInjuries;
            DateTime? maxDate = db.PlayerInjuries.Max(p => p.DownloadDate);
            if (maxDate != null)
            {
                playerInjuries = (from p in db.PlayerInjuries select p).ToList();
            }
            else
            {
                return new List<PlayerInjury>();
            }

            return playerInjuries;
        }

        public void UpdateUserLeagueUpdatedDate(int userLeagueId, DateTime updatedDate, bool rostersUpdated)
        {
            var userLeague = (from ul in db.UserLeagues where ul.Id == userLeagueId select ul).FirstOrDefault();
            if (userLeague != null)
            {
                userLeague.UpdatedDate = updatedDate;
                if (rostersUpdated)
                    userLeague.RostersUpdatedDate = updatedDate;
                db.SaveChanges();
            }
        }

        public int DeletePlayerInjuries()
        {
            int deleted = 0;
            foreach (var i in db.PlayerInjuries)
            {
                db.PlayerInjuries.Remove(i);
                deleted++;
            }
            db.SaveChanges();

            return deleted;
        }

        public bool AddNBAPlayerGame(NBAPlayerGame playerGame)
        {
            db.NBAPlayerGames.Add(playerGame);
            db.SaveChanges();

            return true;
        }

        public bool AddHitterPlayerGame(MLBHitterGame playerGame)
        {
            db.MLBHitterGames.Add(playerGame);
            db.SaveChanges();

            return true;
        }

        public bool AddPitcherPlayerGame(MLBPitcherGame playerGame)
        {
            db.MLBPitcherGames.Add(playerGame);
            db.SaveChanges();

            return true;
        }

        public bool MarkGameFinished(int gameId)
        {
            var game = db.Games.Find(gameId);
            if (game != null && !game.IsFinished)
            {
                game.IsFinished = true;
                db.SaveChanges();

                return true;
            }

            return false;
        }

        public Player AddPlayer(Player player, bool generateId)
        {
            using (var transaction = db.Database.BeginTransaction())
            {
                db.Players.Add(player);
                if (!generateId)
                    db.Database.ExecuteSqlRaw("SET IDENTITY_INSERT Players ON");
                db.SaveChanges();
                if (!generateId)
                    db.Database.ExecuteSqlRaw("SET IDENTITY_INSERT Players OFF");
                transaction.Commit();
            }

            return player;
        }

        public bool AddSeasonPlayer(SeasonPlayer seasonPlayer)
        {
            db.SeasonPlayers.Add(seasonPlayer);
            return (db.SaveChanges() > 0);
        }

        public bool AddPlayerDefaultPosition(PlayerDefaultPosition playerDefaultPosition)
        {
            try
            {
                db.PlayerDefaultPositions.Add(playerDefaultPosition);
                return (db.SaveChanges() > 0);
            }
            catch
            {

            }

            return false;
        }

        public bool AddFantasyProviderPlayer(FantasyProviderPlayer fantasyProviderPlayer)
        {
            var find = (from p in db.FantasyProviderPlayers where p.PlayerId == fantasyProviderPlayer.PlayerId && p.FantasyProviderId == fantasyProviderPlayer.FantasyProviderId select p).FirstOrDefault();
            if (find != null)
                return false;

            db.FantasyProviderPlayers.Add(fantasyProviderPlayer);
            return (db.SaveChanges() > 0);
        }

        public bool AddPositionSourcePlayer(PositionSourcePlayer positionSourcePlayer)
        {
            db.PositionSourcePlayers.Add(positionSourcePlayer);
            return (db.SaveChanges() > 0);
        }

        public List<Season> GetSeasons()
        {
            return (from s in db.Seasons.AsNoTracking() orderby s.DisplayOrder select s).ToList();
        }

        public List<Game> GetGames(Season season)
        {
            string cacheId = "GetGames" + season.Id.ToString();
            if (CacheItemExists(cacheId))
                return (List<Game>)GetCacheItem(cacheId);

            var games = (from g in db.Games.AsNoTracking()
                         .Include(i => i.Season)
                         .Include(g => g.AwayTeam)
                         .Include(g => g.HomeTeam)
                         where g.Season.Id == season.Id
                         orderby g.GameDate, g.GameTime
                         select g).ToList();

            foreach (var game in games)
                game.FillProperties(Sport, colorLib);

            AddCacheItem(cacheId, games);

            return games;
        }

        public Game GetGame(int gameId)
        {
            string cacheId = "GetGame" + gameId.ToString();
            if (CacheItemExists(cacheId))
                return (Game)GetCacheItem(cacheId);

            Game game = null;

            Season season = (from g in db.Games.Include(s => s.Season) where g.Id == gameId select g.Season).FirstOrDefault();
            if (season != null)
            {
                game = (from g in GetGames(season) where g.Id == gameId select g).FirstOrDefault();
            }

            AddCacheItem(cacheId, game);

            return game;
        }

        public List<Game> GetGames(Season season, Team team)
        {
            string cacheId = "GetGames"
                + ":S" + season.Id.ToString()
                + ":T" + team.Id.ToString();
            if (CacheItemExists(cacheId))
                return (List<Game>)GetCacheItem(cacheId);

            var teamGames = (from g in GetGames(season) where g.IncludesTeam(team.Id) orderby g.GameTime ascending select g).ToList();

            AddCacheItem(cacheId, teamGames);

            return teamGames;
        }

        public SeasonPlayer GetSeasonPlayer(int playerId, PlayerType playerType, Season season)
        {
            var seasonPlayer = (from p in GetSeasonPlayers(season, playerType) where p.PlayerId == playerId select p).FirstOrDefault();

            return seasonPlayer;
        }

        public Game AddGame(Game game)
        {
            db.Games.Add(game);
            db.SaveChanges();

            return game;
        }

        public void UpdateGame(Game game)
        {
            var g = (from g1 in db.Games where g1.Id == game.Id select g1).FirstOrDefault();
            if (g != null)
            {
                g.GameDate = game.GameDate;
                g.GameTime = game.GameTime;
                g.SportRadarId = game.SportRadarId;
                g.IsFinished = game.IsFinished;
                g.IsPostponed = game.IsPostponed;
                g.Period = game.Period;
                g.GameClock = game.GameClock;
                g.PercentComplete = game.PercentComplete;
                g.HomeScore = game.HomeScore;
                g.AwayScore = game.AwayScore;
                g.AwayMoneyLine = game.AwayMoneyLine;
                g.HomeMoneyLine = game.HomeMoneyLine;
                g.OverUnder = game.OverUnder;
                g.HomeSpread = game.HomeSpread;
                db.Update(g);
                db.SaveChanges();
            }
        }

        public List<string> GetValidCategoryCodes(PlayerType playerType)
        {
            var validCodes = new List<string>();

            var q1 = (from ulpt in db.UserLeaguePlayerTypes.AsNoTracking().Include(i => i.CategoriesString)
                      where ulpt.PlayerTypeId == playerType.Id
                      group ulpt by ulpt.CategoriesString.Code into groupResult
                      select new
                      {
                          Count = groupResult.Count(),
                          CategoriesCode = groupResult.Max(f => f.CategoriesString.Code)
                      }).ToList();

            var q = (from c in q1 orderby c.Count descending select c).ToList();
            foreach (var code in q)
            {
                if (code.Count >= 15)
                {
                    validCodes.Add(code.CategoriesCode);
                }
            }

            return validCodes;
        }

        public PlayerInjury AddPlayerInjury(PlayerInjury playerInjury)
        {
            db.PlayerInjuries.Add(playerInjury);
            db.SaveChanges();

            return playerInjury;
        }

        public int AddPlayerInjuries(List<PlayerInjury> playerInjuries)
        {
            int added = 0;

            var currentInjuries = db.PlayerInjuries.ToList();

            foreach (var newPlayerInjury in playerInjuries)
            {
                db.PlayerInjuries.Add(newPlayerInjury);
                added++;
            }

            db.SaveChanges();

            return added;
        }

        public int UpdateExtraAnalysisLeagues(List<ExtraAnalysisLeague> extraAnalysisLeagues)
        {
            foreach (var el in db.ExtraAnalysisLeagues)
                db.ExtraAnalysisLeagues.Remove(el);
            db.SaveChanges();

            foreach (var el in extraAnalysisLeagues)
                db.ExtraAnalysisLeagues.Add(el);
            db.SaveChanges();

            return extraAnalysisLeagues.Count;
        }

        public CategoriesString GetDefaultCategoriesString(PlayerType playerType)
        {
            string cacheId = "GetDefaultCategoriesString" + playerType.Id.ToString();
            if (CacheItemExists(cacheId))
                return (CategoriesString)GetCacheItem(cacheId);

            CategoriesString outString = null;

            var q1 = (from ulpt in db.UserLeaguePlayerTypes.AsNoTracking()
                      where ulpt.PlayerTypeId == playerType.Id
                      group ulpt by ulpt.CategoriesString.Id into groupResult
                      select new
                      {
                          Count = groupResult.Count(),
                          CategoriesStringId = groupResult.Max(f => f.CategoriesStringId)
                      }).ToList();
            var q = (from c in q1 orderby c.Count descending select c).ToList();
            if (q.Count > 0)
            {
                outString = GetCategoriesString(q.First().CategoriesStringId);
            }

            AddCacheItem(cacheId, outString);

            return outString;
        }

        public string GetUserLeagueCategoryCode(UserLeague userLeague, PlayerType playerType)
        {
            var catString = (from pt in db.UserLeaguePlayerTypes.Include(i => i.CategoriesString) where pt.UserLeagueId == userLeague.Id && pt.PlayerTypeId == playerType.Id select pt.CategoriesString).FirstOrDefault();
            if (catString == null)
                catString = GetDefaultCategoriesString(playerType);
            //var ownershipPlayers = GetOwnershipPlayers(catString.Code, DateTime.UtcNow);
            //if (ownershipPlayers.Count == 0 || ownershipPlayers.First().LeagueCount < 10)
            //    catString = GetDefaultCategoriesString(playerType);

            return catString.Code;
        }

        public List<int> GetUserLeagueIdsWithCategoriesCode(string categoriesCode, Season season)
        {
            var ids = (from pt in db.UserLeaguePlayerTypes.Include(i => i.CategoriesString)
                       join ul in db.UserLeagues on pt.UserLeagueId equals ul.Id
                       where ul.SeasonId == season.Id && pt.CategoriesString.Code == categoriesCode
                       select pt.UserLeagueId).ToList();

            return ids;
        }

        public List<int> GetUserLeagueIdsWithNoWaivers(Season season)
        {
            var ids = (from ul in GetUserLeagues() where ul.SeasonId == season.Id && ul.FantasyProviderId == 4 && !ul.IsAuction && ul.IsProLeague select ul.Id).ToList();

            return ids;
        }

        public List<int> GetAuctionUserLeagueIds(Season season)
        {
            var ids = (from ul in GetUserLeagues() where ul.SeasonId == season.Id && ul.IsAuction && ul.FantasyProviderId == 1 && ul.IsProLeague select ul.Id).ToList();

            return ids;
        }

        public bool AddNFLGame(NFLGame game)
        {
            var currentGame = (from g in db.NFLGames where g.GameId == game.GameId select g).FirstOrDefault();
            if (currentGame != null)
            {
                db.Remove(currentGame);
                db.SaveChanges();
            }

            db.NFLGames.Add(game);
            db.SaveChanges();

            return true;
        }

        public List<NFLGame> GetNFLGames(Season season, DateTime startDate, DateTime endDate)
        {
            string cacheId = "GetNFLGames" + season.Id.ToString()
                + ":S" + startDate.ToShortDateString()
                + ":E" + endDate.ToShortDateString();
            if (CacheItemExists(cacheId))
                return (List<NFLGame>)GetCacheItem(cacheId);

            var nflGames = (from nflg in db.NFLGames.AsNoTracking()
                         .Include(i => i.Game).ThenInclude(t => t.AwayTeam).AsNoTracking()
                         .Include(i => i.Game).ThenInclude(t => t.HomeTeam).AsNoTracking()
                            join g in db.Games.Include(s => s.Season) on nflg.GameId equals g.Id
                            orderby g.GameDate ascending, g.GameTime ascending
                            where g.Season.Id == season.Id && g.GameDate >= startDate && g.GameDate <= endDate
                            select nflg).ToList();

            foreach (var nflGame in nflGames)
                nflGame.MoneyLineToWinsAndPoints();

            var games = (from g in db.Games.AsNoTracking()
                         .Include(t => t.AwayTeam).AsNoTracking()
                         .Include(t => t.HomeTeam).AsNoTracking()
                         where g.GameDate >= startDate && g.GameDate <= endDate
                         select g).ToList();

            foreach (var g in games)
            {
                if (nflGames.Find(nflg => nflg.GameId == g.Id) == null)
                {
                    NFLGame nflGame = new NFLGame();
                    nflGame.Game = g;
                    nflGame.GameId = g.Id;
                    nflGames.Add(nflGame);
                }
            }

            AddCacheItem(cacheId, nflGames);

            return nflGames;
        }

        public void DeleteNFLPlayerGames(Game game)
        {
            foreach (var playerGame in (from pg in db.NFLDefenseGames where pg.GameId == game.Id select pg))
                db.Remove(playerGame);
            foreach (var playerGame in (from pg in db.NFLKickerGames where pg.GameId == game.Id select pg))
                db.Remove(playerGame);
            foreach (var playerGame in (from pg in db.NFLOffensiveGame where pg.GameId == game.Id select pg))
                db.Remove(playerGame);
            db.SaveChanges();
        }

        public List<DepthPlayer> GetDepthPlayers(PlayerType playerType, string categoriesCode, DateTime dateTime, bool sortByActive)
        {
            string cacheId = "GetDepthPlayers"
                + ":PT" + playerType.Id.ToString()
                + ":S" + sortByActive.ToString()
                + ":C" + categoriesCode
                + ":D" + dateTime.ToShortDateString() + dateTime.ToShortTimeString();
            if (CacheItemExists(cacheId))
                return (List<DepthPlayer>)GetCacheItem(cacheId);

            var adpPlayers = GetAdpPlayers(GetDefaultSeason(), GetDefaultCategoriesString(playerType).Code, 50, GetDefaultSeason().StartDate.AddDays(-60));

            var depthPlayers = new List<DepthPlayer>();

            var ownershipPlayers = GetOwnershipPlayers(categoriesCode, dateTime);
            var season = GetDefaultSeason();
            foreach (var seasonPlayer in GetSeasonPlayers(season, playerType))
            {
                var ownershipPlayer = ownershipPlayers.Find(p => p.PlayerId == seasonPlayer.PlayerId);
                if (ownershipPlayer != null)
                {
                    DepthPlayer depthPlayer = new DepthPlayer();
                    depthPlayer.SeasonPlayer = seasonPlayer;
                    depthPlayer.OwnershipPlayer = ownershipPlayer;
                    var adpPlayer = (from adp in adpPlayers where adp.PlayerId == ownershipPlayer.PlayerId select adp).FirstOrDefault();
                    depthPlayer.TieBreakerSort = (adpPlayer != null) ? adpPlayer.Adp : double.MaxValue;
                    depthPlayers.Add(depthPlayer);
                }
            }

            foreach (var team in GetTeams())
            {
                foreach (var position in (from p in GetPositions()
                                          where p.PlayerType.Id == playerType.Id && p.IsActualPosition
                                          orderby p.DisplayOrder
                                          select p))
                {
                    var positionDepthPlayers = new List<DepthPlayer>();
                    foreach (var dp in (from dp1 in depthPlayers where dp1.SeasonPlayer.TeamId == team.Id select dp1))
                    {
                        var pp = (from pp1 in GetPlayerDefaultPositions() where pp1.PlayerId == dp.SeasonPlayer.PlayerId && pp1.PositionId == position.Id select dp).FirstOrDefault();
                        if (pp != null)
                        {
                            positionDepthPlayers.Add(dp);
                            dp.Position = position;
                        }
                    }

                    int depth = 0;
                    List<DepthPlayer> sorted;
                    if (sortByActive)
                        sorted = (from dp1 in positionDepthPlayers orderby dp1.OwnershipPlayer.ActivePercent descending, dp1.OwnershipPlayer.OwnershipPercent descending, dp1.TieBreakerSort select dp1).ToList();
                    else
                        sorted = (from dp1 in positionDepthPlayers orderby dp1.OwnershipPlayer.OwnershipPercent descending, dp1.OwnershipPlayer.ActivePercent descending, dp1.TieBreakerSort select dp1).ToList();

                    double totalOwnership = 0;
                    double totalActive = 0;
                    foreach (var sortPlayer in sorted)
                    {
                        sortPlayer.Depth = ++depth;
                        sortPlayer.Team = team;
                        totalOwnership += sortPlayer.OwnershipPlayer.OwnershipPercent;
                        totalActive += sortPlayer.OwnershipPlayer.ActivePercent;
                    }

                    foreach (var sortPlayer in sorted)
                    {
                        if (totalOwnership > 0)
                            sortPlayer.OwnershipDepthPercent = sortPlayer.OwnershipPlayer.OwnershipPercent / totalOwnership * 100;
                        if (totalActive > 0)
                            sortPlayer.ActiveDepthPercent = sortPlayer.OwnershipPlayer.ActivePercent / totalActive * 100;
                    }
                }
            }

            foreach (var depthPlayer in depthPlayers)
            {
                if (depthPlayer.Team == null)
                    continue;

                if (depthPlayer.Depth <= 1)
                {
                    depthPlayer.HigherDepthPlayers = null;
                }
                else
                {
                    depthPlayer.HigherDepthPlayers = (from dp in depthPlayers
                                                      where dp.Team != null && dp.Position != null && dp.Team.Id == depthPlayer.Team.Id && dp.Position.Id == depthPlayer.Position.Id && dp.Depth < depthPlayer.Depth
                                                      orderby dp.Depth ascending
                                                      select dp).ToList();
                }
            }


            AddCacheItem(cacheId, depthPlayers);

            return depthPlayers;
        }

        public MonsterBar GetMonsterBar(
            PlayerType playerType,
            Season season,
            List<CategorySetting> categorySettings,
            string scoringSystem,
            PerValue perValue,
            int leagueSize,
            int activeSize)
        {
            string cacheId = "GetMonsterBar"
                + ":PT" + playerType.Id.ToString()
                + ":S" + season.Id.ToString()
                + ":SC" + scoringSystem
                + ":PV" + perValue.Id.ToString()
                + ":LS" + leagueSize.ToString();
            foreach (var cs in categorySettings)
                cacheId += "CS" + cs.Category.Id.ToString() + "|" + cs.PointsPerStat.ToString() + "|" + cs.IsActive.ToString();

            if (CacheItemExists(cacheId))
                return (MonsterBar)GetCacheItem(cacheId);

            MonsterBar monsterBar = new MonsterBar();
            monsterBar.PlayerType = playerType;
            Category gamesCategory = GetGamesCategory(playerType.Id);
            Category measureCategory = GetMeasureCategory(playerType.Id);
            if (measureCategory != null)
                monsterBar.MeasureText = measureCategory.Title.ToLower();
            int topRank = activeSize > 0 ? activeSize : Convert.ToInt32((double)leagueSize * 0.8);
            int ownableRank = Math.Max(topRank, leagueSize);

            List<string> ids = new List<string>();

            if (Sport.IsNBA || Sport.IsMLB || Sport.IsNHL)
            {
                ids.Add("LS");
                ids.Add("S");
                ids.Add("2M");
                ids.Add("3W");
                ids.Add("W");
            }

            if (Sport.IsNFL)
            {
                ids.Add("LS");
                ids.Add("S");
                ids.Add("2M");
                ids.Add("3W");
                ids.Add("W");
            }

            foreach (var id in ids)
            {
                MonsterBarItem item = new MonsterBarItem();
                Season processSeason = season;
                DateTime startDate = processSeason.StartDate;
                DateTime endDate = processSeason.UpdatedDate;
                switch (id)
                {
                    case "LS":
                        processSeason = GetPreviousSeason(season.Year.GetValueOrDefault(season.StartDate.Year) - 1);
                        if (processSeason != null)
                        {
                            startDate = processSeason.StartDate;
                            endDate = processSeason.UpdatedDate;
                        }
                        item.Description = "Last Season";
                        break;
                    case "2M":
                        startDate = endDate.AddMonths(-2);
                        item.Description = "Past 2 Months";
                        break;
                    case "3W":
                        startDate = endDate.AddDays(-20);
                        item.Description = "Past 3 Weeks";
                        break;
                    case "W":
                        startDate = endDate.AddDays(-6);
                        item.Description = "Past Week";
                        break;
                    case "D":
                        startDate = endDate;
                        item.Description = GetStartedGameDate(season).DayOfWeek.ToString();
                        break;
                    default:
                        item.Description = "Current Season";
                        break;
                }

                ValueAverages valueAverages;
                if (processSeason != null)
                    item.ValuePlayers = GetValuePlayers(playerType, processSeason, startDate, endDate, 0, categorySettings, scoringSystem, perValue, leagueSize, true, out valueAverages);
                else
                    item.ValuePlayers = new List<ValuePlayer>();
                item.Title = id;
                item.DisplayOrder = monsterBar.MonsterBarItems.Count + 1;
                monsterBar.MonsterBarItems.Add(item);
            }

            var seasonPlayers = GetSeasonPlayers(season, playerType);
            foreach (var seasonPlayer in seasonPlayers)
            {
                var monsterBarPlayer = new MonsterBarPlayer();
                monsterBarPlayer.Player = seasonPlayer.Player;
                monsterBar.MonsterBarPlayers.Add(monsterBarPlayer);

                ValuePlayer prev = null;
                for (int i = monsterBar.MonsterBarItems.Count - 1; i >= 0; i--)
                {
                    var item = monsterBar.MonsterBarItems[i];
                    var monsterBarValuePlayer = new MonsterBarValuePlayer();

                    monsterBarValuePlayer.ValuePlayer = (from vp in item.ValuePlayers where vp.Player.Id == monsterBarPlayer.Player.Id select vp).FirstOrDefault();
                    monsterBarValuePlayer.IsTopPlayer = monsterBarValuePlayer.ValuePlayer == null ? false : monsterBarValuePlayer.ValuePlayer.Rank <= topRank;
                    monsterBarValuePlayer.IsOwnablePlayer = monsterBarValuePlayer.ValuePlayer == null ? false : monsterBarValuePlayer.ValuePlayer.Rank <= ownableRank;

                    if (monsterBarValuePlayer.ValuePlayer == null)
                        monsterBarPlayer.MonsterBarValuePlayers.Insert(0, null);
                    else if (prev != null && monsterBarValuePlayer.ValuePlayer != null)
                    {
                        if (prev.StatPlayer.Get(gamesCategory.Id) == monsterBarValuePlayer.ValuePlayer.StatPlayer.Get(gamesCategory.Id))
                            monsterBarPlayer.MonsterBarValuePlayers.Insert(0, null);
                        else
                        {
                            monsterBarPlayer.MonsterBarValuePlayers.Insert(0, monsterBarValuePlayer);
                            prev = monsterBarValuePlayer.ValuePlayer;
                        }
                    }
                    else if (monsterBarValuePlayer != null && prev == null)
                    {
                        monsterBarPlayer.MonsterBarValuePlayers.Insert(0, monsterBarValuePlayer);
                        prev = monsterBarValuePlayer.ValuePlayer;
                    }
                }
            }

            foreach (var monsterBarPlayer in monsterBar.MonsterBarPlayers)
            {
                monsterBarPlayer.IsGoodFreeAgent = false;
                for (int i = 1; i < ids.Count; i++) // skip last season
                {
                    var monsterBotValuePlayer = monsterBarPlayer.MonsterBarValuePlayers[i];
                    if (monsterBotValuePlayer != null && monsterBotValuePlayer.IsTopPlayer)
                    {
                        monsterBarPlayer.IsGoodFreeAgent = true;
                        break;
                    }
                }
            }

            AddCacheItem(cacheId, monsterBar);

            return monsterBar;
        }

        public List<BoxScorePlayer> GetBoxScorePlayers(Season season, Game game, bool onlyPlayed)
        {
            var boxScorePlayers = new List<BoxScorePlayer>();

            foreach (var playerType in GetPlayerTypes())
            {
                var seasonPlayers = GetSeasonPlayers(season, playerType);
                var statPlayers = GetStatPlayers(playerType, season.Id, game.GameDate, game.GameDate, false, game);
                foreach (var statPlayer in statPlayers)
                {
                    var boxScorePlayer = new BoxScorePlayer();
                    boxScorePlayer.Game = game;
                    boxScorePlayer.SeasonPlayer = GetSeasonPlayer(statPlayer.Player.Id);
                    boxScorePlayer.Team = statPlayer.Team2 != null ? statPlayer.Team2 : statPlayer.Team;
                    boxScorePlayers.Add(boxScorePlayer);
                }

                if (!onlyPlayed)
                {
                    var gamePlayers = (from sp in seasonPlayers where game.IncludesTeam(sp.Team.Id) select sp).ToList();
                    foreach (var gamePlayer in gamePlayers)
                    {
                        if (boxScorePlayers.Find(p => p.SeasonPlayer.PlayerId == gamePlayer.PlayerId) == null)
                        {
                            var boxScorePlayer = new BoxScorePlayer();
                            boxScorePlayer.Game = game;
                            boxScorePlayer.SeasonPlayer = GetSeasonPlayer(gamePlayer.PlayerId);
                            boxScorePlayer.Team = gamePlayer.Team;
                            boxScorePlayers.Add(boxScorePlayer);
                        }
                    }
                }
            }

            return boxScorePlayers;
        }

        public List<UserLeagueWaiverPlayer> GetUserLeagueWaiverPlayers(UserLeague userLeague)
        {
            if (userLeague == null)
                return new List<UserLeagueWaiverPlayer>();

            var ww = (from p in db.UserLeagueWaiverPlayers where p.UserLeagueId == userLeague.Id select p).ToList();

            return ww;
        }

        public bool AddNFLOffensiveGame(NFLOffensiveGame offensiveGame)
        {
            db.NFLOffensiveGame.Add(offensiveGame);
            db.SaveChanges();

            return true;
        }

        public bool AddNFLKickerGame(NFLKickerGame pg)
        {
            db.NFLKickerGames.Add(pg);
            db.SaveChanges();

            return true;
        }


        public bool AddNFLDefenseGame(NFLDefenseGame pg)
        {
            db.NFLDefenseGames.Add(pg);
            db.SaveChanges();

            return true;
        }

        public int ClearNHLPlayerGames(Game game)
        {
            int deleted = 0;
            foreach (var playerGame in (from pg in db.NHLGoalieGames where pg.GameId == game.Id select pg))
            {
                deleted++;
                db.NHLGoalieGames.Remove(playerGame);
            }
            foreach (var playerGame in (from pg in db.NHLSkaterGames where pg.GameId == game.Id select pg))
            {
                deleted++;
                db.NHLSkaterGames.Remove(playerGame);
            }
            db.SaveChanges();

            return deleted;
        }

        public bool AddNHLSkaterGame(NHLSkaterGame pg)
        {
            db.NHLSkaterGames.Add(pg);
            db.SaveChanges();

            return true;
        }

        public bool AddNHLGoalieGame(NHLGoalieGame pg)
        {
            db.NHLGoalieGames.Add(pg);
            db.SaveChanges();

            return true;
        }



        public void UpdateSeasonPlayerTeam(Season season, int playerId, int teamId)
        {
            var seasonPlayer = (from sp in db.SeasonPlayers where sp.SeasonId == season.Id && sp.PlayerId == playerId select sp).FirstOrDefault();
            if (seasonPlayer != null)
            {
                seasonPlayer.Team = db.Teams.Find(teamId);
                db.Update(seasonPlayer);
                db.SaveChanges();
            }
        }

        public Player FindPlayer(string firstName, string lastName, DateTime birthdate, bool birthdateIsValid = true)
        {
            var lastNameMatches = (from p in GetPlayers() where p.LastName.Contains(lastName) || lastName.Contains(p.LastName) select p).ToList();
            if (lastNameMatches.Count > 0)
            {
                var firstNameMatches = (from p in lastNameMatches where p.FirstName == firstName select p).ToList();
                if (firstNameMatches.Count == 0)
                    firstNameMatches = (from p in lastNameMatches where p.FirstName.Substring(0, 1) == firstName.Substring(0, 1) select p).ToList();

                List<Player> matches = null;
                if (birthdateIsValid)
                    matches = (from p in firstNameMatches where p.Birthdate == birthdate select p).ToList();
                else
                    matches = firstNameMatches;

                if (matches.Count == 1)
                    return matches.First();

                return null;
            }

            return null;
        }

        public List<PlayerInjury> UpdatePlayerInjuries(List<PlayerInjury> playerInjuries)
        {
            var newInjuries = new List<PlayerInjury>();

            try
            {
                var now = DateTime.UtcNow;

                var oldInjuries = GetPlayerInjuries();

                var newPlayerStatuses = new List<PlayerStatus>();

                foreach (var inj in playerInjuries)
                {
                    var match = (from i in oldInjuries
                                 where i.PlayerId == inj.PlayerId
                                 && i.InjuryStatus == inj.InjuryStatus
                                 && i.PlayerStatus == inj.PlayerStatus
                                 && i.Comment == inj.Comment
                                 && i.Description == inj.Description
                                 && i.EstimatedReturnDate == inj.EstimatedReturnDate
                                 select i).FirstOrDefault();
                    if (match == null)
                    {
                        inj.StartDate = inj.UpdateDate = now;
                        newInjuries.Add(inj);

                        var playerStatus = new PlayerStatus();
                        playerStatus.PlayerId = inj.PlayerId;
                        playerStatus.Subject = inj.Description;
                        playerStatus.Comment = inj.Comment;
                        playerStatus.PlayerStatusTypeId = GetPlayerStatusTypeByName(inj.PlayerStatus).Id;
                        playerStatus.DateAdded = now;
                        playerStatus.IsActive = true;
                        playerStatus.EstimatedReturnDate = inj.EstimatedReturnDate;
                        newPlayerStatuses.Add(playerStatus);
                    }
                }

                foreach (var oldInjury in oldInjuries)
                {
                    var match = (from i in playerInjuries
                                 where i.PlayerId == oldInjury.PlayerId
                                 && i.InjuryStatus == oldInjury.InjuryStatus
                                 && i.PlayerStatus == oldInjury.PlayerStatus
                                 && i.Comment == oldInjury.Comment
                                 && i.Description == oldInjury.Description
                                 && i.EstimatedReturnDate == oldInjury.EstimatedReturnDate
                                 select i).FirstOrDefault();
                    if (match == null)
                    {
                        foreach (var oldActive in (from ps in db.PlayerStatuses where ps.PlayerId == oldInjury.PlayerId && ps.IsActive && oldInjury.UpdateDate == ps.DateAdded select ps))
                        {
                            oldActive.IsActive = false;
                            oldActive.DateDeactivated = now;
                            db.Update(oldActive);

                            //if ((from ps in newPlayerStatuses where ps.PlayerId==oldActive.PlayerId select ps).FirstOrDefault() == null)
                            //{
                            //    var playerStatus = new PlayerStatus();
                            //    playerStatus.PlayerId = oldActive.PlayerId;
                            //    playerStatus.Subject = "OK";
                            //    playerStatus.Comment = "";
                            //    playerStatus.PlayerStatusTypeId = GetPlayerStatusTypeByName("Playing").Id;
                            //    playerStatus.DateAdded = now;
                            //    playerStatus.IsActive = true;
                            //    newPlayerStatuses.Add(playerStatus);
                            //}
                        }
                        db.Remove(oldInjury);
                    }
                }
                db.SaveChanges();

                foreach (var newInjury in newInjuries)
                    AddPlayerInjury(newInjury);

                foreach (var newPlayerStatus in newPlayerStatuses)
                    AddPlayerStatus(newPlayerStatus);

                // make sure there's only one active for each player
                int prevPlayerId = 0;
                foreach (var playerStatus in (from ps in db.PlayerStatuses orderby ps.PlayerId, ps.DateAdded descending select ps))
                {
                    if (playerStatus.PlayerId == prevPlayerId)
                        playerStatus.IsActive = false;
                    prevPlayerId = playerStatus.PlayerId;
                }
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return newInjuries;
        }

        public PlayerStatus AddPlayerStatus(PlayerStatus playerStatus)
        {
            db.Add(playerStatus);
            playerStatus.IsActive = true;
            db.SaveChanges();

            return playerStatus;
        }

        public PlayerStatus DisablePlayerStatus(int playerId)
        {
            PlayerStatus disabledPlayerStatus = null;

            var activePlayerStatuses = (from pst in db.PlayerStatuses where pst.PlayerId == playerId && pst.IsActive select pst).ToList();
            if (activePlayerStatuses.Count() > 0)
            {
                foreach (var pst in activePlayerStatuses)
                {
                    disabledPlayerStatus = pst;
                    pst.IsActive = false;
                    db.Update(pst);
                }
            }

            return disabledPlayerStatus;
        }

        public PlayerStatusType GetPlayerStatusTypeByName(string name)
        {
            name = name.Replace("COVID-19", "Out");
            name = name.Replace("IR", "Injured");
            name = name.Replace("PUP-R", "Injured");
            name = name.Replace("Injured Reserve – Long Term", "Injured");
            name = name.Replace("Injured Reserve", "Injured");
            name = name.Replace("Out Indefinitely", "Injured");
            name = name.Replace("60-Day IL", "Injured");
            name = name.Replace("Out Indefinitely", "Injured");
            name = name.Replace("Day To Day", "Questionable");
            name = name.Replace("Day-to-Day", "Questionable");
            if (name.IndexOf("Day IL") >= 0)
                name = "Injured";

            var playerStatusTypes = db.PlayerStatusTypes.ToList();

            var exactMatch = (from pst in playerStatusTypes where pst.Title.ToUpper() == name.ToUpper() select pst).FirstOrDefault();
            if (exactMatch != null)
                return exactMatch;

            PlayerStatusType playerStatusType = (from pst in playerStatusTypes where pst.Title == "Note" select pst).FirstOrDefault();   // default to Note if no match

            return playerStatusType;
        }

        public List<PlayerStatus> GetActivePlayerStatuses()
        {
            var playerStatuses = (from ps in db.PlayerStatuses.AsNoTracking()
                                  .Include(ps => ps.Player)
                                  .Include(ps => ps.PlayerStatusType)
                                  .Include(ps => ps.PlayerStatusTagType)
                                  where ps.IsActive
                                  orderby ps.DateAdded descending, ps.PlayerId
                                  select ps).ToList();

            var seasonGames = GetGames(GetDefaultSeason());

            foreach (var playerStatus in playerStatuses)
            {
                if (playerStatus.EstimatedReturnDate != null)
                {
                    var seasonPlayer = GetSeasonPlayer(playerStatus.PlayerId);
                    if (seasonPlayer != null)
                    {
                        playerStatus.EstimatedGamesToMiss = (from g in seasonGames
                                                             where
                                         g.IncludesTeam(seasonPlayer.TeamId)
                                         && !g.IsFinished
                                         && g.GameDate < playerStatus.EstimatedReturnDate
                                                             select g).ToList();
                    }
                }
            }

            return playerStatuses;
        }

        public PlayerStatus GetPlayerActivePlayerStatus(int playerId)
        {
            var playerStatus = (from ps in GetActivePlayerStatuses() where ps.PlayerId == playerId orderby ps.DateAdded descending select ps).FirstOrDefault();

            return playerStatus;
        }

        public TimeSpan TimeUntilNextGame(Season season)
        {
            var nextGame = NextGame(season);

            if (nextGame != null)
                return nextGame.TimeUntilGame;
            else
                return new TimeSpan();
        }

        public Game NextGame(Season season)
        {
            var games = GetGames(season);

            var nextGame = (from g in games
                            where !g.IsFinished && g.TimeUntilGame.TotalSeconds > 0
                            orderby g.GameTime ascending,
                            g.HomeTeam.Code ascending
                            select g).FirstOrDefault();

            return nextGame;
        }

        public DateTime GetLiveGameDate(Season season)
        {
            // live game date or latest finished date
            string cacheId = "GetLiveGameDate" + season.Id.ToString();
            if (CacheItemExists(cacheId))
                return (DateTime)GetCacheItem(cacheId);

            DateTime liveDate = DateTime.Today;
            var game = (from g in GetGames(season) where g.HasStarted && !g.IsFinished orderby g.GameTime descending select g).FirstOrDefault();
            if (game != null)
                liveDate = game.GameDate;
            else
            {
                var game2 = (from g in GetGames(season) where g.IsFinished orderby g.GameTime descending select g).FirstOrDefault();
                if (game2 != null)
                    liveDate = game2.GameDate;
            }

            AddCacheItem(cacheId, liveDate);

            return liveDate;
        }

        public DateTime GetLiveStartGameDate(Season season)
        {
            string cacheId = "GetLiveStartGameDate" + season.Id.ToString();
            if (CacheItemExists(cacheId))
                return (DateTime)GetCacheItem(cacheId);

            DateTime startDate = GetLiveGameDate(season);
            if (Sport.IsNFL)
            {
                DayOfWeek startWeekDay = Sport.StartDayOfWeek;
                while (startDate.DayOfWeek != startWeekDay)
                    startDate = startDate.AddDays(-1);
            }

            AddCacheItem(cacheId, startDate);

            return startDate;
        }

        public DateTime GetLiveEndGameDate(Season season)
        {
            string cacheId = "GetLiveEndGameDate" + season.Id.ToString();
            if (CacheItemExists(cacheId))
                return (DateTime)GetCacheItem(cacheId);

            DateTime endDate = GetLiveGameDate(season);
            if (Sport.IsNFL)
            {
                DayOfWeek startWeekDay = Sport.StartDayOfWeek;
                while (endDate.DayOfWeek != startWeekDay)
                    endDate = endDate.AddDays(1);
                endDate = endDate.AddDays(-1);  // step back one day
            }

            AddCacheItem(cacheId, endDate);

            return endDate;
        }

        public DateTime GetUpcomingGamesStartDate(Season season)
        {
            string cacheId = "GetUpcomingGamesStartDate" + season.Id.ToString();
            if (CacheItemExists(cacheId))
                return (DateTime)GetCacheItem(cacheId);

            DateTime startDate = GetLiveGameDate(season);

            var unfinishedGame = (from g in GetGames(season) where !g.IsFinished orderby g.GameDate ascending select g).FirstOrDefault();
            if (unfinishedGame != null)
            {
                startDate = unfinishedGame.GameDate;
                if (Sport.IsNFL)
                {
                    DayOfWeek startWeekDay = Sport.StartDayOfWeek;
                    while (startDate.DayOfWeek != startWeekDay)
                        startDate = startDate.AddDays(-1);
                }
            }

            AddCacheItem(cacheId, startDate);

            return startDate;
        }

        public DateTime GetUpcomingGamesEndDate(Season season)
        {
            string cacheId = "GetUpcomingGamesEndDate" + season.Id.ToString();
            if (CacheItemExists(cacheId))
                return (DateTime)GetCacheItem(cacheId);

            DateTime endDate = GetUpcomingGamesStartDate(season);
            if (Sport.IsNFL)
                endDate = endDate.AddDays(6);

            AddCacheItem(cacheId, endDate);

            return endDate;
        }

        public DateTime GetCurrentGameDate(Season season)
        {
            string cacheId = "GetCurrentGameDate" + season.Id.ToString();
            if (CacheItemExists(cacheId))
                return (DateTime)GetCacheItem(cacheId);

            DateTime currentGameDate = DateTime.Today;

            var game = (from g in GetGames(season) where g.HasStarted && !g.IsFinished select g).FirstOrDefault();
            if (game != null)
                currentGameDate = game.GameDate;

            AddCacheItem(cacheId, currentGameDate);

            return currentGameDate;
        }

        public DateTime GetStartedGameDate(Season season)
        {
            string cacheId = "GetStartedGameDate" + season.Id.ToString();
            if (CacheItemExists(cacheId))
                return (DateTime)GetCacheItem(cacheId);

            var games = GetGames(season);
            var game = (from g in games where g.HasStarted orderby g.GameDate descending select g).FirstOrDefault();

            DateTime startedGameDate = DateTime.Today;

            if (game != null)
                startedGameDate = game.GameDate;

            AddCacheItem(cacheId, startedGameDate);

            return startedGameDate;
        }

        public DateTime GetActivePeriodStartDate(Season season, int weeksBack = 0)
        {
            string cacheId = "GetActivePeriodStartDate:S" + season.Id.ToString() + ":W" + weeksBack.ToString();
            if (CacheItemExists(cacheId))
                return (DateTime)GetCacheItem(cacheId);

            DateTime currentDate;

            DayOfWeek startWeekDay = Sport.StartDayOfWeek;
            var lastGame = (from g in GetGames(season) where g.SeasonId == season.Id && g.HasStarted orderby g.GameTime descending select g).FirstOrDefault();
            if (lastGame != null)
            {
                currentDate = lastGame.GameDate;
                while (currentDate.DayOfWeek != startWeekDay)
                    currentDate = currentDate.AddDays(-1);
                if (weeksBack > 0)
                    currentDate = currentDate.AddDays(-7 * weeksBack);
            }
            else
            {
                currentDate = DateTime.Today;
            }

            AddCacheItem(cacheId, currentDate);

            return currentDate;
        }

        public int GetCurrentWeekNumber(Season season)
        {
            DateTime firstPeriodDate = season.StartDate;
            while (firstPeriodDate.DayOfWeek != Sport.StartDayOfWeek)
                firstPeriodDate = firstPeriodDate.AddDays(-1);

            var nextGame = NextGame(season);

            DateTime gameDate = nextGame.GameDate;
            while (gameDate.DayOfWeek != Sport.StartDayOfWeek)
                gameDate = gameDate.AddDays(-1);

            TimeSpan diff = gameDate - season.StartDate;

            int week = 0;
            if (diff.TotalDays > 0)
                week = Convert.ToInt32(diff.TotalDays) / 7 + 1;

            return week;
        }

        public DateTime GetPeriod(Season season, int weeksBack = 0)
        {
            string cacheId = "GetPeriod:S" + season.Id.ToString() + ":W" + weeksBack.ToString();
            if (CacheItemExists(cacheId))
                return (DateTime)GetCacheItem(cacheId);

            DateTime currentDate;

            DayOfWeek startWeekDay = Sport.StartDayOfWeek;
            var nextGame = NextGame(season);
            if (nextGame != null)
            {
                currentDate = nextGame.GameDate;
                while (currentDate.DayOfWeek != startWeekDay)
                    currentDate = currentDate.AddDays(-1);
                if (weeksBack > 0)
                    currentDate = currentDate.AddDays(-7 * weeksBack);
            }
            else
            {
                currentDate = DateTime.Today;
            }

            AddCacheItem(cacheId, currentDate);

            return currentDate;
        }

        public PerValue GetPerGamePerValue(int playerTypeId)
        {
            var cacheId = "GetPerGamePerValue" + playerTypeId.ToString();
            if (CacheItemExists(cacheId))
                return (PerValue)GetCacheItem(cacheId);

            var perValues = GetPerValues(playerTypeId);
            Category gamesCat = GetGamesCategory(playerTypeId);

            var perValue = (from pv in perValues where pv.CategoryId == gamesCat.Id select pv).FirstOrDefault();

            AddCacheItem(cacheId, perValue);

            return perValue;
        }

        public PerValue GetTotalPerValue(int playerTypeId)
        {
            var cacheId = "GetTotalPerValue" + playerTypeId.ToString();
            if (CacheItemExists(cacheId))
                return (PerValue)GetCacheItem(cacheId);

            var perValues = GetPerValues(playerTypeId);
            Category gamesCat = GetGamesCategory(playerTypeId);

            var perValue = (from pv in perValues where pv.CategoryId == null select pv).FirstOrDefault();

            AddCacheItem(cacheId, perValue);

            return perValue;
        }

        public List<OwnershipPlayer> GetTrendingPlayers()
        {
            int hours;
            if (Sport.IsNFL)
                hours = 24 * 3;
            else
                hours = 24;

            return GetOwnershipPlayersWithChange("nowaiver", DateTime.UtcNow, hours);
        }

        public List<Game> GetGames(Season season, DateTime startDate, DateTime endDate)
        {
            var cacheId = "GetGames"
                + "S:" + season.Id.ToString()
                + "SD:" + startDate.ToShortDateString()
                + "ED:" + endDate.ToShortDateString();
            if (CacheItemExists(cacheId))
                return (List<Game>)GetCacheItem(cacheId);

            var games = (from g in db.Games.AsNoTracking()
                         .Include(t => t.AwayTeam).AsNoTracking()
                         .Include(t => t.HomeTeam).AsNoTracking()
                         where g.GameDate >= startDate && g.GameDate <= endDate && (g.IsPostponed == null ? true : false)
                         orderby g.GameDate, g.GameTime
                         select g).ToList();

            foreach (var game in games)
                game.FillProperties(Sport, colorLib);

            AddCacheItem(cacheId, games);

            return games;
        }

        public List<PlayerGameStateType> GetPlayerGameStateTypes()
        {
            string cacheId = "GetPlayerGameStateTypes";
            if (CacheItemExists(cacheId))
                return (List<PlayerGameStateType>)GetCacheItem(cacheId);

            var playerGameStateTypes = db.PlayerGameStateTypes.ToList();
            AddCacheItem(cacheId, playerGameStateTypes);

            return playerGameStateTypes;
        }

        public int ClearDatePlayerGameStates(DateTime gameDate)
        {

            var deletes = (from p in db.PlayerGameStates
                           join g in db.Games on p.GameId equals g.Id
                           where g.GameDate == gameDate
                           select p
                         );
            foreach (var d in deletes)
            {
                db.Remove(d);
            }
            db.SaveChanges();

            return 0;
        }

        public bool AddPlayerGameState(PlayerGameState playerGameState)
        {
            try
            {
                var player = (from p1 in db.Players where p1.Id == playerGameState.PlayerId select p1).FirstOrDefault();
                if (player != null)
                {
                    var current = (from p in db.PlayerGameStates where p.PlayerId == playerGameState.PlayerId && p.GameId == playerGameState.GameId select p).FirstOrDefault();
                    if (current != null)
                    {
                        db.Remove(current);
                        db.SaveChanges();
                    }

                    db.Add(playerGameState);
                    if (db.SaveChanges() > 0)
                        return true;
                }
            }
            catch (Exception ex)
            {
            }

            return false;
        }

        public List<PlayerGameState> GetPlayerGameStates(DateTime startDate, DateTime endDate)
        {
            var playerGameStates = (from p in db.PlayerGameStates.AsNoTracking()
                .Include(i => i.Player)
                .Include(i => i.Team)
                .Include(i => i.Game)
                .Include(i => i.PlayerGameStateType)
                                    join g in db.Games.AsNoTracking() on p.GameId equals g.Id
                                    where g.GameDate >= startDate && g.GameDate <= endDate
                                    select p).ToList();

            return playerGameStates;
        }

        public List<PlayerGameDate> GetPlayerGameDates(DateTime startDate, DateTime endDate, List<ValuePlayer> teamEaseValuePlayers)
        {
            var playerGameDates = new List<PlayerGameDate>();
            var season = GetDefaultSeason();

            var playerGameStates = GetPlayerGameStates(startDate, endDate);

            var seasonPlayers = new List<SeasonPlayer>();
            foreach (var pt in GetPlayerTypes())
                foreach (var seasonPlayer in GetSeasonPlayers(season, pt))
                    seasonPlayers.Add(seasonPlayer);

            var games = GetGames(season, startDate, endDate);
            foreach (var game in games)
            {
                foreach (var team in game.GetTeams())
                {
                    foreach (var teamPlayer in (from tp in seasonPlayers where tp.TeamId == team.Id select tp))
                    {
                        PlayerGameDate playerGameDate = new PlayerGameDate();
                        playerGameDate.SeasonPlayer = teamPlayer;
                        playerGameDate.Game = game;
                        playerGameDate.PlayerGameState = (from pgs in playerGameStates where pgs.PlayerId == teamPlayer.PlayerId && pgs.GameId == game.Id select pgs).FirstOrDefault();
                        if (teamEaseValuePlayers != null)
                        {
                            var activeRosterSpot = GetEaseActiveRosterSpot(teamPlayer.Player.DefaultPosition);
                            if (activeRosterSpot != null)
                                playerGameDate.EaseValuePlayer = valuePlayerLib.GetTeamValuePlayer(teamEaseValuePlayers, game.GetOpponent(team), activeRosterSpot);
                        }
                        playerGameDates.Add(playerGameDate);
                    }
                }
            }

            return playerGameDates;
        }

        public CompletedTask GetCompletedTask(string taskId)
        {
            var completedTask = (from t in db.CompletedTasks where t.TaskId == taskId select t).FirstOrDefault();

            return completedTask;
        }

        public CompletedTask AddCompletedTask(CompletedTask completedTask)
        {
            db.CompletedTasks.Add(completedTask);
            db.SaveChanges();

            return completedTask;
        }

        public bool AddArticle(Article article)
        {
            var current = (from a in db.Articles where a.SportRadarId == article.SportRadarId select a).FirstOrDefault();
            if (current != null)
            {
                if (current.UpdatedDate == article.UpdatedDate)
                    return false;

                DeleteArticle(current.Id);
            }

            db.Articles.Add(article);
            db.SaveChanges();

            return true;
        }

        public void DeleteArticle(int articleId)
        {
            var article = (from a in db.Articles
                           .Include(i => i.ArticleGames)
                           .Include(i => i.ArticlePlayers)
                           .Include(i => i.ArticleTeams)
                           where a.Id == articleId
                           select a).FirstOrDefault();
            if (article != null)
            {
                foreach (var ag in article.ArticleGames)
                    db.Remove(ag);
                foreach (var ap in article.ArticlePlayers)
                    db.Remove(ap);
                foreach (var at in article.ArticleTeams)
                    db.Remove(at);
                db.SaveChanges();
                db.Remove(article);
                db.SaveChanges();
            }
        }

        public Article GetArticle(int articleId)
        {
            var article = (from a in db.Articles where a.Id == articleId select a).FirstOrDefault();

            return article;
        }

        public List<Article> GetArticles(DateTime startDate, DateTime endDate, bool includeAutomatedArticles = true)
        {
            string cacheId = "GetArticles" + startDate.ToShortDateString() + ":" + endDate.ToShortDateString() + ":" + includeAutomatedArticles.ToString();
            if (CacheItemExists(cacheId))
                return (List<Article>)GetCacheItem(cacheId);

            var articles = (from a in db.Articles
                            .Include(i => i.ArticleGames).ThenInclude(i2 => i2.Game).ThenInclude(i3 => i3.AwayTeam)
                            .Include(i => i.ArticleGames).ThenInclude(i2 => i2.Game).ThenInclude(i3 => i3.HomeTeam)
                            .Include(i => i.ArticlePlayers).ThenInclude(i2 => i2.Player)
                            .Include(i => i.ArticleTeams).ThenInclude(i2 => i2.Team)
                            where a.CreatedDate >= startDate && a.CreatedDate <= endDate
                                && (!includeAutomatedArticles ? a.Byline != "By The Associated Press" : true)
                            orderby a.CreatedDate descending
                            select a).ToList();

            AddCacheItem(cacheId, articles);

            return articles;
        }

        public List<Article> GetRecentArticles(int pastHours = 24 * 2)
        {
            string cacheId = "GetRecentArticles";
            if (CacheItemExists(cacheId))
                return (List<Article>)GetCacheItem(cacheId);
            var articles = GetArticles(DateTime.Today.AddHours(-1 * pastHours), DateTime.Today.AddDays(1), false);

            AddCacheItem(cacheId, articles);

            return articles;
        }

        public List<Article> GetPlayerRecentArticles(int playerId)
        {
            string cacheId = "GetPlayerRecentArticles" + playerId;
            if (CacheItemExists(cacheId))
                return (List<Article>)GetCacheItem(cacheId);

            var articles = new List<Article>();
            foreach (var article in GetRecentArticles(12))
                foreach (var articlePlayer in article.ArticlePlayers)
                    if (articlePlayer.PlayerId == playerId)
                    {
                        articles.Add(article);
                        break;
                    }

            AddCacheItem(cacheId, articles);

            return articles;
        }

        public List<Article> GetGameArticles(Game game)
        {
            string cacheId = "GetGameArticles" + game.Id.ToString();
            if (CacheItemExists(cacheId))
                return (List<Article>)GetCacheItem(cacheId);

            var articles = new List<Article>();
            foreach (var article in GetRecentArticles(24 * 7))
            {
                bool found = false;
                foreach (var articleGame in article.ArticleGames)
                    if (articleGame.GameId == game.Id)
                    {
                        articles.Add(article);
                        found = true;
                    }
                if (found)
                    continue;
            }

            AddCacheItem(cacheId, articles);

            return articles;
        }

        public CategoriesString GetCategoriesString(string categoriesCode)
        {
            string cacheId = "GetCategoriesString" + categoriesCode;
            if (CacheItemExists(cacheId))
                return (CategoriesString)GetCacheItem(cacheId);

            CategoriesString outString = null;
            if (categoriesCode.Length > 0)
            {
                outString = (from c in db.CategoriesStrings where c.Code == categoriesCode select c).FirstOrDefault();
                if (outString == null)
                {
                    outString = new CategoriesString();
                    outString.Code = categoriesCode;
                    db.CategoriesStrings.Add(outString);
                    db.SaveChanges();
                }
            }

            AddCacheItem(cacheId, outString);

            return outString;
        }

        public CategoriesString GetCategoriesString(int categoriesStringId)
        {
            string cacheId = "GetCategoriesString" + categoriesStringId.ToString();
            if (CacheItemExists(cacheId))
                return (CategoriesString)GetCacheItem(cacheId);

            var catString = (from cs in db.CategoriesStrings where cs.Id == categoriesStringId select cs).FirstOrDefault();

            AddCacheItem(cacheId, catString);

            return catString;
        }

        public List<GameScoringAlert> GetGameScoringAlerts(Season season, DateTime startDate, DateTime endDate)
        {
            var scoringCategories = (from s in db.GameScoringAlerts
                                     .Include(i => i.Game)
                                     .Include(i => i.Player).ThenInclude(i2 => i2.PlayerDefaultPositions)
                                     .Include(i => i.Team)
                                     .Include(i => i.Category)
                                     where !s.Game.IsFinished && s.Game.GameDate >= startDate && s.Game.GameDate <= endDate
                                     orderby s.ScoringDate descending, s.Category.DisplayOrder ascending
                                     select s).ToList();

            return scoringCategories;
        }

        public void UpdatePlayerGamePositionCategories(Game game, List<PlayerGamePositionCategory> playerGamePositionCategories)
        {
            if (!Sport.IsNBA)
                return;

            var measureCategory = GetMeasureCategory(GetDefaultPlayerType().Id);

            foreach (var playerGamePositionCategory in (from pgp in db.PlayerGamePositionCategories where pgp.GameId == game.Id select pgp))
                db.Remove(playerGamePositionCategory);
            db.SaveChanges();

            foreach (var playerGamePositionCategory in playerGamePositionCategories)
                db.Add(playerGamePositionCategory);
            db.SaveChanges();
        }

        public List<PlayerPositionPercent> GetPlayerPositionPercents(Season season, DateTime startDate, DateTime endDate, int gameId = 0)
        {
            string cacheId = "GetPlayerPositionPercents";
            if (gameId != 0)
                cacheId += ":G" + gameId.ToString();
            else
            {
                cacheId += ":S" + season.Id.ToString();
                cacheId += ":SD" + startDate.ToShortDateString();
                cacheId += ":ED" + endDate.ToShortDateString();
            }
            if (CacheItemExists(cacheId))
                return (List<PlayerPositionPercent>)GetCacheItem(cacheId);

            var positions = GetPositions();
            List<PlayerPositionPercent> playerPositionPercents = new List<PlayerPositionPercent>();

            var q = (from pc in db.PlayerGamePositionCategories.AsNoTracking()
                     .Include(pc => pc.Game)
                     join g in db.Games.AsNoTracking() on pc.GameId equals g.Id
                     where g.SeasonId == season.Id && g.GameDate >= startDate && g.GameDate <= endDate
                     group pc by new { pc.PlayerId, pc.PositionId } into groupResult
                     select new
                     {
                         PlayerId = groupResult.Key.PlayerId,
                         PositionId = groupResult.Key.PositionId,
                         CategoryValue = groupResult.Sum(f => f.CategoryValue)
                     }
                     ).ToList();

            var players = new List<Player>();
            foreach (var item in q)
            {
                var playerPositionPercent = new PlayerPositionPercent();
                playerPositionPercent.Player = GetPlayer(item.PlayerId);
                playerPositionPercent.Position = (from p in positions where p.Id == item.PositionId select p).FirstOrDefault();
                playerPositionPercent.CategoryValue = item.CategoryValue;
                playerPositionPercents.Add(playerPositionPercent);
                if ((from p in players where p.Id == item.PlayerId select p).FirstOrDefault() == null)
                    players.Add(playerPositionPercent.Player);
            }

            foreach (var player in players)
            {
                double valueSum = playerPositionPercents.Where(pp => pp.Player.Id == player.Id).Sum(s => s.CategoryValue);
                if (valueSum > 0)
                {
                    foreach (var playerPositionPercent in (from pp in playerPositionPercents where pp.Player.Id == player.Id select pp))
                    {
                        playerPositionPercent.Percent = Math.Round(playerPositionPercent.CategoryValue / valueSum * 100, 2);
                        playerPositionPercent.PercentColorCode = colorLib.GetYellowRangeColorStyle((double)playerPositionPercent.DisplayPercent, 0, 100, true);
                    }
                }
            }

            AddCacheItem(cacheId, playerPositionPercents);

            return playerPositionPercents;
        }

        public List<DisplayColumn> GetDisplayColumns(string userId)
        {
            if (userId == null)
                return new List<DisplayColumn>();

            var displayColumns = new List<DisplayColumn>();

            var columns = (from c in db.UserOptionTypes where c.OptionGroup == "DisplayColumn" where c.IsEnabled orderby c.DisplayOrder select c).ToList();
            var userSettings = (from u in db.UserOptions where u.UserId == userId select u).ToList();

            foreach (var column in columns)
            {
                var displayColumn = new DisplayColumn();
                displayColumn.UserOptionType = column;
                var userSetting = (from u in userSettings where u.UserOptionTypeId == column.Id select u).FirstOrDefault();
                if (userSetting != null)
                    displayColumn.IsSelected = userSetting.ValueBool.GetValueOrDefault(false);
                else
                    displayColumn.IsSelected = column.DefaultValueBool.GetValueOrDefault(false);
                displayColumns.Add(displayColumn);
            }

            return displayColumns;
        }

        public async Task<UserDisplayColumns> GetUserDisplayColumns(string userId)
        {
            var userDisplayColumns = new UserDisplayColumns();
            foreach (var displayColumn in (from dc in await GetDisplayColumnsAsync(userId) select dc))
                userDisplayColumns.DisplayColumns.Add(displayColumn);

            return userDisplayColumns;
        }

        public List<DisplayColumn> UpdateDisplayColumns(string userId, List<DisplayColumn> displayColumns)
        {
            // delete current
            var userOptions = (from u in db.UserOptions.Include(i => i.UserOptionType) where u.UserId == userId && u.UserOptionType.OptionGroup == "DisplayColumn" select u);
            foreach (var userOption in userOptions)
                db.Remove(userOption);
            db.SaveChanges();

            foreach (var displayColumn in displayColumns)
            {
                var userOption = new UserOption();
                userOption.UserId = userId;
                userOption.UserOptionTypeId = displayColumn.UserOptionType.Id;
                userOption.ValueBool = displayColumn.IsSelected;
                db.Add(userOption);
            }
            db.SaveChanges();

            return displayColumns;
        }

        public async Task<List<ProjectionPlayer>> GetProjectionPlayers(PlayerType playerType, Season season, DateTime pastStartDate, DateTime pastEndDate, DateTime projectedStartDate, DateTime projectedEndDate, List<CategorySetting> categorySettings, string scoringSystem, PerValue perValue, int leagueSize)
        {
            var projectionPlayers = new List<ProjectionPlayer>();
            var categories = GetCategories(playerType);
            var perGamePerValue = GetPerGamePerValue(playerType.Id);
            var perValues = GetPerValues(playerType.Id);
            var playerStatuses = await GetActivePlayerStatusesAsync();

            List<StatPlayer> statPlayers = GetStatPlayers(playerType, season.Id, pastStartDate, pastEndDate, true, null, true);
            var games = GetGames(season, projectedStartDate, projectedEndDate);

            var calcStatPlayers = new List<StatPlayer>();
            foreach (var statPlayer in statPlayers)
            {
                var seasonPlayer = GetSeasonPlayer(statPlayer.Player.Id);
                if (seasonPlayer != null)
                {
                    var teamGames = (from g in games where g.IncludesTeam(seasonPlayer.Team.Id) select g).ToList();
                    if (teamGames.Count > 0)
                    {
                        var playerStatus = (from ps in playerStatuses where ps.PlayerId == seasonPlayer.PlayerId && ps.EstimatedReturnDate != null select ps).FirstOrDefault();
                        if (playerStatus != null)
                            teamGames = (from game in teamGames where game.GameDate >= playerStatus.EstimatedReturnDate select game).ToList();
                        double g = (double)teamGames.Count;

                        foreach (var cat in categories)
                        {
                            if (cat.SourceField != null && cat.SourceField.Length > 0)
                            {
                                double total = statPlayer.Get(perGamePerValue, cat.Id, 0);
                                total *= g;
                                statPlayer.Set(cat.Id, total);
                            }
                        }
                        statPlayer.FillCalculated(Sport, categories, playerType);
                        statPlayer.FillPerValueStats(perValues, categories);
                        calcStatPlayers.Add(statPlayer);
                    }
                }
            }

            var outValueAverages = new ValueAverages();
            var valuePlayers = valuePlayerLib.GetValuePlayers(calcStatPlayers, categorySettings, GetGamesCategory(playerType.Id), scoringSystem, perValue, playerType, GetDisplayCategories(), leagueSize, out outValueAverages);

            foreach (var vp in valuePlayers)
            {
                var projectionPlayer = new ProjectionPlayer();
                projectionPlayer.ValuePlayer = vp;
                projectionPlayer.SeasonPlayer = GetSeasonPlayer(vp.Player.Id);
                if (projectionPlayer.SeasonPlayer != null)
                    projectionPlayers.Add(projectionPlayer);
            }

            var playerPositionPercents = GetPlayerPositionPercents(season, season.StartDate, season.EndDate);
            var measureCat = GetMeasureCategory(playerType.Id);
            var actualPositions = await GetActualPositionsAsync(playerType);

            if (actualPositions.Count > 0)
            {
                foreach (var seasonTeam in season.SeasonTeams)
                {
                    var totalPositionMinutes = new Dictionary<int, double>();
                    foreach (var position in actualPositions)
                        totalPositionMinutes[position.Id] = 0;

                    var teamProjPlayers = (from pp in projectionPlayers where pp.SeasonPlayer.TeamId == seasonTeam.TeamId select pp).ToList();
                    double teamMinutes = 0;
                    foreach (var teamProjPlayer in teamProjPlayers)
                    {
                        var playerPercents = (from pp in playerPositionPercents where pp.Player.Id == teamProjPlayer.SeasonPlayer.PlayerId select pp).ToList();
                        double playerMinutes = teamProjPlayer.ValuePlayer.StatPlayer.Get(measureCat.Id);
                        teamMinutes += playerMinutes;
                        foreach (var playerPercent in playerPercents)
                        {
                            var positionMinutes = playerMinutes * (playerPercent.Percent / 100);
                            totalPositionMinutes[playerPercent.Position.Id] = totalPositionMinutes[playerPercent.Position.Id] + positionMinutes;
                        }
                    }

                    if (teamMinutes > 0)
                    {
                        var positionPercentOverAverage = new Dictionary<int, double>();
                        double expectedPositionMinutes = teamMinutes / (double)actualPositions.Count;
                        foreach (var position in await GetActualPositionsAsync(playerType))
                            positionPercentOverAverage[position.Id] = (totalPositionMinutes[position.Id] - expectedPositionMinutes) / expectedPositionMinutes * 100;
                        foreach (var projPlayer in (from pp in projectionPlayers where pp.SeasonPlayer.TeamId == seasonTeam.TeamId select pp))
                        {
                            var playerPercents = (from pp in playerPositionPercents where pp.Player.Id == projPlayer.SeasonPlayer.PlayerId select pp).ToList();
                            double totalWeightedOver = 0;
                            double totalWeight = 0;
                            foreach (var playerPercent in playerPercents)
                            {
                                var percentOver = positionPercentOverAverage[playerPercent.Position.Id];
                                var overPlayerPercent = new PlayerPositionPercentOver();
                                overPlayerPercent.NormalPlayerPositionPercent = playerPercent;
                                overPlayerPercent.Position = playerPercent.Position;
                                overPlayerPercent.Percent = percentOver;
                                if (percentOver >= 0)
                                    overPlayerPercent.PercentColorCode = colorLib.GetGreenRangeColorStyle(percentOver, 0, 100, true);
                                else
                                    overPlayerPercent.PercentColorCode = colorLib.GetRedRangeColorStyle(Math.Abs(percentOver), 0, 100, true);
                                projPlayer.OverPlayerPositionPercents.Add(overPlayerPercent);
                                totalWeight += overPlayerPercent.NormalPlayerPositionPercent.Percent;
                                totalWeightedOver += overPlayerPercent.Percent * overPlayerPercent.NormalPlayerPositionPercent.Percent;
                            }

                            if (totalWeight > 0)
                            {
                                double overPercent = totalWeightedOver / totalWeight;
                                projPlayer.EstimatedUpside = -1 * overPercent;
                                if (projPlayer.EstimatedUpside >= 0)
                                    projPlayer.EstimatedUpsideColorCode = colorLib.GetGreenRangeColorStyle(projPlayer.EstimatedUpside, 0, 100, true);
                                else
                                    projPlayer.EstimatedUpsideColorCode = colorLib.GetRedRangeColorStyle(Math.Abs(projPlayer.EstimatedUpside), 0, 100, true);
                            }
                        }
                    }
                }
            }

            return projectionPlayers;
        }

        public List<Helper> GetHelpers()
        {
            string cacheId = "GetHelpers";
            if (CacheItemExists(cacheId))
                return (List<Helper>)GetCacheItem(cacheId);

            var helpers = (from helper in db.Helpers where !helper.IsDisabled orderby helper.DisplayOrder select helper).ToList();

            AddCacheItem(cacheId, helpers);

            return helpers;
        }

        public Helper GetHelper(int helperId)
        {
            var helper = (from h in GetHelpers() where h.Id == helperId select h).FirstOrDefault();

            return helper;
        }

        public void ClearLogItems()
        {
            using (var command = db.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = "DELETE _Logs";
                db.Database.OpenConnection();
                command.ExecuteNonQuery();
            }
            ClearCache();
        }

        public List<LogItem> GetLogItems(string filterLevel)
        {
            var logItems = new List<LogItem>();

            using (var command = db.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = "SELECT Id, Message, MessageTemplate, [Level], TimeStamp, Exception, Properties From _Logs ORDER BY TimeStamp DESC";
                db.Database.OpenConnection();
                using (DbDataReader result = command.ExecuteReader())
                {
                    foreach (DbDataRecord row in result)
                    {
                        if (filterLevel != null && filterLevel.Length > 0 && filterLevel != (string)row["Level"])
                            continue;

                        var logItem = new LogItem();
                        logItem.Id = (int)row["Id"];
                        logItem.Level = row["Level"] != DBNull.Value ? (string)row["Level"] : "";
                        logItem.Message = row["Message"] != DBNull.Value ? (string)row["Message"] : "";
                        logItem.MessageTemplate = row["MessageTemplate"] != DBNull.Value ? (string)row["MessageTemplate"] : "";
                        logItem.TimeStamp = (DateTime)row["TimeStamp"];
                        logItem.Exception = row["Exception"] != DBNull.Value ? (string)row["Exception"] : "";
                        logItem.Properties = row["Properties"] != DBNull.Value ? (string)row["Properties"] : "";
                        logItems.Add(logItem);
                    }
                }
            }

            return logItems;
        }

        public ISportDbLib GetSportDbLib()
        {
            if (Sport.Title == "NBA")
                return new NBADbLib(db);

            if (Sport.Title == "NHL")
                return new NHLDbLib(db);

            if (Sport.Title == "MLB")
                return new MLBDbLib(db);

            if (Sport.Title == "NFL")
                return new NFLDbLib(db);

            throw new Exception("Must add support for " + Sport.Title + "DbLib");
        }

    }
}
