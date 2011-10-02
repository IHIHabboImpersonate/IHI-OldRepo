namespace IHI.Server.Rooms
{
    public interface IFloorPositionable
    {
        /// <summary>
        /// Returns the current position the IFloorPositionable is at in the room.
        /// </summary>
        FloorPosition GetPosition();

        /// <summary>
        /// Sets the current position the IFloorPositionable is at in the room.
        /// </summary>
        /// <returns>The current IFloorPositionable. This allows chaining.</returns>
        RoomUnit SetPosition(FloorPosition position);
    }
}