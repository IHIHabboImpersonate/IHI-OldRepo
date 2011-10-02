using System.Collections.Generic;

namespace IHI.Server.Networking
{
    public delegate void ConnectionEventHandler(object sender, ConnectionEventArgs e);

    /// <summary>
    /// Manages accepted IonTcpConnections and enables them to interact with the Ion environment.
    /// </summary>
    public class IonTcpConnectionManager
    {
        #region Fields

        /// <summary>
        /// A 32 bit integer that holds the maximum amount of connections in the Connection manager.
        /// </summary>
// ReSharper disable UnaccessedField.Local
        private readonly int _maxSimultaneousConnections;
// ReSharper restore UnaccessedField.Local

        /// <summary>
        /// A System.Collections.Generic.Dictionary with client IDs as keys and IonTcpConnections as values. This collection holds active IonTcpConnections.
        /// </summary>
        private Dictionary<uint, IonTcpConnection> _connections;

        private IonTcpConnectionListener _listener;


        public event ConnectionEventHandler OnConnectionOpen;

        #endregion

        #region Constructors

        /// <summary>
        /// Constructs an instance of IonTcpConnectionManager, constructs an IonTcpConnectionListener, binds it to a given local IP and TCP port and sets the maximum amount of connections.
        /// </summary>
        /// <param name="localIP">The local IP address to bind the listener to.</param>
        /// <param name="port">The TCP port number to bind the listener to.</param>
        /// <param name="maxSimultaneousConnections">The maximum amount of connections in the Connection manager.</param>
        public IonTcpConnectionManager(string localIP, int port, int maxSimultaneousConnections)
        {
            var initialCapacity = maxSimultaneousConnections;
            if (maxSimultaneousConnections > 4)
                initialCapacity /= 4; // Set 1/4 of max connections as initial capacity to avoid too much resizing

            _connections = new Dictionary<uint, IonTcpConnection>(initialCapacity);
            _maxSimultaneousConnections = maxSimultaneousConnections;

            _listener = new IonTcpConnectionListener(localIP, port, this);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Destroys all resources in the Connection manager.
        /// </summary>
        public void DestroyManager()
        {
            _connections.Clear();
            _connections = null;
            _listener = null;
        }

        /// <summary>
        /// Returns true if the Connection collection currently contains a Connection with a given client ID.
        /// </summary>
        /// <param name="connectionID">The client ID to check.</param>
        public bool ContainsConnection(uint connectionID)
        {
            lock (_connections)
                return _connections.ContainsKey(connectionID);
        }

        /// <summary>
        /// Tries to return the IonTcpConnection instance of a given client ID. Null is returned if the Connection is not in the manager.
        /// </summary>
        /// <param name="connectionID">The ID of the client to get Connection of as an unsigned 32 bit integer.</param>
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
        /// Returns the IonTcpConnection listener instance.
        /// </summary>
        public IonTcpConnectionListener GetListener()
        {
            return _listener;
        }

        /// <summary>
        /// Handles a newly created IonTcpConnection and performs some checks, before adding it to the Connection collection and starting the client session.
        /// </summary>
        /// <param name="connection">The IonTcpConnection instance representing the new Connection to handle.</param>
        public void HandleNewConnection(IonTcpConnection connection)
        {
            // TODO: check max simultaneous connections
            // TODO: check max simultaneous connections per IP
            // TODO: check project specific actions

            // INFO: client ID = Connection ID, client ID = session ID
            // Add Connection to collection
            lock (_connections)
                _connections.Add(connection.GetID(), connection);

            connection.Start();

            if (OnConnectionOpen != null)
            {
                OnConnectionOpen.Invoke(connection, null);
            }

            //IonEnvironment.GetHabboHotel().GetClients().StartClient(Connection.ID);
        }

        public void DropConnection(uint clientID)
        {
            var connection = GetConnection(clientID);
            if (connection == null) return;
            CoreManager.GetServerCore().GetStandardOut().PrintNotice("Dropped Connection => " +
                                                                     connection.GetIPAddressString());

            connection.Stop();
            lock (_connections)
                _connections.Remove(clientID);
        }

        public bool TestConnection(uint clientID)
        {
            var connection = GetConnection(clientID);
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
            var connection = GetConnection(connectionID);
            if (connection == null) return;
            connection.Stop();
            _connections.Remove(connectionID);
        }
    }
}