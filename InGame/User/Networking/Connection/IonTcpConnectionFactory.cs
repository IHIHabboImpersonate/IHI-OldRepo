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
using System.Net.Sockets;

namespace IHI.Server.Networking
{
    /// <summary>
    ///   A factory for creating IonTcpConnections.
    /// </summary>
    public class IonTcpConnectionFactory
    {
        #region Fields

        #endregion

        #region Properties

        /// <summary>
        ///   Gets the total amount of created connections.
        /// </summary>
        public uint Count { get; private set; }

        #endregion

        #region Methods

        /// <summary>
        ///   Creates an IonTcpConnection instance for a given socket and assigns it an unique ID.
        /// </summary>
        /// <param name = "socket">The System.Networking.Socket.Sockets object to base the Connection on.</param>
        /// <returns>IonTcpConnection</returns>
        public IonTcpConnection CreateConnection(Socket socket)
        {
            if (socket == null)
                return null;

            IonTcpConnection connection = new IonTcpConnection(++Count, socket);
            CoreManager.ServerCore.GetStandardOut().PrintNotice(string.Format("Created Connection for {0}.",
                                                                              connection.GetIPAddressString()));

            return connection;
        }

        #endregion
    }
}