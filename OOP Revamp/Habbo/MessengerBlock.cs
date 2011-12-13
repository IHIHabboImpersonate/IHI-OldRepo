using System;

namespace IHI.Server.Libraries.Cecer1.Messenger
{
    [Flags]
    public enum MessengerBlock
    {
        None = 0,
        Stalk = 1,
        Request = 2,
        Invite = 4
    }
}