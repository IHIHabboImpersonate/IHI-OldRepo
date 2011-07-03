using System;
using System.Collections.Generic;
using System.Text;

namespace IHI.Server
{
    public abstract class Room
    {
        private RoomModel fModel;
        private List<IHI.Server.FloorItem> fFloorItems;
        private List<WallItem> fWallItems;
        private List<RoomUnit> fUnits;
    }
}
