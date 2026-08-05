using RotoMonster.Core.Libs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RotoMonster.Core
{
    public class StatPlayer
    {
        private Dictionary<int, double> _stats = new Dictionary<int, double>();
        private Dictionary<int, Dictionary<int, double>> _perStats = new Dictionary<int, Dictionary<int, double>>();

        public Player Player { get; set; }
        public Team Team { get; set; }
        public Team Team2 { get; set; }
        public int TeamActiveRosterSpotId { get; set; }
        public PlayerType PlayerType { get; set; }
        public ActiveRosterSpot ActiveRosterSpot { get; set; } = null; // only used for ease players
        public int Games { get; set; } = 0;
        Game IndividualGame { get; set; } = null;
        public Category MeasureCategory { get; set; } = null;
        public string MeasureText = "";

        public void Set(int categoryId, double value)
        {
            _stats[categoryId] = value;
        }

        public double Get(int categoryId, double defaultValue = 0)
        {
            if (_stats.ContainsKey(categoryId))
                return Convert.ToDouble(_stats[categoryId]);

            return defaultValue;
        }

        public double Get(PerValue perValue, int categoryId, double defaultValue = 0)
        {
            if (perValue == null || perValue.Category == null)
                return Get(categoryId, defaultValue);

            if (_perStats[perValue.Id].ContainsKey(categoryId))
                return Convert.ToDouble(_perStats[perValue.Id][categoryId]);

            return defaultValue;
        }

        public double Ratio(int topCategoryId, int bottomCategoryId, double multiplier = 1)
        {
            double v = 0;
            double top = Get(topCategoryId);
            double bottom = Get(bottomCategoryId);
            if (bottom != 0)
                v = (top / bottom) * multiplier;

            return v;
        }

        public void FillCalculated(Sport sport, List<Category> categories, PlayerType playerType)
        {
            if (sport.IsNBA)
            {
                FillCalculatedNBA(categories);
            }
            if (sport.IsMLB)
            {
                if (playerType.Title == "Hitters")
                    FillCalculatedMLBHitters(categories);
                else if (playerType.Title == "Pitchers")
                    FillCalculatedMLBPitchers(categories);
            }
            if (sport.IsNFL)
            {
                if (playerType.Title == "Defense")
                    FillCalculatedNFLDefenses(categories);
                else if (playerType.Title == "Offensive")
                    FillCalculatedNFLOffensive(categories);
                else if (playerType.Title == "Kickers")
                    FillCalculatedNFLKickers(categories);

            }
            if (sport.IsNHL)
            {
                if (playerType.Title == "Skater")
                    FillCalculatedNHLSkaters(categories);
                else if (playerType.Title == "Goalie")
                    FillCalculatedNHLGoalies(categories);
            }
        }

        public void FillCalculatedNHLSkaters(List<Category> categories)
        {
            foreach (var c in (from c1 in categories where c1.SourceField == null select c1))
            {
                switch (c.Id)
                {
                    case 10: // Time On Ice
                        Set(c.Id, Get(11) + Get(12) + Get(13));
                        break;
                    case 1: // Goals
                        Set(c.Id, Get(14) + Get(15) + Get(16));
                        break;
                    case 2: // Assists
                        Set(c.Id, Get(19) + Get(20) + Get(21));
                        break;
                    case 4: // PP Points
                        Set(c.Id, Get(14) + Get(19));
                        break;
                    case 5: // Shots
                        Set(c.Id, Get(22) + Get(23) + Get(24));
                        break;
                    case 49: // games (already filled)
                        break;

                    default:
                        throw new Exception("Unexpected Category Id " + c.Id.ToString());
                }
            }
        }

        public void FillCalculatedNHLGoalies(List<Category> categories)
        {
            foreach (var c in (from c1 in categories where c1.SourceField == null select c1))
            {
                double shotsAgainst = Get(66) + Get(67) + Get(68) + Get(69);
                double goalsAgainst = Get(61) + Get(62) + Get(63) + Get(64);

                switch (c.Id)
                {
                    case 57: // Time On Ice
                        Set(c.Id, Get(58) + Get(59) + Get(60));
                        break;
                    case 72: // Shots Against
                        Set(c.Id, shotsAgainst);
                        break;
                    case 52: // Goals Against
                        Set(c.Id, goalsAgainst);
                        break;
                    case 51: // Goals Against Average
                        double timeOnIce = Get(58) + Get(59) + Get(60);
                        if (timeOnIce > 0)
                            Set(c.Id, goalsAgainst / timeOnIce * 60);
                        else
                            Set(c.Id, 0);
                        break;
                    case 53: // Save %
                        if (shotsAgainst > 0)
                            Set(c.Id, 1 - goalsAgainst / shotsAgainst);
                        else
                            Set(c.Id, 0);
                        break;
                    case 56: // Saves
                        Set(c.Id, shotsAgainst - goalsAgainst);
                        break;
                    case 55: // Assists
                        Set(c.Id, 0);
                        break;
                    case 71: // games (already filled)
                        break;

                    default:
                        throw new Exception("Unexpected Category Id " + c.Id.ToString());
                }
            }
        }

        public void FillCalculatedNBA(List<Category> categories)
        {
            double twosAttempted = Get(14) - Get(23);
            double twosMade = Get(13) - Get(2);
            double top, bottom;

            foreach (var c in (from c1 in categories where c1.SourceField == null select c1))
            {
                switch (c.Id)
                {
                    case 1: // Points
                        Set(c.Id, Get(17) + twosMade * 2 + Get(2) * 3);
                        break;
                    case 3: // Rebounds
                        Set(c.Id, Get(9) + Get(10));
                        break;
                    case 7: // Field Goal %
                        Set(c.Id, Ratio(13, 14));
                        break;
                    case 8: // Free Throw %
                        Set(c.Id, Ratio(17, 18));
                        break;
                    case 15: // Field Goals Missed
                        Set(c.Id, Get(14) - Get(13));
                        break;
                    case 19: // Free Throws Missed
                        Set(c.Id, Get(18) - Get(17));
                        break;
                    case 26: // Twos Made
                        Set(c.Id, Get(13) - Get(2));
                        break;
                    case 27: // Twos Attempted
                        Set(c.Id, Get(14) - Get(23));
                        break;
                    case 24: // Threes Missed
                        Set(c.Id, Get(23) - Get(2));
                        break;
                    case 29: // Assist to Turnovers
                        Set(c.Id, Ratio(4, 25));
                        break;
                    case 31: // Net Free Throws
                        Set(c.Id, Get(17) - (Get(18) - Get(17)));
                        break;
                    case 32: // Threes %
                        Set(c.Id, Ratio(2, 23));
                        break;
                    case 33: // Twos %
                        if (twosAttempted > 0)
                            Set(c.Id, (twosMade / twosAttempted));
                        else
                            Set(c.Id, 0);
                        break;
                    case 34: // Adjusted FG% (2pt FGM + 1.5 * 3pt FGM) / FGA
                        top = twosMade + 1.5 * Get(2);
                        bottom = Get(14);
                        if (bottom > 0)
                            Set(c.Id, top / bottom);
                        else
                            Set(c.Id, 0);
                        break;
                    case 46: // Steals+Blocks
                        Set(c.Id, Get(5) + Get(6));
                        break;
                    case 56: // Assists minus Turnovers
                        Set(c.Id, Get(4) - Get(25));
                        break;
                    case 57: // Assists plus Turnovers
                        Set(c.Id, Get(4) + Get(25));
                        break;
                    case 58: // TS% PTS/(2*(fga+0.44*fta))
                        top = Get(1);
                        bottom = 2 * (Get(14) + 0.44 * Get(18));
                        if (bottom > 0)
                            Set(c.Id, top / bottom);
                        else
                            Set(c.Id, 0);
                        break;
                    case 61: // Twos Missed
                        Set(c.Id, twosAttempted - twosMade);
                        break;
                    case 20: // games (already filled)
                        break;
                    case 22: // starts (already filled)
                        break;
                    default:
                        throw new Exception("Unexpected Category Id " + c.Id.ToString());
                }
            }
        }

        public void FillCalculatedMLBPitchers(List<Category> categories)
        {
            double top, bottom;

            foreach (var c in (from c1 in categories where c1.SourceField == null && !c1.IsDisabled.GetValueOrDefault() select c1))
            {
                switch (c.Id)
                {
                    case 51: // ERA
                        top = Get(50);
                        bottom = Get(46);
                        if (bottom > 0)
                            Set(c.Id, top / bottom * 9);
                        else
                            Set(c.Id, 0);
                        break;
                    case 55: // WHIP
                        top = Get(75) + Get(80);
                        bottom = Get(46);
                        if (bottom > 0)
                            Set(c.Id, top / bottom);
                        else
                            Set(c.Id, 0);
                        break;
                    case 109: // SV+HLD
                        Set(c.Id, Get(49) + Get(57));
                        break;
                    case 47: // games (already filled)
                        break;
                    case 100: // starts (already filled)
                        break;
                    default:
                        break;
                        // throw new Exception("Unexpected Category Id " + c.Id.ToString());
                }
            }

        }

        public void FillCalculatedMLBHitters(List<Category> categories)
        {
            double top, bottom;

            foreach (var c in (from c1 in categories where c1.SourceField == null && !c1.IsDisabled.GetValueOrDefault() select c1))
            {
                switch (c.Id)
                {
                    case 39: // Total Bases
                        Set(c.Id, Get(36) * 1 + Get(37) * 2 + Get(38) * 3 + Get(31) * 4);
                        break;
                    case 42: // AVG
                        Set(c.Id, Ratio(30, 44));
                        break;
                    case 58: // Runs Produced (R + RBI - HR)
                        Set(c.Id, Get(34) + Get(32) - Get(31));
                        break;
                    case 61: // Extra Base Hits (2B + 3B + HR)
                        Set(c.Id, Get(37) + Get(38) - Get(31));
                        break;
                    case 63: // Net Steals
                        Set(c.Id, Get(33) - Get(41));
                        break;
                    case 66: // OBP (H+BB+HBP)/(AB+BB+HBP+SF)
                        top = Get(30) + Get(35) + Get(62);
                        bottom = Get(44) + Get(35) + Get(62) + Get(68);
                        if (bottom > 0)
                            Set(c.Id, top / bottom);
                        else
                            Set(c.Id, 0);
                        break;
                    case 67: // SLG (1B+2x2B+3x3B+4xHR)/AB
                        top = Get(36) * 1 + Get(37) * 2 + Get(38) * 3 + Get(31) * 4;
                        bottom = Get(44);
                        if (bottom > 0)
                            Set(c.Id, top / bottom);
                        else
                            Set(c.Id, 0);
                        break;
                    case 71: // OPS (1B+2x2B+3x3B+4xHR)/AB
                        Set(c.Id, Get(66) + Get(67));
                        break;
                    case 43: // games (already filled)
                        break;
                    case 122: // starts (already filled)
                        break;
                    default:
                        break;
                        // throw new Exception("Unexpected Category Id " + c.Id.ToString());
                }
            }

        }

        public void FillCalculatedNFLDefenses(List<Category> categories)
        {
            // double top, bottom;

            foreach (var c in (from c1 in categories where c1.SourceField == null && !c1.IsDisabled.GetValueOrDefault() select c1))
            {
                switch (c.Id)
                {
                    case 112: // Takeaways
                        Set(c.Id, Get(35) + Get(36));
                        break;
                    case 47: // games (already filled)
                        break;
                    case 100: // starts (already filled)
                        break;
                    default:
                        break;
                        // throw new Exception("Unexpected Category Id " + c.Id.ToString());
                }
            }

        }

        public void FillCalculatedNFLOffensive(List<Category> categories)
        {
            double top, bottom;

            foreach (var c in (from c1 in categories where c1.SourceField == null && !c1.IsDisabled.GetValueOrDefault() select c1))
            {
                switch (c.Id)
                {
                    case 113: // Passing Yards Average
                        top = Get(6);
                        bottom = Get(11);
                        if (bottom > 0)
                            Set(c.Id, top / bottom);
                        else
                            Set(c.Id, 0);
                        break;
                    case 114: // Rushing Yards Average
                        top = Get(3);
                        bottom = Get(10);
                        if (bottom > 0)
                            Set(c.Id, top / bottom);
                        else
                            Set(c.Id, 0);
                        break;
                    case 115: // Receiving Yards Average
                        top = Get(4);
                        bottom = Get(7);
                        if (bottom > 0)
                            Set(c.Id, top / bottom);
                        else
                            Set(c.Id, 0);
                        break;
                    case 117: // Total Scoring
                        Set(c.Id, Get(1) * 6 + Get(2) * 6 + Get(5) * 6 + Get(31) * 6 + Get(76) * 2 + Get(77) * 2);
                        break;

                    default:
                        break;
                        // throw new Exception("Unexpected Category Id " + c.Id.ToString());
                }
            }
        }

        public void FillCalculatedNFLKickers(List<Category> categories)
        {
            double top, bottom;

            foreach (var c in (from c1 in categories where c1.SourceField == null && !c1.IsDisabled.GetValueOrDefault() select c1))
            {
                switch (c.Id)
                {
                    case 116: // Kicking Scoring
                        Set(c.Id, Get(21) * 3 + Get(22));
                        break;

                    case 118: // Total Scoring
                        Set(c.Id, Get(21) * 3 + Get(22));
                        break;

                    default:
                        break;
                        // throw new Exception("Unexpected Category Id " + c.Id.ToString());
                }
            }
        }

        public void FillPerValueStats(List<PerValue> perValues, List<Category> categories)
        {
            foreach (var perValue in perValues)
            {
                if (perValue.Category == null)
                    continue;

                if (!_perStats.ContainsKey(perValue.Id))
                    _perStats[perValue.Id] = new Dictionary<int, double>();
                Dictionary<int, double> pers = _perStats[perValue.Id];
                foreach (var c in categories)
                {
                    if (c.WeightCategoryId == null && !c.PerValuesSameAsTotal)
                    {
                        double value = Ratio(c.Id, perValue.CategoryId.GetValueOrDefault());
                        pers[c.Id] = value;
                    }
                    else
                    {
                        pers[c.Id] = Get(c.Id);
                    }
                }
            }
        }

        public double MeasureCategoryValue
        {
            get
            {
                if (MeasureCategory != null)
                {
                    return Get(MeasureCategory.Id);
                }

                return 0;
            }
        }

        /**/
    }
}
