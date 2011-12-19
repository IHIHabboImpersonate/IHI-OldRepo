using System;

namespace IHI.Server.Habbos
{
    public class HabboEventArgs : EventArgs
    {
        public bool Cancelled
        {
            get;
            private set;
        }
        public void Cancel()
        {
            Cancelled = true;
        }
    }
}