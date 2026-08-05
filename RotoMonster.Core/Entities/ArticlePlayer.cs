using System;
using System.Collections.Generic;
using System.Text;

namespace RotoMonster.Core
{
    public class ArticlePlayer
    {
        public int ArticleId { get; set; }
        public int PlayerId { get; set; }

        public Article Article { get; set; }
        public Player Player { get; set; }
    }
}
