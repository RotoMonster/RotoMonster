using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RotoMonster.Core.Libs;

namespace RotoMonster.Core
{
    public class ValuePlayerLib
    {
        ColorLib colorLib = new ColorLib();

        public List<ValuePlayer> GetValuePlayers(
            List<StatPlayer> statPlayers,
            List<CategorySetting> categorySettings,
            Category gameCategory,
            string scoringSystem,
            PerValue perValue,
            PlayerType playerType,
            List<Category> displayCategories,
            int leagueSize,
            out ValueAverages outValueAverages
            )
        {
            outValueAverages = new ValueAverages();

            if (statPlayers.Count == 0)
                return new List<ValuePlayer>();

            List<ValuePlayer> outVp = null;

            if (scoringSystem == "C")
            {
                List<ValuePlayer> valuePlayers = new List<ValuePlayer>();

                foreach (var statPlayer in statPlayers)
                {
                    ValuePlayer vp = new ValuePlayer();
                    vp.StatPlayer = statPlayer;
                    valuePlayers.Add(vp);
                }

                Dictionary<int, double> catTotals = new Dictionary<int, double>();
                foreach (var cs in categorySettings)
                {
                    if (cs.Category.WeightCategoryId == null)
                    {
                        List<double> allValues = new List<double>();
                        foreach (var statPlayer in statPlayers)
                        {
                            double stat = statPlayer.Get(perValue, cs.Category.Id);
                            allValues.Add(stat);
                        }

                        double avg = allValues.Average();
                        outValueAverages.Averages[cs.Category.Id] = avg;
                        double sum = allValues.Sum(d => Math.Pow(d - avg, 2));
                        double stdev = Math.Sqrt((sum) / (allValues.Count() - 1));
                        outValueAverages.Stdevs[cs.Category.Id] = stdev;
                    }
                    else
                    {
                        // get AVG
                        double totalRatios = 0;
                        double totalWeight = 0;
                        foreach (var statPlayer in statPlayers)
                        {
                            double ratio = statPlayer.Get(perValue, cs.Category.Id);
                            double weight = statPlayer.Get(perValue, cs.Category.WeightCategoryId.GetValueOrDefault());
                            totalRatios += (ratio * weight);
                            totalWeight += weight;
                        }
                        double avg = 0;
                        if (totalWeight > 0)
                            avg = totalRatios / totalWeight;
                        outValueAverages.Averages[cs.Category.Id] = avg;

                        // get AVG A values
                        List<double> aValues = new List<double>();
                        foreach (var statPlayer in statPlayers)
                        {
                            double ratio = statPlayer.Get(perValue, cs.Category.Id);
                            double weight = statPlayer.Get(perValue, cs.Category.WeightCategoryId.GetValueOrDefault());
                            if (weight > 0)
                            {
                                double aVal = (ratio - avg) * weight;
                                aValues.Add(aVal);
                            }
                        }

                        double avgA = aValues.Count>0? aValues.Average():0;
                        outValueAverages.AverageAs[cs.Category.Id] = avgA;
                        double sum = aValues.Sum(d => Math.Pow(d - avgA, 2));
                        double stdev = Math.Sqrt((sum) / (aValues.Count() - 1));
                        outValueAverages.Stdevs[cs.Category.Id] = stdev;
                    }
                }
                FillCategoryValuePlayersAndColors(valuePlayers, statPlayers, categorySettings, perValue, outValueAverages);

                var outValuePlayers = (from v in valuePlayers orderby v.LeagueValue descending select v).ToList();

                if (leagueSize == 0)    // don't filter top players, just return
                {
                    return outValuePlayers;
                }

                // recalc avg/stdev using top players
                var limitedValuePlayers = new List<ValuePlayer>();
                foreach (var v in outValuePlayers)
                {
                    if (leagueSize != 0 && perValue.SkillCategoryValue != null)
                    {
                        double games = v.StatPlayer.Get(gameCategory.Id);
                        if (games > 0)
                        {
                            double catValue = v.StatPlayer.Get(perValue.Category.Id);
                            double val = catValue / games;
                            if (val < perValue.SkillCategoryValue.GetValueOrDefault())
                                continue;
                        }
                    }

                    limitedValuePlayers.Add(v);

                    if (limitedValuePlayers.Count == leagueSize)
                        break;
                }

                List<StatPlayer> limitedStatPlayers = new List<StatPlayer>();
                foreach (var sp in statPlayers)
                {
                    var match = (from v in limitedValuePlayers where v.StatPlayer.Player.Id == sp.Player.Id select v).FirstOrDefault();
                    if (match != null)
                    {
                        limitedStatPlayers.Add(sp);
                    }
                }
                GetValuePlayers(limitedStatPlayers, categorySettings, gameCategory, scoringSystem, perValue, playerType, displayCategories, 0, out outValueAverages);   // updates outValueAverages
                FillCategoryValuePlayersAndColors(valuePlayers, statPlayers, categorySettings, perValue, outValueAverages);
                var outValuePlayers2 = (from v in valuePlayers orderby v.LeagueValue descending select v).ToList();

                outVp = outValuePlayers2;
            }
            else // Points
            {
                List<ValuePlayer> valuePlayers = new List<ValuePlayer>();
                foreach (var statPlayer in statPlayers)
                {
                    var valuePlayer = GetPointsValuePlayer(statPlayer, perValue, categorySettings);
                    valuePlayers.Add(valuePlayer);
                }

                var topValuePlayers = (from v in valuePlayers orderby v.LeagueValue descending select v).Take(leagueSize).ToList();

                double totalValues = 0;
                foreach (var vp in topValuePlayers)
                    totalValues += vp.LeagueValue;
                double avg = 0;
                double colorAvg = 0;
                if (topValuePlayers.Count > 0 && categorySettings.Count > 0)
                {
                    avg = totalValues / Convert.ToDouble(topValuePlayers.Count);
                    colorAvg = avg / Convert.ToDouble(categorySettings.Count);
                }

                outValueAverages.PointsAverageValue = avg;
                outValueAverages.CategoryAverageValue = colorAvg;
                outValueAverages.PointsMinValue = topValuePlayers.Min(v => v.LeagueValue);
                outValueAverages.PointsMaxValue = topValuePlayers.Max(v => v.LeagueValue);

                FillPointsValuePlayersAndColors(valuePlayers, outValueAverages, categorySettings);

                var outValuePlayers = (from v in valuePlayers orderby v.LeagueValue descending select v).ToList();
                int rank = 0;
                foreach (var v in outValuePlayers)
                {
                    rank++;
                    v.Rank = rank;
                }

                outVp = outValuePlayers;
            }

            // stat colors
            foreach (var c in displayCategories)
            {
                double max = outVp.Where(vp => vp.Rank <= leagueSize).Max(vp => vp.StatPlayer.Get(perValue, c.Id));
                double min = outVp.Where(vp => vp.Rank <= leagueSize).Min(vp => vp.StatPlayer.Get(perValue, c.Id));

                double diff = max - min;

                if (diff != 0)
                {
                    foreach (var vp in outVp)
                    {
                        var val = vp.StatPlayer.Get(perValue, c.Id);
                        var percent = (val - min) / diff * 100;
                        percent = Math.Max(0, percent);
                        if (!c.IsPositive.GetValueOrDefault())
                            percent = (100 - percent);
                        vp.SetStatColor(c.Id, percent);
                    }
                }
            }

            return outVp;
        }

