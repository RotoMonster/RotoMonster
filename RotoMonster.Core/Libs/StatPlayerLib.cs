using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace RotoMonster.Core.Libs
{
    public class StatPlayerLib
    {
        public List<StatPlayer> GetStatPlayersFromTable(
            IEnumerable<dynamic> q,
            List<Player> players,
            List<SeasonPlayer> seasonPlayers,
            List<Team> teams,
            List<Category> categories,
            Category gamesCategory)
        {
            List<StatPlayer> statPlayers = new List<StatPlayer>();

            var fieldCats = (from c in categories where c.SourceField != null select c).ToList();

            foreach (var l in q)
            {
                int playerId = l.GetType().GetProperty("PlayerId").GetValue(l, null);

                var pg = (from p in statPlayers where p.Player.Id == playerId select p).FirstOrDefault();
                var sp = (from p in seasonPlayers where p.PlayerId == playerId select p).FirstOrDefault();
                if (sp == null)
                    continue;

                if (pg == null)
                {
                    pg = new StatPlayer();
                    pg.Player = (from p in players where p.Id == playerId select p).FirstOrDefault();
                    statPlayers.Add(pg);
                }

                int teamId = sp.TeamId;
                pg.Team = (from t1 in teams where t1.Id == teamId select t1).FirstOrDefault();

                if (l.GetType().GetProperty("TeamId") != null)
                {
                    int team2Id = l.GetType().GetProperty("TeamId").GetValue(l, null);
                    pg.Team2 = (from t1 in teams where t1.Id == team2Id select t1).FirstOrDefault();
                }

                pg.Set(gamesCategory.Id, l.GetType().GetProperty("Games").GetValue(l, null));
                pg.Games = l.GetType().GetProperty("Games").GetValue(l, null);
                var t = l.GetType();
                foreach (var c in fieldCats)
                {
                    if (l.GetType().GetProperty(c.SourceField) != null)
                    {
                        double? v = l.GetType().GetProperty(c.SourceField).GetValue(l, null);
                        if (v != null)
                        {
                            double current = pg.Get(c.Id);
                            pg.Set(c.Id, v.GetValueOrDefault());
                        }
                    }
                }
            }

            return statPlayers;
        }

        public List<StatPlayer> GetTeamStatPlayersFromTable(
            IEnumerable<dynamic> q,
            List<Team> teams,
            List<Game> analyzedGames,
            List<Category> categories,
            Category gamesCategory)
        {
            List<StatPlayer> statPlayers = new List<StatPlayer>();

            var fieldCats = (from c in categories where c.SourceField != null select c).ToList();

            foreach (var l in q)
            {
                int teamId = l.GetType().GetProperty("TeamId").GetValue(l, null);
                int activeRosterSpotId = l.GetType().GetProperty("ActiveRosterSpotId").GetValue(l, null);

                var pg = (from p in statPlayers where p.Team.Id == teamId && p.TeamActiveRosterSpotId == activeRosterSpotId select p).FirstOrDefault();

                if (pg == null)
                {
                    pg = new StatPlayer();
                    pg.Team = (from t1 in teams where t1.Id == teamId select t1).FirstOrDefault();
                    pg.TeamActiveRosterSpotId = activeRosterSpotId;
                    pg.Player = new Player();
                    pg.Player.Id = pg.Team.Id;
                    statPlayers.Add(pg);
                }

                var t = l.GetType();
                foreach (var c in fieldCats)
                {
                    if (l.GetType().GetProperty(c.SourceField) != null)
                    {
                        double? v = l.GetType().GetProperty(c.SourceField).GetValue(l, null);
                        if (v != null)
                        {
                            double current = pg.Get(c.Id);
                            pg.Set(c.Id, v.GetValueOrDefault());
                        }
                    }
                }

                int teamGames = (from g in analyzedGames where g.IncludesTeam(teamId) select g).Count();
                pg.Set(gamesCategory.Id, teamGames);
            }

            return statPlayers;
        }

    }
}
