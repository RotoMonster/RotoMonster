using System;
using System.Collections.Generic;
using System.Text;

namespace RotoMonster.Core
{
    public class PerValue
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string ColumnTitle { get; set; }
        public int PlayerTypeId { get; set; }
        public int? CategoryId { get; set; }
        public bool? IsDefault { get; set; }
        public bool? IsDefaultDisplay { get; set; }
        public double? SkillCategoryValue { get; set; }
        public int DisplayOrder { get; set; }

        public PlayerType PlayerType { get; set; }
        public Category Category { get; set; }
    }
}
