using System.Net.Sockets;

using IHI.Server;

namespace IHI.Server.Networking
{
    /// <summary>
    /// A factory for creating IonTcpConnections.
    /// </summary>
    public class IonTcpConnectionFactory
    {
        #region Fields
        /// <summary>
        /// A 32 bit unsigned integer that is incremented everytime a Connection is added.
        /// </summary>
        private uint mConnectionCounter;
        #endregion

        #region Properties
        /// <summary>
        /// Gets the total amount of created connections.
        /// </summary>
        public uint Count
        {
            get { return mConnectionCounter; }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Creates an IonTcpConnection instance for a given socket and assigns it an unique ID.
        /// </summary>  
        /// <param name="Socket">The System.Networking.Socket.Sockets object to base the Connection on.</param>
        /// <returns>IonTcpConnection</returns>
        public IonTcpConnection CreateConnection(Socket Socket)
        {
            if (Socket == null)
                return null;

            IonTcpConnection Connection = new IonTcpConnection(++mConnectionCounter, Socket);
            CoreManager.GetCore().GetStandardOut().PrintNotice(string.Format("Created Connection for {0}.", Connection.GetIPAddressString()));
            
            return Connection;
        }
        #endregion
    }
}
