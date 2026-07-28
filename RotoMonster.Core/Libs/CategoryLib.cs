using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.ComponentModel.DataAnnotations;

namespace RotoMonster.Core.Libs
{
    public class CategoryLib
    {
        public Category GetCategory(List<Category> categories, string abbreviation)
        {
            var cat = (from c in categories where c.Abbreviation.ToUpper() == abbreviation.ToUpper() select c).FirstOrDefault();
            if (cat == null)
            {
                foreach (var c in categories)
                {
                    if (c.OtherAbbreviations != null)
                    {
                        foreach (string abv in c.OtherAbbreviations.Split(','))
                        {
                            if (abv.Trim().ToUpper() == abbreviation.ToUpper())
                                return c;
                        }
                    }
                }
            }

            return cat;
        }

        public Category GetFanTraxCategory(List<Category> categories, string fanTraxCatId)
        {
            var cat = (from c in categories where c.FanTraxId == fanTraxCatId select c).FirstOrDefault();

            return cat;
        }

        public List<Category> GetFanTraxCategories(List<Category> categories, string fanTraxGroup, string fanTraxCatId)
        {
            var cats = (from c in categories where c.FanTraxGroup == fanTraxGroup && c.FanTraxId == fanTraxCatId select c).ToList();

            if (cats.Count == 0)
            {
                cats = (from c in categories where c.FanTraxId == fanTraxCatId select c).ToList();
            }

            return cats;
        }

    }

    public class CategorySelect
    {
        public string Title { get; set; }
        public string Name { get; set; }
        public string Id { get; set; }
        public string Value { get; set; }
    }

    public class PointsValue
    {
        public string Title { get; set; }
        public string Name { get; set; }
        public string Id { get; set; }

        [Range(-100, 100)]
        public double Value { get; set; }
    }

    public class ActiveRosterSpotValue
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Name { get; set; }

        [Range(0, 100)]
        public int Value { get; set; }
    }

}
