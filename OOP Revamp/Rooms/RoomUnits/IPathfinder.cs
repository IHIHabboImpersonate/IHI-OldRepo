using System.Collections.Generic;

namespace IHI.Server.Rooms.RoomUnits
{
    public interface IPathfinder
    {
        void ApplyCollisionMap(byte[,] map, float[,] height);
        ICollection<byte[]> Path(byte startX, byte startY, byte endX, byte endY, float maxDrop, float maxJump);
    }
}