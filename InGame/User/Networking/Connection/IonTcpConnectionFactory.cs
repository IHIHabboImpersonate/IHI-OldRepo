using System.Net.Sockets;

namespace IHI.Server.Networking
{
    /// <summary>
    /// A factory for creating IonTcpConnections.
    /// </summary>
    public class IonTcpConnectionFactory
    {
        #region Fields

        #endregion

        #region Properties

        /// <summary>
        /// Gets the total amount of created connections.
        /// </summary>
        public uint Count { get; private set; }

        #endregion

        #region Methods

        /// <summary>
        /// Creates an IonTcpConnection instance for a given socket and assigns it an unique ID.
        /// </summary>  
        /// <param name="socket">The System.Networking.Socket.Sockets object to base the Connection on.</param>
        /// <returns>IonTcpConnection</returns>
        public IonTcpConnection CreateConnection(Socket socket)
        {
            if (socket == null)
                return null;

            var connection = new IonTcpConnection(++Count, socket);
            CoreManager.ServerCore.GetStandardOut().PrintNotice(string.Format("Created Connection for {0}.",
                                                                                   connection.GetIPAddressString()));

            return connection;
        }

        #endregion
    }
}