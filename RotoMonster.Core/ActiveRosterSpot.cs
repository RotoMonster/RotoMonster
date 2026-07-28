using System;
using System.Collections.Generic;
using System.Text;

namespace RotoMonster.Core
{
    public class ActiveRosterSpot
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string YahooTitle { get; set; }
        public string FanTraxTitle { get; set; }
        public string ESPNTitle { get; set; }
        public int DefaultNumberOf { get; set; }
        public int DisplayOrder { get; set; }
        public int FilterDisplayOrder { get; set; }
        public bool UsesEase { get; set; }

        public List<ActiveRosterSpotPosition> ActiveRosterSpotPositions { get; set; }

        public PlayerType PlayerType
        {
            get
            {
                foreach(var activeRosterSpotPosition in ActiveRosterSpotPositions)
                    return activeRosterSpotPosition.Position.PlayerType;

                return null;
            }
        }

        public bool PositionQualifies(Position position)
        {
            foreach (var arsp in ActiveRosterSpotPositions)
            {
                if (arsp.PositionId == position.Id)
                    return true;
            }

            return false;
        }

        public bool PositionsQualify(List<Position> positions)
        {
            foreach (Position position in positions)
                if (PositionQualifies(position))
                    return true;

            return false;
        }

    }
}
