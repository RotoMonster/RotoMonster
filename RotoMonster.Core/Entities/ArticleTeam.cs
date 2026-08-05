using System;
using System.Collections.Generic;
using System.Text;

namespace RotoMonster.Core
{
    public class ArticleTeam
    {
        public int ArticleId { get; set; }
        public int TeamId { get; set; }

        public Article Article { get; set; }
        public Team Team { get; set; }
    }
}
