#region GPLv3

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
// 

#endregion

#region Usings

using System;

#endregion

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
        ///   Prints the Status to the Status line with red ERROR text.
        /// </summary>
        internal void Display()
        {
            CoreManager.InstallerCore.Out.SetStatus("ERROR: " + _status, ConsoleColor.Red);
        }
    }
}