using System;
using System.Collections.Generic;
using System.Text;

namespace RotoMonster.Core
{
    public class DisplayCategory
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsBeforeStats { get; set; }
        public bool IsAfterStats { get; set; }

        public Category Category { get; set; }
    }
}
