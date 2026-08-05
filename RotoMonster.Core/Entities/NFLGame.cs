using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace RotoMonster.Core
{
    public class NFLGame
    {
        public int Id { get; set; }
        public int GameId { get; set; }
        public double OverUnder { get; set; } = 0;
        public double HomeSpread { get; set; } = 0;
        public int? HomeMoneyLine { get; set; } = 0;
        public int? AwayMoneyLine { get; set; } = 0;

        public Game Game { get; set; }

        [NotMapped] public double HomeWinPercent { get; set; }
        [NotMapped] public double AwayWinPercent { get; set; }
        [NotMapped] public double HomeProjectedPoints { get; set; }
        [NotMapped] public double AwayProjectedPoints { get; set; }

        public NFLGame()
        {
            MoneyLineToWinsAndPoints();
        }

        public void MoneyLineToWinsAndPoints()
        {
            if (OverUnder == 0)
                return;

            double tmpHomeMoneyLine = -1;
            if (HomeMoneyLine == 0)
                tmpHomeMoneyLine = 100;
            else
                tmpHomeMoneyLine = Convert.ToDouble(HomeMoneyLine);

            double tmpAwayMoneyLine = -1;
            if (AwayMoneyLine == 0)
                tmpAwayMoneyLine = 100;
            else
                tmpAwayMoneyLine = Convert.ToDouble(AwayMoneyLine);

            HomeWinPercent = 0;
            AwayWinPercent = 0;

            if (tmpHomeMoneyLine != -1 && tmpAwayMoneyLine != -1)
            {
                if (tmpHomeMoneyLine >= 100)
                {
                    HomeWinPercent = 100d / (Convert.ToDouble(Math.Abs(tmpHomeMoneyLine) + 100d));
                    AwayWinPercent = Convert.ToDouble(Math.Abs(tmpAwayMoneyLine)) / Convert.ToDouble(Math.Abs(tmpAwayMoneyLine) + 100d);
                }
                else
                {
                    AwayWinPercent = 100d / (Convert.ToDouble(Math.Abs(tmpAwayMoneyLine) + 100d));
                    HomeWinPercent = Convert.ToDouble(Math.Abs(tmpHomeMoneyLine)) / (Convert.ToDouble(Math.Abs(tmpHomeMoneyLine) + 100d));
                }
            }

            double totalWinPercent = HomeWinPercent + AwayWinPercent;
            if (totalWinPercent > 0)
            {
                HomeWinPercent = (HomeWinPercent / totalWinPercent);
                AwayWinPercent = (AwayWinPercent / totalWinPercent);
            }

            // Points/Runs
            double winRuns = Math.Abs(OverUnder / 2);
            double loseRuns = Math.Abs(OverUnder / 2);

            double calcWins = 0;
            while (true)
            {
                calcWins = 1 / ((1 + Math.Pow(loseRuns / winRuns, 1.85)));
                if (calcWins > Math.Max(HomeWinPercent,AwayWinPercent))
                    break;
                winRuns += 0.05;
                loseRuns -= 0.05;
            }

            if (HomeWinPercent > AwayWinPercent)
            {
                HomeProjectedPoints = winRuns;
                AwayProjectedPoints = loseRuns;
            }
            else
            {
                HomeProjectedPoints = loseRuns;
                AwayProjectedPoints = winRuns;
            }

            HomeProjectedPoints = Math.Round(HomeProjectedPoints, 1);
            AwayProjectedPoints = Math.Round(AwayProjectedPoints, 1);
        }

        public double WinPercent(int moneyLine)
        {
            if (moneyLine >= 100)
            {
                double winPercent = 100 / (Convert.ToDouble(moneyLine) + 100);
                winPercent = Math.Round(winPercent, 2);

                return winPercent;
            }

            else if (moneyLine < 0)
            {
                double winPercent = Convert.ToDouble(Math.Abs(moneyLine)) / (Convert.ToDouble(Math.Abs(moneyLine)) + 100);
                winPercent = Math.Round(winPercent, 2);

                return winPercent;
            }

            return 0;
        }

        public double ProjectedPoints(Sport sport, Team team)
        {
            double points = 0;
            if (OverUnder > 0)
            {
                if (sport.IsNBA || sport.IsNFL)
                {
                    if (Game.HomeTeam.Id == team.Id)
                        points = (OverUnder / 2 - HomeSpread / 2);
                    else
                        points = (OverUnder / 2 + HomeSpread / 2);
                }

                if (sport.IsNHL || sport.IsMLB)
                {
                    if (team.Id == Game.HomeTeamId)
                        return HomeProjectedPoints;
                    else
                        return AwayProjectedPoints;
                }
            }

            return points;
        }

        public double OpponentProjectedPoints(Sport sport, Team team)
        {
            if (OverUnder > 0)
            {
                if (sport.IsNBA || sport.IsNFL)
                {
                    if (Game.AwayTeam.Id == team.Id)
                        return (OverUnder / 2 - HomeSpread / 2);
                    else
                        return (OverUnder / 2 + HomeSpread / 2);
                }

                if (sport.IsNHL || sport.IsMLB)
                {
                    if (Game.AwayTeamId == team.Id)
                        return ProjectedPoints(sport, Game.HomeTeam);
                    else
                        return ProjectedPoints(sport, Game.AwayTeam);
                }
            }

            return 0;
        }

    }
}
