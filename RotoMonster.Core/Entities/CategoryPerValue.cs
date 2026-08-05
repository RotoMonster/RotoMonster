using System;
using System.Collections.Generic;
using System.Text;

namespace RotoMonster.Core
{
    public class CategoryPerValue
    {
        public int CategoryId { get; set; }
        public int PerValueId { get; set; }
        public string DisplayFormat { get; set; }

        public Category Category { get; set; }
        public PerValue PerValue { get; set; }
    }
}
