using System;

namespace IHI.Server.Rooms.Furni
{
    public abstract class FloorFurni : Furni, IRollerable
    {
        #region IRollerable Members

        public IRollerable Roll(FloorPosition to)
        {
            throw new NotImplementedException();
        }

        public IRollerable Roll(FloorPosition from, FloorPosition to)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}