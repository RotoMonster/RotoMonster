using System;
using System.Collections.Generic;
using System.Text;

namespace RotoMonster.Core
{
    public class PlayerStatusTagType
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Icon { get; set; }
        public bool? IsDefault { get; set; }
        public int DisplayOrder { get; set; }

    }
}
