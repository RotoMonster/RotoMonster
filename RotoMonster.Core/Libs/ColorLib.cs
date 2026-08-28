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
            return "own-cell";
        }

        public OwnBadgeState GetOwnBadgeState(DisplayPlayer displayPlayer)
        {
            if (displayPlayer.UserLeagueTeam == null)
                return displayPlayer.IsWaiver ? OwnBadgeState.Waiver : OwnBadgeState.None;

            if (displayPlayer.IsMyPlayer)
            {
                if (displayPlayer.IsIR)
                    return OwnBadgeState.IR;

                if (displayPlayer.IsActive)
                    return OwnBadgeState.Starting;

                return OwnBadgeState.Bench;
            }

            if (displayPlayer.IsIR)
                return OwnBadgeState.OtherIR;

            if (displayPlayer.IsActive)
                return OwnBadgeState.OtherStarting;

            return OwnBadgeState.OtherBench;
        }

        public bool IsOwnBadgeStarting(OwnBadgeState state)
        {
            return state == OwnBadgeState.Starting || state == OwnBadgeState.OtherStarting;
        }

        public bool IsOwnBadgeBench(OwnBadgeState state)
        {
            return state == OwnBadgeState.Bench || state == OwnBadgeState.OtherBench;
        }

        public bool IsOwnBadgeIR(OwnBadgeState state)
        {
            return state == OwnBadgeState.IR || state == OwnBadgeState.OtherIR;
        }

        public string GetOwnBadgeCss(OwnBadgeState state)
        {
            switch (state)
            {
                case OwnBadgeState.Starting:
                    return "own-badge own-badge--starting";
                case OwnBadgeState.Bench:
                    return "own-badge own-badge--bench";
                case OwnBadgeState.IR:
                    return "own-badge own-badge--ir";
                case OwnBadgeState.OtherStarting:
                    return "own-badge own-badge--other-starting";
                case OwnBadgeState.OtherBench:
                    return "own-badge own-badge--other-bench";
                case OwnBadgeState.OtherIR:
                    return "own-badge own-badge--other-ir";
                case OwnBadgeState.Waiver:
                    return "own-badge own-badge--waiver";
                default:
                    return "";
            }
        }

        public string GetOwnBadgeTitle(DisplayPlayer displayPlayer, OwnBadgeState state)
        {
            string title = GetOwnBadgeTitle(state);

            // The badge only has room for a number, so the date goes here.
            if (state == OwnBadgeState.Waiver)
            {
                if (displayPlayer == null || !displayPlayer.WaiverDate.HasValue)
                    return title;

                return title + " until " + displayPlayer.WaiverDate.Value.ToString("MMM d");
            }

            if (state != OwnBadgeState.OtherStarting
                && state != OwnBadgeState.OtherBench
                && state != OwnBadgeState.OtherIR)
                return title;

            if (displayPlayer.UserLeagueTeam == null)
                return title;

            string teamTitle = displayPlayer.UserLeagueTeam.Title;

            if (string.IsNullOrWhiteSpace(teamTitle))
                return title;

            return title + " - " + teamTitle.Trim();
        }

        /// <summary>
        /// What the WW badge reads. Plain WW when they clear today or tomorrow,
        /// or when we have no date at all, and WW2 upwards when it is further
        /// out. Ken's shape: "WW by default and WW2, 3, etc. if more than 1."
        /// </summary>
        public string GetWaiverBadgeText(DisplayPlayer displayPlayer)
        {
            if (displayPlayer == null || !displayPlayer.WaiverDate.HasValue)
                return "WW";

            var days = (int)Math.Ceiling(
                (displayPlayer.WaiverDate.Value.Date - DateTime.Today).TotalDays);

            return days > 1 ? "WW" + days.ToString() : "WW";
        }

        public string GetOwnBadgeTitle(OwnBadgeState state)
        {
            switch (state)
            {
                case OwnBadgeState.Starting:
                case OwnBadgeState.OtherStarting:
                    return "Active";
                case OwnBadgeState.Bench:
                case OwnBadgeState.OtherBench:
                    return "Bench";
                case OwnBadgeState.IR:
                case OwnBadgeState.OtherIR:
                    return "Injured Reserve";
                case OwnBadgeState.Waiver:
                    return "On Waiver Wire";
                default:
                    return "";
            }
        }

        public string GetPointsColor(double points, Sport sport)
        {
            if (sport.HighPoints == 0 || sport.LowPoints == 0)
                return White;

            return GetYellowRangeColorStyle(points, sport.LowPoints, sport.HighPoints, true);
        }

    }
}
