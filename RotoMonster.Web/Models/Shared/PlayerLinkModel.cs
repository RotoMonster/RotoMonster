using System;
using System.Collections.Generic;
using System.Text;
using RotoMonster.Core;

namespace RotoMonster.Models.Shared
{
    public class PlayerLinkModel
    {
        public Player Player { get; set; }
        public DisplayPlayer DisplayPlayer { get; set; } = null;
        public UserLeague SelectedUserLeague { get; set; }
        public PlayerGameState PlayerGameState { get; set; } = null;
    }
}
