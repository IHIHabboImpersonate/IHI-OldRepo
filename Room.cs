using System;
using System.Collections.Generic;
using System.Text;

namespace IHI.Server.Rooms
{
    public abstract class Room
    {
        private RoomModel fModel;
        private List<IHI.Server.FloorItem> fFloorItems;
        private List<WallItem> fWallItems;
        private List<IRoomUnit> fUnits;
    }
}
