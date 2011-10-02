using System;
using IHI.Server.Habbos.Catalogue;

namespace IHI.Server.Rooms.Furni
{
    public abstract class Furni : CatalogueItem, IFloorPositionable
    {
        #region IFloorPositionable Members

        public FloorPosition GetPosition()
        {
            throw new NotImplementedException();
        }

        public RoomUnit SetPosition(FloorPosition position)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}