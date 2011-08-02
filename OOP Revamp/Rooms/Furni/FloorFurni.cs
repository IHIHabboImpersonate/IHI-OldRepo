using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IHI.Server.Rooms.Furni
{
    public abstract class FloorFurni : Furni, IRollerable
    {
        public IRollerable Roll(FloorPosition To)
        {
            throw new NotImplementedException();
        }

        public IRollerable Roll(FloorPosition From, FloorPosition To)
        {
            throw new NotImplementedException();
        }
    }
}