        public ValuePlayer GetPointsValuePlayer(StatPlayer statPlayer, PerValue perValue, List<CategorySetting> categorySettings)
        {
            var valuePlayer = new ValuePlayer();
            valuePlayer.StatPlayer = statPlayer;
            foreach (var cs in categorySettings)
            {
                double stat = statPlayer.Get(perValue, cs.Category.Id);
                double val = stat * cs.PointsPerStat;
                valuePlayer.Set(cs.Category.Id, val);
                valuePlayer.LeagueValue += val;
            }

            return valuePlayer;
        }

        public void FillPointsValuePlayersAndColors(List<ValuePlayer> valuePlayers, ValueAverages valueAverages, List<CategorySetting> categorySettings)
        {
            double range = valueAverages.PointsMaxValue - valueAverages.PointsMinValue;
            foreach (var vp in valuePlayers)
            {
                if (valueAverages.PointsAverageValue > 0 && valueAverages.CategoryAverageValue > 0)
                {
                    double colorTmp = range == 0 ? 0 : (vp.LeagueValue - valueAverages.PointsAverageValue) / range * 100;
                    string c;
                    if (colorTmp >= 0)
                        c = colorLib.GetGreenRangeColorStyle(colorTmp, 0, 100, true);
                    else
                        c = colorLib.GetRedRangeColorStyle(-1 * colorTmp, 0, 100, true);
                    vp.LeagueValueColor = c;
                    foreach (var cs in categorySettings)
                    {
                        double val = vp.Get(cs.Category.Id, 0);
                        double colorVal = (val - valueAverages.CategoryAverageValue) / valueAverages.CategoryAverageValue;
                        vp.SetC(cs.Category.Id, vp.GetValueColor(colorVal));
                    }
                }
            }
        }

