using System.Collections.Generic;

namespace IHI.Server.Rooms.RoomUnits
{
    public interface IPathfinder
    {
        void ApplyCollisionMap(byte[,] Map, float[,] Height);
        ICollection<byte[]> Path(byte StartX, byte StartY, byte EndX, byte EndY, float MaxDrop, float MaxJump);
    }
}