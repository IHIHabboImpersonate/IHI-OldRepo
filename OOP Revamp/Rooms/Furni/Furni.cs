using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using IHI.Server.Habbos.Catalogue;

namespace IHI.Server.Rooms.Furni
{
    public abstract class Furni : CatalogueItem, IFloorPositionable
    {
        public FloorPosition GetPosition()
        {
            throw new NotImplementedException();
        }

        public RoomUnit SetPosition(FloorPosition Position)
        {
            throw new NotImplementedException();
        }
    }
}