        public void FillCategoryValuePlayersAndColors(
            List<ValuePlayer> valuePlayers,
            List<StatPlayer> statPlayers,
            List<CategorySetting> categorySettings,
            PerValue perValue,
            ValueAverages valueAverages)
        {
            if (valueAverages.Averages.Count == 0)
                return;

            foreach (var cs in categorySettings)
            {
                if (cs.Category.WeightCategoryId == null)
                {
                    double avg = valueAverages.Averages[cs.Category.Id];
                    double stdev = valueAverages.Stdevs[cs.Category.Id];

                    foreach (var statPlayer in statPlayers)
                    {
                        var vp = (from v in valuePlayers where v.StatPlayer.Player.Id == statPlayer.Player.Id && v.StatPlayer.TeamActiveRosterSpotId==statPlayer.TeamActiveRosterSpotId select v).First();
                        double stat = statPlayer.Get(perValue, cs.Category.Id);
                        double val = 0;
                        if (stdev > 0)
                        {
                            val = (stat - avg) / stdev;
                            if (!cs.Category.IsPositive.GetValueOrDefault(false))
                                val *= -1;
                            vp.Set(cs.Category.Id, val);
                        }
                    }
                }
                else
                {
                    double avg = valueAverages.Averages[cs.Category.Id];
                    double avgA = valueAverages.AverageAs[cs.Category.Id];
                    double stdev = valueAverages.Stdevs[cs.Category.Id];
                    foreach (var statPlayer in statPlayers)
                    {
                        var vp = (from v in valuePlayers where v.StatPlayer.Player.Id == statPlayer.Player.Id && v.StatPlayer.TeamActiveRosterSpotId == statPlayer.TeamActiveRosterSpotId select v).First();
                        double ratio = statPlayer.Get(perValue, cs.Category.Id);
                        double weight = statPlayer.Get(perValue, cs.Category.WeightCategoryId.GetValueOrDefault());
                        if (weight > 0)
                        {
                            double aVal = (ratio - avg) * weight;
                            double val = 0;
                            if (stdev > 0)
                            {
                                val = (aVal - avgA) / stdev;
                                if (!cs.Category.IsPositive.GetValueOrDefault(false))
                                    val *= -1;
                                vp.Set(cs.Category.Id, val);
                            }
                        }
                    }
                }
            }
            foreach (var v in valuePlayers)
                v.FillTotalValuesAndColors(categorySettings);

            int rank = 0;
            foreach (var v in (from v1 in valuePlayers orderby v1.LeagueValue descending select v1))
            {
                rank++;
                v.Rank = rank;
            }

        }

        public ValuePlayer GetTeamValuePlayer(List<ValuePlayer> teamValuePlayers, Team team, ActiveRosterSpot activeRosterSpot)
        {
            if (activeRosterSpot != null)
                return (from vp in teamValuePlayers where vp.Player.Id == team.Id && vp.StatPlayer.TeamActiveRosterSpotId == activeRosterSpot.Id select vp).FirstOrDefault();
            else
                return null;
        }

        /**/
    }
}
