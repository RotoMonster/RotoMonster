using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RotoMonster.Core.Libs
{
    public class GameLib
    {

        public Game GetTeamGame(List<Game> games, DateTime gameDate, Team team, int number = 1)
        {
            var game = (from g in games where g.GameDate == gameDate && g.Number == number && (g.HomeTeam.Id == team.Id || g.AwayTeam.Id == team.Id) select g).FirstOrDefault();

            return game;
        }

    }
}
