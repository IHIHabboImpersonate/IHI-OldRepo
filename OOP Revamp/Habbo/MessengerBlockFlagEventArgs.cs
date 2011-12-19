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