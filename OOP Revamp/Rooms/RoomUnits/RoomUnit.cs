using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace IHI.Server.Rooms
{
    public abstract class RoomUnit : IRollerable, ITalkable
    {
        protected Room fRoom;
        protected FloorPosition fPosition;
        protected FloorPosition fDestination;
        protected IFigure fFigure;

        protected string fDisplayName;

        /// <summary>
        /// Returns the current IRoom the RoomUnit is in.
        /// </summary>
        /// <returns></returns>
        public Room GetRoom()
        {
            return this.fRoom;
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
        /// Sets the desired position in the room of this user.
        /// </summary>
        /// <returns>The current User object. This allows chaining.</returns>
        public RoomUnit SetDestination(FloorPosition Destination)
        {
            this.fDestination = Destination;
            return this;
        }

        /// <summary>
        /// Returns the desired position in the room of this user.
        /// </summary>
        public FloorPosition GetDestination()
        {
            return this.fDestination;
        }

        public IFigure GetFigure()
        {
            return this.fFigure;
        }

        public string GetDisplayName()
        {
            return this.fDisplayName;
        }

        public RoomUnit SetDisplayName(string DisplayName)
        {
            this.fDisplayName = DisplayName;
            return this;
        }

        public ITalkable Shout(string Message)
        {
            throw new NotImplementedException();
            return this;
        }

        public ITalkable Whisper(ITalkable Recipient, string Message)
        {
            throw new NotImplementedException();
            return this;
        }

        public ITalkable Say(string Message)
        {
            throw new NotImplementedException();
            return this;
        }

        public IRollerable Roll(FloorPosition To)
        {
            throw new NotImplementedException();
        }

        public IRollerable Roll(FloorPosition From, FloorPosition To)
        {
            throw new NotImplementedException();
        }

        public FloorPosition GetPosition()
        {
            throw new NotImplementedException();
        }

        public RoomUnit SetPosition(FloorPosition Position)
        {
            throw new NotImplementedException();
        }
    }
}
