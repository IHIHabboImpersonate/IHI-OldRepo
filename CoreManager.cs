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
namespace IHI.Server
{
    public static class CoreManager
    {
        /// <summary>
        ///   The instance of the server Core
        /// </summary>
        public static Core ServerCore { get; private set; }

        /// <summary>
        ///   The instance of the installer Core.
        /// </summary>
        public static Install.Core InstallerCore { get; private set; }

        internal static void InitialiseServerCore()
        {
            ServerCore = new Core();
        }

        internal static void InitialiseInstallerCore()
        {
            InstallerCore = new Install.Core();
        }

        internal static void DereferenceInstallerCore()
        {
            InstallerCore = null;
        }
    }
}