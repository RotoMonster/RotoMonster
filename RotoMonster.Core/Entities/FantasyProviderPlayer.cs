using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace RotoMonster.Core
{
    public class FantasyProviderPlayer
    {
        public int FantasyProviderId { get; set; }
        public int PlayerId { get; set; }

        public FantasyProvider FantasyProvider { get; set; }
        public Player Player { get; set; }
        public string ProviderId { get; set; }
    }
}
