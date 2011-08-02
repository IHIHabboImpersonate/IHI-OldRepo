using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IHI.Server.Rooms
{
    public interface IRollerable : IFloorPositionable
    {
        IRollerable Roll(FloorPosition To);
        IRollerable Roll(FloorPosition From, FloorPosition To);
    }
}
