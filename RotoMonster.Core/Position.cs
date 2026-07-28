using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace RotoMonster.Core
{
    public class Position
    {
        public int Id { get; set; }
        public PlayerType PlayerType { get; set; }
        public string Title { get; set; }
        public string Abbreviation { get; set; }
        public bool IsDefault { get; set; }
        public bool IsActualPosition { get; set; }
        public int LineupCount { get; set; }   // # at this position in actual lineup (for example, 2 Defenders in NHL)
        public string ColorCode { get; set; }
        public int DisplayOrder { get; set; }

        [NotMapped]
        public string ColoredHtml
        {
            get
            {
                return "<span style='color:#" + ColorCode + "'>" + Abbreviation + "</span>";
            }
        }
    }
}
