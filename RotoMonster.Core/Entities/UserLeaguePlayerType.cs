using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace RotoMonster.Core
{
    public class UserLeaguePlayerType
    {
        public int UserLeagueId { get; set; }
        public int PlayerTypeId { get; set; }
        public int CategoriesStringId { get; set; }


        public UserLeague UserLeague { get; set; }
        public PlayerType PlayerType { get; set; }
        public CategoriesString CategoriesString { get; set; }

        [NotMapped]
        public string CategoriesCode1 { get; set; } = "";

    }
}

