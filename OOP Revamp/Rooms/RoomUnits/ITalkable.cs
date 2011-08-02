using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IHI.Server.Rooms
{
    // TODO: Update method comments
    public interface ITalkable
    {
        /// <summary>
        /// Shout a message from the User to all Users in the room.
        /// </summary>
        /// <param name="Message">The message to send.</param>
        /// <returns>The current User object. This allows chaining.</returns>
        ITalkable Shout(string Message);

        /// <summary>
        /// Whisper a message from the User to another User in the room.
        /// </summary>
        /// <param name="Recipient">The User to recieve the message.</param>
        /// <param name="Message">The message to send.</param>
        /// <returns>The current User object. This allows chaining.</returns>
        ITalkable Whisper(ITalkable Recipient, string Message);

        /// <summary>
        /// Say a message from the User to close Users in the room.
        /// </summary>
        /// <param name="Message">The message to send.</param>
        /// <returns>The current User object. This allows chaining.</returns>
        ITalkable Say(string Message);
    }
}
