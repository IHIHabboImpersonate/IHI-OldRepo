// 
// Copyright (C) 2012  Chris Chenery
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
using System;
using System.Collections.Generic;

namespace IHI.Server.Networking.Messages
{
    public abstract class OutgoingMessage
    {
        protected readonly IInternalOutgoingMessage InternalOutgoingMessage = new InternalOutgoingMessage();

        public abstract OutgoingMessage Send(IMessageable target);

        public OutgoingMessage Send(IEnumerable<IMessageable> targets, bool sendOncePerConnection = false)
        {
            if (sendOncePerConnection)
                throw new NotImplementedException();
            foreach (IMessageable target in targets)
            {
                Send(target);
            }
            return this;
        }
    }
}