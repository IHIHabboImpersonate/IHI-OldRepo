namespace IHI.Server.Rooms
{
    public interface ITalkable
    {
        /// <summary>
        /// Shout a message from the ITalkable to all RoomUnits in the room.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <returns>The current ITalkable object. This allows chaining.</returns>
        ITalkable Shout(string message);

        /// <summary>
        /// Say a message from the ITalkable to near RoomUnits in the room.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <returns>The current ITalkable object. This allows chaining.</returns>
        ITalkable Say(string message);

        /// <summary>
        /// Whisper a message from the ITalkable to another RoomUnit in the room.
        /// </summary>
        /// <param name="recipient">The RoomUnit to recieve the message.</param>
        /// <param name="message">The message to send.</param>
        /// <returns>The current ITalkable object. This allows chaining.</returns>
        ITalkable Whisper(ITalkable recipient, string message);
    }
}