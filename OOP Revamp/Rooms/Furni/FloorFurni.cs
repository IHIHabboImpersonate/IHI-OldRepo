using System;

namespace IHI.Server.Rooms.Furni
{
    public abstract class FloorFurni : Furni, IRollerable
    {
        #region IRollerable Members

        public IRollerable Roll(FloorPosition To)
        {
            throw new NotImplementedException();
        }

        public IRollerable Roll(FloorPosition From, FloorPosition To)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}