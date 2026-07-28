using System;
using System.Collections.Generic;
using System.Text;

namespace RotoMonster.Core.Libs
{
    public class ColorLib
    {
        private string White
        {
            get
            {
                return "ffffff";
            }
        }

        private string Gray
        {
            get
            {
                return "d9d9d9";
            }
        }

        public string OwnershipGetPercentColor(double percent)
        {
            double adjustedPercent = Math.Min(100, Math.Max(0, percent));
            int otherColor = Convert.ToInt32(Math.Round(255 - adjustedPercent, 0));
            var c = System.Drawing.Color.FromArgb(otherColor, 255, 255);

            return c.Name.Substring(2, 6);
        }

        public string OwnershipPercentChangeColor(double percent)
        {
            if (percent >= 0)
            {
                return GetGreenRangeColorStyle(percent * 4, 0, 100, true);
            }
            else
            {
                return GetRedRangeColorStyle(percent * -4, 0, 100, true);
            }
        }

        public string ActiveGetPercentColor(double percent)
        {
            return GetYellowRangeColorStyle(percent, 0, 100, true);
        }

        public string GetPointsColor(Sport sport, double points)
        {
            double low = 0;
            double high = 0;

            if (sport.IsNFL)
            {
                low = 15;
                high = 35;
            }
            else if (sport.IsNBA)
            {
                low = 100;
                high = 130;
            }
            else if (sport.IsNHL)
            {
                low = 1;
                high = 5;
            }
            else if (sport.IsMLB)
            {
                low = 1;
                high = 7;
            }

            if (points == 0)
                return White;
            else
            {
                double percent = (points - low) / (high - low) * 100;

                return GetYellowRangeColorStyle(percent, 0, 100, true);
            }

        }

        public string GetStatColor(double percent)
        {
            double adjustedPercent = Math.Min(100, Math.Max(0, percent));
            int otherColor = Convert.ToInt32(Math.Round(255 - adjustedPercent, 0));
            var c = System.Drawing.Color.FromArgb(255, 255, otherColor);

            return c.Name.Substring(2, 6);
        }

        public string GetGreenRangeColorStyle(double value, double low, double high, bool colorHigh, string appendStyle = "")
        {
            int otherColor = 0;
            double adjustedValue = Math.Min(high, value);
            adjustedValue = Math.Max(low, adjustedValue);
            double percent = (adjustedValue - low) / (high - low) * 100;
            if (!colorHigh)
                percent = 100 - percent;
            otherColor = Convert.ToInt32(Math.Round(255 - percent / 200 * 255, 0));
            System.Drawing.Color c = System.Drawing.Color.FromArgb(otherColor, 255, otherColor);

            return c.Name.Substring(2, 6);
        }

        public string GetRedRangeColorStyle(double value, double low, double high, bool colorHigh, string appendStyle = "")
        {
            int otherColor = 0;
            double adjustedValue = Math.Min(high, value);
            adjustedValue = Math.Max(low, adjustedValue);
            double percent = (adjustedValue - low) / (high - low) * 100;
            if (!colorHigh)
                percent = 100 - percent;
            otherColor = Convert.ToInt32(Math.Round(255 - percent / 200 * 255, 0));
            System.Drawing.Color c = System.Drawing.Color.FromArgb(255, otherColor, otherColor);

            return c.Name.Substring(2, 6);
        }

        public string GetYellowRangeColorStyle(double value, double low, double high, bool colorHigh, string appendStyle = "")
        {
            int otherColor = 0;
            double adjustedValue = Math.Min(high, value);
            adjustedValue = Math.Max(low, adjustedValue);
            double percent = (adjustedValue - low) / (high - low) * 100;
            if (!colorHigh)
                percent = 100 - percent;
            otherColor = Convert.ToInt32(Math.Round(255 - percent / 200 * 255, 0));
            System.Drawing.Color c = System.Drawing.Color.FromArgb(255, 255, otherColor);

            return c.Name.Substring(2, 6);
        }

        public string GetGamePercentColorStyle(int percent)
        {
            double adjustedPercent = Math.Min(100, Math.Max(0, (double)percent));
            int otherColor = Convert.ToInt32(Math.Round(255 - adjustedPercent * 0.8, 0));
            System.Drawing.Color c = System.Drawing.Color.FromArgb(otherColor, 255, 255);
            string style = c.Name.Substring(2, 6);

            return style;
        }

        public string TimeUntilGameColorStyle(Game game)
        {
            TimeSpan ts = game.TimeUntilGame;

            if (game.IsFinished)
                return Gray;

            if (!game.HasStarted)
            {
                double percent = Math.Min(ts.TotalHours / 6 * 100, 100);

                return GetYellowRangeColorStyle(percent, 0, 100, false);
            }

            return GetGamePercentColorStyle(game.PercentComplete);
        }

        public string TimeSinceInjuryColorStyle(PlayerStatus playerStatus)
        {
            if (playerStatus.TimeSince.TotalDays <= 1)
            {
                double percent = Math.Min(100 - playerStatus.TimeSince.TotalHours / 24 * 100, 100);

                string colorStyle = GetYellowRangeColorStyle(percent, 0, 100, true);
                return colorStyle;
            }

            return White;
        }

        public string GetTimeSpanColor(TimeSpan ts, double maxHours, string appendStyle = "")
        {
            double hours = ts.TotalHours;
            hours = Math.Max(0, Math.Min(hours, maxHours));

            double percent = hours / maxHours * 100;

            int otherColor = 0;
            double adjustedPercent = Math.Min(100, Math.Max(0, percent));

            int yellowRange = 60;
            otherColor = (int)Math.Round(yellowRange * percent / 100, 0);
            var c = System.Drawing.Color.FromArgb(255, 255, (255 - yellowRange) + otherColor);

            return c.Name.Substring(2, 6);
        }

        public string GetPlayerStatusTypeStyle(PlayerStatusType playerStatusType)
        {
            string style = "background-color:#" + playerStatusType.BackgroundColor;
            style += ";color:#" + playerStatusType.TextColor;

            return style;
        }

        public string GetDisplayPlayerOwnCss(DisplayPlayer displayPlayer)
        {
            if (displayPlayer.UserLeagueTeam == null)
                return "";

            string ownCss = "";

            if (displayPlayer.IsMyPlayer)
            {
                if (displayPlayer.IsActive)
                    ownCss = "own-my";
                else
                    ownCss = "own-my-i";
            }
            else
                ownCss = "own";

            return ownCss;
        }

        public string GetPointsColor(double points, Sport sport)
        {
            if (sport.HighPoints == 0 || sport.LowPoints == 0)
                return White;

            return GetYellowRangeColorStyle(points, sport.LowPoints, sport.HighPoints, true);
        }

    }
}
