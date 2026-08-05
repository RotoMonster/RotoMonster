using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;


namespace RotoMonster.Core
{
    public class ValuePlayer
    {
        public StatPlayer StatPlayer { get; set; }
        public int Rank { get; set; }
        public double LeagueValue { get; set; }
        public double PuntValue { get; set; }
        public double ExpectedTeamLeagueValue { get; set; } = 0; // used by Team Ease
        public double OpponentValueBoost { get; set; } = 0;

        public string LeagueValueColor { get; set; }
        public string PuntValueColor { get; set; }

        private Dictionary<int, double> _catValues = new Dictionary<int, double>();
        private Dictionary<int, string> _catColorValues = new Dictionary<int, string>();
        private Dictionary<int, double> _statColorValues = new Dictionary<int, double>();

        public ValuePlayer()
        {
            LeagueValue = 0;
            PuntValue = 0;
        }

        public Player Player
        {
            get
            {
                return StatPlayer.Player;
            }
        }

        public void Set(int categoryId, double value)
        {
            _catValues[categoryId] = value;
        }

        public double Get(int categoryId, double defaultValue = 0)
        {
            if (_catValues.ContainsKey(categoryId))
                return Convert.ToDouble(_catValues[categoryId]);

            return defaultValue;
        }

        public void SetC(int categoryId, string value)
        {
            _catColorValues[categoryId] = value;
        }

        public string GetC(int categoryId, string defaultValue = "ffffff")
        {
            if (_catColorValues.ContainsKey(categoryId))
                return (string)(_catColorValues[categoryId]);

            return defaultValue;
        }

        public void SetStatColor(int categoryId, double value)
        {
            _statColorValues[categoryId] = value;
        }

        public double GetStatColor(int categoryId, double defaultValue = 0)
        {
            if (_statColorValues.ContainsKey(categoryId))
                return (double)(_statColorValues[categoryId]);

            return defaultValue;
        }

        public void FillTotalValuesAndColors(List<CategorySetting> categorySettings)
        {
            LeagueValue = 0;
            PuntValue = 0;
            var activeCategorySettings = (from c in categorySettings where c.IsActive select c).ToList();
            foreach (var cs in categorySettings)
            {
                double val = Get(cs.Category.Id);
                LeagueValue += val;
                if (cs.IsActive)
                    PuntValue += val;
                SetC(cs.Category.Id, GetValueColor(val));
            }
            if (categorySettings.Count > 0)
                LeagueValue /= (double)categorySettings.Count;
            if (activeCategorySettings.Count > 0)
                PuntValue /= (double)activeCategorySettings.Count;
            LeagueValueColor = GetValueColor(LeagueValue);
            PuntValueColor = GetValueColor(PuntValue);
        }

        public string GetValueColor(double value)
        {
            int otherColor = Math.Max(255 - (int)(Math.Abs(value) / 2 * 150), 0);
            System.Drawing.Color c;
            if (value < 0)
                c = System.Drawing.Color.FromArgb(255, otherColor, otherColor);
            else if (value > 0)
                c = System.Drawing.Color.FromArgb(otherColor, 255, otherColor);
            else
                c = System.Drawing.Color.FromArgb(255, 255, 255);

            string outColor = c.Name.Substring(2, 6);

            return outColor;
        }

        /**/
    }

}
