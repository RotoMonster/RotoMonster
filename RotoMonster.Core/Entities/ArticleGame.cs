using System;
using System.Collections.Generic;
using System.Text;

namespace RotoMonster.Core
{
    public class ArticleGame
    {
        public int ArticleId { get;set; }
        public int GameId { get; set; }

        public Article Article { get; set; }
        public Game Game { get; set; }
    }
}
