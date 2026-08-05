using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace RotoMonster.Core
{
    public class OwnershipPlayerChange
    {
        public readonly OwnershipPlayer NewOwnershipPlayer;
        public readonly OwnershipPlayer OldOwnershipPlayer;

        public OwnershipPlayerChange(OwnershipPlayer newOwnershipPlayer, OwnershipPlayer oldOwnershipPlayer)
        {
            this.NewOwnershipPlayer = newOwnershipPlayer;
            this.OldOwnershipPlayer = oldOwnershipPlayer;
        }

        public int PlayerId
        {
            get
            {
                return NewOwnershipPlayer.PlayerId;
            }
        }

        public double OwnershipPercentChange
        {
            get
            {
                if (NewOwnershipPlayer != null && OldOwnershipPlayer != null)
                    return NewOwnershipPlayer.OwnershipPercent - OldOwnershipPlayer.OwnershipPercent;
                else
                    return 0;
            }
        }

        public double ActivePercentChange
        {
            get
            {
                if (NewOwnershipPlayer != null && OldOwnershipPlayer != null)
                    return NewOwnershipPlayer.ActivePercent - OldOwnershipPlayer.ActivePercent;
                else
                    return 0;
            }
        }

    }
}
