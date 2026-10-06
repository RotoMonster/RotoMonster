using RotoMonster.Core.Libs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace RotoMonster.Core
{
    public class Game
    {
        public static readonly (string Id, string Name, string Region)[] DisplayZones =
        {
            ("America/New_York", "Eastern Time", "United States & Canada"),
            ("America/Chicago", "Central Time", "United States & Canada"),
            ("America/Denver", "Mountain Time", "United States & Canada"),
            ("America/Phoenix", "Arizona", "United States & Canada"),
            ("America/Los_Angeles", "Pacific Time", "United States & Canada"),
            ("America/Anchorage", "Alaska", "United States & Canada"),
            ("Pacific/Honolulu", "Hawaii", "United States & Canada"),
            ("America/Halifax", "Atlantic Time (Canada)", "United States & Canada"),
            ("America/St_Johns", "Newfoundland", "United States & Canada"),
            ("America/Regina", "Saskatchewan", "United States & Canada"),
            ("America/Puerto_Rico", "Puerto Rico", "Latin America & Caribbean"),
            ("America/Mexico_City", "Mexico City", "Latin America & Caribbean"),
            ("America/Bogota", "Bogota, Lima", "Latin America & Caribbean"),
            ("America/Caracas", "Caracas", "Latin America & Caribbean"),
            ("America/Santiago", "Santiago", "Latin America & Caribbean"),
            ("America/Argentina/Buenos_Aires", "Buenos Aires", "Latin America & Caribbean"),
            ("America/Sao_Paulo", "Sao Paulo", "Latin America & Caribbean"),
            ("Europe/London", "London, Dublin, Lisbon", "Europe"),
            ("Europe/Paris", "Central Europe - Paris, Berlin, Rome, Madrid", "Europe"),
            ("Europe/Athens", "Eastern Europe - Athens, Helsinki, Kyiv", "Europe"),
            ("Europe/Istanbul", "Istanbul", "Europe"),
            ("Europe/Moscow", "Moscow", "Europe"),
            ("Africa/Lagos", "West Africa - Lagos", "Africa & Middle East"),
            ("Africa/Johannesburg", "South Africa", "Africa & Middle East"),
            ("Africa/Cairo", "Cairo", "Africa & Middle East"),
            ("Africa/Nairobi", "East Africa - Nairobi", "Africa & Middle East"),
            ("Asia/Jerusalem", "Jerusalem", "Africa & Middle East"),
            ("Asia/Riyadh", "Riyadh", "Africa & Middle East"),
            ("Asia/Dubai", "Dubai", "Africa & Middle East"),
            ("Asia/Karachi", "Pakistan", "Asia"),
            ("Asia/Kolkata", "India", "Asia"),
            ("Asia/Dhaka", "Bangladesh", "Asia"),
            ("Asia/Bangkok", "Bangkok, Jakarta", "Asia"),
            ("Asia/Shanghai", "China", "Asia"),
            ("Asia/Hong_Kong", "Hong Kong", "Asia"),
            ("Asia/Singapore", "Singapore", "Asia"),
            ("Asia/Manila", "Philippines", "Asia"),
            ("Asia/Taipei", "Taiwan", "Asia"),
            ("Asia/Tokyo", "Japan", "Asia"),
            ("Asia/Seoul", "Korea", "Asia"),
            ("Australia/Perth", "Perth", "Australia & Pacific"),
            ("Australia/Adelaide", "Adelaide", "Australia & Pacific"),
            ("Australia/Darwin", "Darwin", "Australia & Pacific"),
            ("Australia/Brisbane", "Brisbane", "Australia & Pacific"),
            ("Australia/Sydney", "Sydney, Melbourne", "Australia & Pacific"),
            ("Pacific/Guam", "Guam", "Australia & Pacific"),
            ("Pacific/Auckland", "New Zealand", "Australia & Pacific"),
            ("Etc/UTC", "UTC", "Other")
        };

        private static readonly System.Threading.AsyncLocal<string> _displayZoneId = new System.Threading.AsyncLocal<string>();

        public static string DisplayZoneId
        {
            get
            {
                var id = _displayZoneId.Value;
                return IsDisplayZone(id) ? id : DisplayZones[0].Id;
            }
            set { _displayZoneId.Value = value; }
        }

        public static TimeZoneInfo DisplayZone
        {
            get
            {
                try { return TimeZoneInfo.FindSystemTimeZoneById(DisplayZoneId); }
                catch { return TimeZoneInfo.FindSystemTimeZoneById(DisplayZones[0].Id); }
            }
        }

        public static string DisplayZoneName
        {
            get
            {
                foreach (var z in DisplayZones)
                    if (z.Id == DisplayZoneId) return z.Name;
                return DisplayZones[0].Name;
            }
        }

        public static string DisplayZoneLabel(string id)
        {
            string name = id;
            foreach (var z in DisplayZones)
                if (z.Id == id) { name = z.Name; break; }
            try
            {
                var offset = TimeZoneInfo.FindSystemTimeZoneById(id).GetUtcOffset(DateTime.UtcNow);
                string sign = offset < TimeSpan.Zero ? "-" : "+";
                var abs = offset.Duration();
                return "(UTC" + sign + abs.Hours.ToString("00") + ":" + abs.Minutes.ToString("00") + ") " + name;
            }
            catch
            {
                return name;
            }
        }

        public static bool IsDisplayZone(string id)
        {
            if (string.IsNullOrEmpty(id)) return false;
            foreach (var z in DisplayZones)
                if (z.Id == id) return true;
            return false;
        }

        public int Id { get; set; }
        public int Number { get; set; }
        public DateTime GameDate { get; set; }
        public int SeasonId { get; set; }
        public int? HomeTeamId { get; set; }
        public int? AwayTeamId { get; set; }
        public DateTime GameTime { get; set; }
        public bool IsFinished { get; set; }
        public int PercentComplete { get; set; }
        public int ReprocessCount { get; set; }
        public bool? IsPostponed { get; set; }

        public int Period { get; set; }
        [StringLength(12)]
        public string GameClock { get; set; }

        public double OverUnder { get; set; } = 0;
        public double HomeSpread { get; set; } = 0;
        public int? HomeMoneyLine { get; set; } = 0;
        public int? AwayMoneyLine { get; set; } = 0;

        public Team HomeTeam { get; set; }
        public Team AwayTeam { get; set; }
        public Season Season { get; set; }

        public int HomeScore { get; set; }
        public int AwayScore { get; set; }

        [NotMapped] public double HomeWinPercent { get; set; }
        [NotMapped] public double AwayWinPercent { get; set; }
        [NotMapped] public double HomeProjectedPoints { get; set; }
        [NotMapped] public double AwayProjectedPoints { get; set; }

        [NotMapped] public string AwayStyle { get; set; }
        [NotMapped] public string HomeStyle { get; set; }

        public Game()
        {
        }

        public List<Team> GetTeams()
        {
            var teams = new List<Team>();
            teams.Add(AwayTeam);
            teams.Add(HomeTeam);

            return teams;
        }


        [StringLength(80)]
        public string SportRadarId { get; set; }

        public int GetOpponentId(int teamId)
        {
            if (teamId == HomeTeamId)
                return AwayTeamId.GetValueOrDefault(0);
            else
                return HomeTeamId.GetValueOrDefault(0);
        }

        public Team GetOpponent(Team team)
        {
            if (team.Id == HomeTeam.Id)
                return AwayTeam;
            else
                return HomeTeam;
        }

        public string GetOpponentText(Team team)
        {
            var txt = "";
            if (team.Id == AwayTeam.Id)
                txt += "@";
            txt += GetOpponent(team).Code;

            return txt;
        }

        public string GetTeamStyle(Team team)
        {
            if (team.Id == HomeTeam.Id)
                return HomeStyle;
            else
                return AwayStyle;
        }

        public string GameText
        {
            get
            {
                return AwayTeam.Code + "@" + HomeTeam.Code;
            }
        }

        public bool IncludesTeam(int teamId)
        {
            return (HomeTeam.Id == teamId || AwayTeam.Id == teamId);
        }

        public string DayOfWeekString
        {
            get
            {
                return GameDate.DayOfWeek.ToString().Substring(0, 3);
            }
        }

        public DateTime EasternTimeNow
        {
            get
            {
                TimeZoneInfo easternTimeZone = DisplayZone;

                DateTime easternDateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, easternTimeZone);

                return easternDateTime;
            }
        }

        public TimeSpan TimeUntilGame
        {
            get
            {
                DateTime easternDateTime = EasternTimeNow;

                if (EasternTime < easternDateTime)
                    return new TimeSpan();

                return EasternTime - easternDateTime;
            }
        }

        public TimeSpan TimeSinceStart
        {
            get
            {
                if (DateTime.UtcNow < GameTime)
                    return new TimeSpan();

                return DateTime.UtcNow - GameTime;
            }
        }

        public bool HasStarted
        {
            get
            {
                return (DateTime.UtcNow > GameTime);
            }
        }

        public DateTime EasternTime
        {
            get
            {
                TimeZoneInfo easternTimeZone = DisplayZone;
                DateTime easternDateTime = TimeZoneInfo.ConvertTimeFromUtc(GameTime, easternTimeZone);

                return easternDateTime;
            }
        }

        public string GameTimeText
        {
            get
            {
                string o = ""; // GameDate.DayOfWeek.ToString().Substring(0, 3);
                //o += " " + GameDate.Month.ToString() + "/" + GameDate.Day.ToString();

                if (PercentComplete == 0)
                {
                    DateTime est = EasternTime;
                    if (est.Hour > 12)
                        o += (est.Hour - 12).ToString();
                    else
                        o += est.Hour.ToString();
                    o += ":" + String.Format("{0:00}", est.Minute);
                }

                TimeSpan ts = TimeUntilGame;
                if (ts.TotalMinutes > 0)
                {
                    if (ts.TotalHours < 1)
                        o += " in " + String.Format("{0:#0}m", ts.TotalMinutes);
                    else if (ts.TotalHours < 24)
                        o += " in " + String.Format("{0:#0.0}h", ts.TotalHours);
                    else if (ts.TotalDays < 3)
                        o += " in " + String.Format("{0:0.0}d", ts.TotalDays);
                }

                if (IsFinished)
                {
                    o += " F";
                }
                else if (HasStarted && !IsFinished && PercentComplete > 0)
                {
                    o += " " + PercentComplete.ToString() + "%";
                }

                return o;
            }
        }

        public double GetPercentComplete(int period, string clock, Sport sport)
        {
            double percent = 0;

            double minutes = (Convert.ToDouble(period) - 1) * (double)sport.MinutesPerPeriod;
            string[] clockItems = clock.Split(":");
            if (clockItems.Length == 2)
            {
                double m = Convert.ToDouble(clockItems[0]);
                double s = Convert.ToDouble(clockItems[1]);
                double periodMinutes = (double)sport.MinutesPerPeriod - (m + s / 60);
                minutes += periodMinutes;
                percent = minutes / ((double)sport.MinutesPerPeriod * (double)sport.PeriodsPerGame) * 100;
                percent = Math.Round(Math.Min(100, percent), 0);
            }

            return percent;
        }


        public void FillProperties(Sport sport, ColorLib colorLib)
        {
            if (sport.IsNHL || sport.IsMLB)
                MoneyLineToWinsAndPoints();

            if (HasStarted)
            {
                double diff = Math.Abs((double)HomeScore - (double)AwayScore);

                if (diff != 0)
                {
                    int range = Convert.ToInt16(Math.Round(((double)sport.HighPoints - (double)sport.LowPoints), 0));
                    string greenColor = colorLib.GetGreenRangeColorStyle(diff, 0, range, true);
                    string redColor = colorLib.GetRedRangeColorStyle(diff, 0, range, true);
                    string homeColor = (HomeScore > AwayScore) ? greenColor : redColor;
                    string awayColor = (AwayScore > HomeScore) ? greenColor : redColor;

                    string winStyle = (IsFinished) ? "border: solid 1px green;" : "";
                    string lossStyle = (IsFinished) ? "margin: 1px 0 1px 0;" : "";

                    HomeStyle = "background-color:#" + homeColor + ";" + ((HomeScore > AwayScore) ? winStyle : lossStyle);
                    AwayStyle = "background-color:#" + awayColor + ";" + ((AwayScore > HomeScore) ? winStyle : lossStyle);
                }
            }

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

            int repetitions = 0;
            while (repetitions < 100)
            {
                double calcWins = 1 / ((1 + Math.Pow(loseRuns / winRuns, 1.85)));
                if (calcWins > Math.Max(HomeWinPercent, AwayWinPercent))
                    break;
                winRuns += 0.05;
                loseRuns -= 0.05;
                repetitions++;
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
                    if (HomeTeam.Id == team.Id)
                        points = (OverUnder / 2 - HomeSpread / 2);
                    else
                        points = (OverUnder / 2 + HomeSpread / 2);
                }

                if (sport.IsNHL || sport.IsMLB)
                {
                    if (team.Id == HomeTeamId)
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
                    if (AwayTeam.Id == team.Id)
                        return (OverUnder / 2 - HomeSpread / 2);
                    else
                        return (OverUnder / 2 + HomeSpread / 2);
                }

                if (sport.IsNHL || sport.IsMLB)
                {
                    if (AwayTeamId == team.Id)
                        return ProjectedPoints(sport, HomeTeam);
                    else
                        return ProjectedPoints(sport, AwayTeam);
                }
            }

            return 0;
        }

        public int GetTeamScore(Team team)
        {
            if (team.Id == HomeTeamId)
                return HomeScore;
            else if (team.Id == AwayTeamId)
                return AwayScore;
            else
                return 0;
        }

        public int GetOpponentTeamScore(Team team)
        {
            if (team.Id == AwayTeamId)
                return HomeScore;
            else if (team.Id == HomeTeamId)
                return AwayScore;
            else
                return 0;
        }

    }
}
