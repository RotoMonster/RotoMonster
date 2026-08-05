using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace RotoMonster.Core
{
    public class PlayerType
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int DisplayOrder { get; set; }
        public string SingularTitle { get; set; }
        public string PluralTitle { get; set; }
        public int DefaultPerTeam { get; set; }
        public bool IsDefault { get; set; }
        public bool IsDisabled { get; set; }
        public bool IsStreamable { get; set; }

        public List<SeasonPlayer> SeasonPlayers { get; set; }

        [NotMapped]
        public string DisplayTitle { get { return SingularTitle == "Player" ? "" : SingularTitle; } }

    }
}
