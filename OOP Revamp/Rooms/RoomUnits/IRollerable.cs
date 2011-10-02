namespace IHI.Server.Rooms
{
    public interface IRollerable : IFloorPositionable
    {
        IRollerable Roll(FloorPosition to);
        IRollerable Roll(FloorPosition from, FloorPosition to);
    }
}