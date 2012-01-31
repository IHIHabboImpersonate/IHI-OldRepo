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
using System.Collections.Generic;

namespace IHI.Server.Networking
{
    public delegate void ConnectionEventHandler(object sender, ConnectionEventArgs e);

    /// <summary>
    ///   Manages accepted IonTcpConnections and enables them to interact with the Ion environment.
    /// </summary>
    public class IonTcpConnectionManager
    {
        #region Fields

        /// <summary>
        ///   A 32 bit integer that holds the maximum amount of connections in the Connection manager.
        /// </summary>
        /// <summary>
        ///   A System.Collections.Generic.Dictionary with client IDs as keys and IonTcpConnections as values. This collection holds active IonTcpConnections.
        /// </summary>
        private Dictionary<uint, IonTcpConnection> _connections;

        private IonTcpConnectionListener _listener;


        public event ConnectionEventHandler OnConnectionOpen;

        #endregion

        #region Constructors

        /// <summary>
        ///   Constructs an instance of IonTcpConnectionManager, constructs an IonTcpConnectionListener, binds it to a given local IP and TCP port and sets the maximum amount of connections.
        /// </summary>
        /// <param name = "localIP">The local IP address to bind the listener to.</param>
        /// <param name = "port">The TCP port number to bind the listener to.</param>
        /// <param name = "maxSimultaneousConnections">The maximum amount of connections in the Connection manager.</param>
        public IonTcpConnectionManager(string localIP, int port)
        {
            _connections = new Dictionary<uint, IonTcpConnection>();

            _listener = new IonTcpConnectionListener(localIP, port, this);
        }

        #endregion

        #region Methods

        /// <summary>
        ///   Destroys all resources in the Connection manager.
        /// </summary>
        public void DestroyManager()
        {
            _connections.Clear();
            _connections = null;
            _listener = null;
        }

        /// <summary>
        ///   Returns true if the Connection collection currently contains a Connection with a given client ID.
        /// </summary>
        /// <param name = "connectionID">The client ID to check.</param>
        public bool ContainsConnection(uint connectionID)
        {
            lock (_connections)
                return _connections.ContainsKey(connectionID);
        }

        /// <summary>
        ///   Tries to return the IonTcpConnection instance of a given client ID. Null is returned if the Connection is not in the manager.
        /// </summary>
        /// <param name = "connectionID">The ID of the client to get Connection of as an unsigned 32 bit integer.</param>
        public IonTcpConnection GetConnection(uint connectionID)
        {
            lock (_connections)
            {
                try
                {
                    return _connections[connectionID];
                }
                catch
                {
                    return null;
                }
            }
        }

        /// <summary>
        ///   Returns the IonTcpConnection listener instance.
        /// </summary>
        public IonTcpConnectionListener GetListener()
        {
            return _listener;
        }

        /// <summary>
        ///   Handles a newly created IonTcpConnection and performs some checks, before adding it to the Connection collection and starting the client session.
        /// </summary>
        /// <param name = "connection">The IonTcpConnection instance representing the new Connection to handle.</param>
        public void HandleNewConnection(IonTcpConnection connection)
        {
            // TODO: check max simultaneous connections
            // TODO: check max simultaneous connections per IP
            // TODO: check project specific actions

            // INFO: client ID = Connection ID, client ID = session ID
            // Add Connection to collection
            lock (_connections)
                _connections.Add(connection.GetID(), connection);

            if (OnConnectionOpen != null)
            {
                OnConnectionOpen.Invoke(connection, null);
            }

            connection.Start();

            //IonEnvironment.GetHabboHotel().GetClients().StartClient(Connection.ID);
        }

        public void DropConnection(uint clientID)
        {
            IonTcpConnection connection = GetConnection(clientID);
            if (connection == null) return;
            CoreManager.ServerCore.GetStandardOut().PrintNotice("Dropped Connection => " +
                                                                connection.GetIPAddressString());

            connection.Stop();
            lock (_connections)
                _connections.Remove(clientID);
        }

        public bool TestConnection(uint clientID)
        {
            IonTcpConnection connection = GetConnection(clientID);
            if (connection != null)
                return connection.TestConnection(); // Try to send data

            return false; // Connection not here!
        }

        #endregion

        public IEnumerable<IonTcpConnection> GetAllConnections()
        {
            IonTcpConnection[] returnArray;
            lock (_connections)
            {
                returnArray = new IonTcpConnection[_connections.Count];
                _connections.Values.CopyTo(returnArray, 0);
            }
            return returnArray;
        }

        internal void CloseConnection(uint connectionID)
        {
            IonTcpConnection connection = GetConnection(connectionID);
            if (connection == null) return;
            connection.Stop();
            _connections.Remove(connectionID);
        }
    }
}