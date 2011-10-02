using System;

namespace IHI.Server.Install
{
    internal class InputException : Exception
    {
        private readonly string fStatus;

        internal InputException(string Status)
        {
            fStatus = Status;
        }

        /// <summary>
        /// Prints the Status to the Status line with red ERROR text.
        /// </summary>
        internal void Display()
        {
            CoreManager.GetInstallerCore().GetStandardOut().SetStatus("ERROR: " + fStatus, ConsoleColor.Red);
        }
    }
}