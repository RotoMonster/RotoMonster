using System;
using System.Collections.Generic;
using System.Text;

namespace RotoMonster.Core
{
    public class PositionSource
    {
        public int Id { get; set; }
        public int ProviderId { get; set; }

        public int FantasyProviderId { get; set; }

        public FantasyProvider FantasyProvider { get; set; }
    }
}
