using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IHI.Server.Install
{
    internal class InputException : Exception
    {
        private string fStatus;

        internal InputException(string Status)
        {
            this.fStatus = Status;
        }

        /// <summary>
        /// Prints the Status to the Status line with red ERROR text.
        /// </summary>
        internal void Display()
        {
            Output.SetStatus("ERROR: " + this.fStatus, ConsoleColor.Red);
        }
    }
}
