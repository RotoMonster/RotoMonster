using System;
using System.Collections.Generic;
using System.Text;

namespace RotoMonster.Core
{
    public class PlayerStatusType
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string BackgroundColor { get; set; }
        public string TextColor { get; set; }
        public string TextFormat { get; set; }
        public bool? AutoClear { get; set; }
        public bool? UsesDate { get; set; }
        public bool? ShowInDaily { get; set; }
        public bool? AllowFilter { get; set; }
        public bool? AppliesToNextGame { get; set; }
        public bool? IsInGame { get; set; }
        public bool? IsUndetermined { get; set; }
        public bool? ShowOnPlayerProfile { get; set; }
        public int? EndOfGameMissedPlayerStatusTypeId { get; set; }
        public int? EndOfGamePlayedPlayerStatusTypeId { get; set; }
        public string TweetTemplate { get; set; }
        public string UpdateTemplate { get; set; }
        public string PlayType { get; set; }

    }

}
