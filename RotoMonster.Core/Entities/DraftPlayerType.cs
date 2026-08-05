using System;
using System.Collections.Generic;
using System.Text;

namespace RotoMonster.Core
{
    public class DraftPlayerType
    {
        public int DraftId { get; set; }
        public int PlayerTypeId { get; set; }
        public int CategoriesStringId { get; set; }


        public Draft Draft { get; set; }
        public PlayerType PlayerType { get; set; }
        public CategoriesString CategoriesString { get; set; }

    }
}
