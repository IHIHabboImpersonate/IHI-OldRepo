
namespace IHI.Server
{
    public abstract class RoomUnit
    {
        /// <summary>
        /// The room the user is currently in.
        /// </summary>
        protected Room fRoom;

        /// <summary>
        /// The current position in the room the user is currently at.
        /// </summary>
        protected FloorLocation fPosition;

        /// <summary>
        /// The desired position in the room of this user.
        /// </summary>
        protected FloorLocation fDestination;

        /// <summary>
        /// The figure of the user.
        /// </summary>
        protected string fFigure;
        protected byte fRoomUnitID;
        protected string fDisplayName;

        /// <summary>
        /// Returns the current room the user is in.
        /// If the user is not in a room null is returned.
        /// </summary>
        /// <returns></returns>
        public Room GetRoom()
        {
            return this.fRoom;
        }

        /// <summary>
        /// Returns the current position the user is at in the room.
        /// </summary>
        public FloorLocation GetPosition()
        {
            return this.fPosition;
        }

        /// <summary>
        /// Send the current details of the User to the room.
        /// The user will poof when used.
        /// No changes to the User object are made.
        /// </summary>
        /// <returns>The current User object. This allows chaining.</returns>
        public RoomUnit Refresh()
        {

            return this;
        }

        /// <summary>
        /// Shout a message from the User to all Users in the room.
        /// </summary>
        /// <param name="Message">The message to send.</param>
        /// <returns>The current User object. This allows chaining.</returns>
        public RoomUnit Shout(string Message)
        {

            return this;
        }

        /// <summary>
        /// Whisper a message from the User to another User in the room.
        /// </summary>
        /// <param name="Recipient">The User to recieve the message.</param>
        /// <param name="Message">The message to send.</param>
        /// <returns>The current User object. This allows chaining.</returns>
        public RoomUnit Whisper(RoomUnit Recipient, string Message)
        {

            return this;
        }

        /// <summary>
        /// Say a message from the User to close Users in the room.
        /// </summary>
        /// <param name="Message">The message to send.</param>
        /// <returns>The current User object. This allows chaining.</returns>
        public RoomUnit Say(string Message)
        {

            return this;
        }

        /// <summary>
        /// Sets the desired position in the room of this user.
        /// </summary>
        /// <returns>The current User object. This allows chaining.</returns>
        public RoomUnit SetDestination(FloorLocation Destination)
        {
            this.fDestination = Destination;
            return this;
        }

        /// <summary>
        /// Sets the current position the user is at in the room.
        /// </summary>
        /// <returns>The current User object. This allows chaining.</returns>
        public RoomUnit SetPosition(FloorLocation Position)
        {
            this.fPosition = Position;
            // TODO: Force room update
            return this;
        }

        /// <summary>
        /// Returns the desired position in the room of this user.
        /// </summary>
        public FloorLocation GetDestination()
        {
            return this.fDestination;
        }

        public string GetFigure()
        {
            return this.fFigure;
        }

        public void GetDisplayName()
        {
            throw new System.NotImplementedException();
        }

        public void SetDisplayName()
        {
            throw new System.NotImplementedException();
        }
    }
}
