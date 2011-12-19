using System;

namespace IHI.Server.Install
{
    internal class InputException : Exception
    {
        private readonly string _status;

        internal InputException(string status)
        {
            _status = status;
        }

        /// <summary>
        /// Prints the Status to the Status line with red ERROR text.
        /// </summary>
        internal void Display()
        {
            CoreManager.InstallerCore.Out.SetStatus("ERROR: " + _status, ConsoleColor.Red);
        }
    }
}