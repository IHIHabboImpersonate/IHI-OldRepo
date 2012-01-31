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
using IHI.Server.Libraries.Cecer1.Messenger;

namespace IHI.Server.Habbos
{
    public delegate void MessengerBlockFlagEventHandler(object source, MessengerBlockFlagEventArgs e);

    public class MessengerBlockFlagEventArgs : EventArgs
    {
        private readonly MessengerBlock _blockFlag;
        private readonly bool _newState;
        private readonly bool _oldState;

        public MessengerBlockFlagEventArgs(MessengerBlock blockFlag, bool oldState, bool newState)
        {
            _blockFlag = blockFlag;
            _oldState = oldState;
            _newState = newState;
        }

        public MessengerBlock GetBlockFlag()
        {
            return _blockFlag;
        }

        public bool GetOldState()
        {
            return _oldState;
        }

        public bool GetNewState()
        {
            return _newState;
        }
    }
}