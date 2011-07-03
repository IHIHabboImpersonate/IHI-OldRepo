using System;
using System.Collections.Generic;

using IHI.Server;

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
        private readonly int mMaxSimultaneousConnections;
        /// <summary>
        /// A System.Collections.Generic.Dictionary with client IDs as keys and IonTcpConnections as values. This collection holds active IonTcpConnections.
        /// </summary>
        private Dictionary<uint, IonTcpConnection> mConnections;
        private IonTcpConnectionListener mListener;


        public event ConnectionEventHandler OnConnectionOpen;
        #endregion

        #region Constructors
        /// <summary>
        /// Constructs an instance of IonTcpConnectionManager, constructs an IonTcpConnectionListener, binds it to a given local IP and TCP port and sets the maximum amount of connections.
        /// </summary>
        /// <param name="sLocalIP">The local IP address to bind the listener to.</param>
        /// <param name="Port">The TCP port number to bind the listener to.</param>
        /// <param name="maxSimultaneousConnections">The maximum amount of connections in the Connection manager.</param>
        public IonTcpConnectionManager(string sLocalIP, int Port, int maxSimultaneousConnections)
        {
            int initialCapacity = maxSimultaneousConnections;
            if (maxSimultaneousConnections > 4)
                initialCapacity /= 4; // Set 1/4 of max connections as initial capacity to avoid too much resizing

            mConnections = new Dictionary<uint, IonTcpConnection>(initialCapacity);
            mMaxSimultaneousConnections = maxSimultaneousConnections;

            mListener = new IonTcpConnectionListener(sLocalIP, Port, this);
        }
        #endregion

        #region Methods
        /// <summary>
        /// Destroys all resources in the Connection manager.
        /// </summary>
        public void DestroyManager()
        {
            mConnections.Clear();
            mConnections = null;
            mListener = null;
        }

        /// <summary>
        /// Returns true if the Connection collection currently contains a Connection with a given client ID.
        /// </summary>
        /// <param name="ClientID">The client ID to check.</param>
        public bool ContainsConnection(uint ClientID)
        {
            return mConnections.ContainsKey(ClientID);
        }
        /// <summary>
        /// Tries to return the IonTcpConnection instance of a given client ID. Null is returned if the Connection is not in the manager.
        /// </summary>
        /// <param name="ID">The ID of the client to get Connection of as an unsigned 32 bit integer.</param>
        public IonTcpConnection GetConnection(uint ClientID)
        {
            try { return mConnections[ClientID]; }
            catch { return null; }
        }
        /// <summary>
        /// Returns the IonTcpConnection listener instance.
        /// </summary>
        public IonTcpConnectionListener GetListener()
        {
            return mListener;
        }

        /// <summary>
        /// Handles a newly created IonTcpConnection and performs some checks, before adding it to the Connection collection and starting the client session.
        /// </summary>
        /// <param name="Connection">The IonTcpConnection instance representing the new Connection to handle.</param>
        public void HandleNewConnection(IonTcpConnection Connection)
        {
            // TODO: check max simultaneous connections
            // TODO: check max simultaneous connections per IP
            // TODO: check project specific actions

            // INFO: client ID = Connection ID, client ID = session ID
            // Add Connection to collection
            mConnections.Add(Connection.GetID(), Connection);

            Connection.Start();

            if (this.OnConnectionOpen != null)
            {
                this.OnConnectionOpen.Invoke(Connection, null);
            }

            //IonEnvironment.GetHabboHotel().GetClients().StartClient(Connection.ID);
        }
        public void DropConnection(uint clientID)
        {
            IonTcpConnection Connection = GetConnection(clientID);
            if (Connection != null)
            {
                Core.GetStandardOut().PrintNotice("Dropped Connection => " + Connection.GetIPAddressString());
                
                Connection.Stop();
                mConnections.Remove(clientID);
            }
        }
        public bool TestConnection(uint ClientID)
        {
            IonTcpConnection connection = GetConnection(ClientID);
            if (connection != null)
                return connection.TestConnection(); // Try to send data

            return false; // Connection not here!
        }
        #endregion
    }

    public class ConnectionEventArgs : EventArgs
    {

    }
}
