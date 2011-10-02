using System;
using System.Net;
using System.Net.Sockets;

namespace IHI.Server.Networking
{
    /// <summary>
    /// Listens for TCP connections at a given port, asynchronously accepting connections and optionally insert them in the Ion environment Connection manager.
    /// </summary>
    public class IonTcpConnectionListener
    {
        #region Fields

        /// <summary>
        /// The maximum length of the Connection request queue for the listener as an integer.
        /// </summary>
        private const int ListenerConnectionrequestQueueLength = 1;

        private readonly AsyncCallback _connectionRequestCallback;

        /// <summary>
        /// An IonTcpConnectionFactory instance that is capable of creating IonTcpConnections.
        /// </summary>
        private IonTcpConnectionFactory _factory;

        /// <summary>
        /// A System.Networking.Sockets.TcpListener that listens for connections.
        /// </summary>
        private TcpListener _listener;

        private IonTcpConnectionManager _manager;

        #endregion

        #region Properties

        /// <summary>
        /// Gets whether the listener is listening for new connections or not.
        /// </summary>
        public bool IsListening { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructs an IonTcpConnection listener and binds it to a given local IP address and TCP port.
        /// </summary>
        /// <param name="localIP">The IP address string to parse and bind the listener to.</param>
        /// <param name="port">The TCP port number to parse the listener to.</param>
        /// <param name="manager">TODO: Document</param>
        public IonTcpConnectionListener(string localIP, int port, IonTcpConnectionManager manager)
        {
            IPAddress ip;
            if (!IPAddress.TryParse(localIP, out ip))
            {
                ip = IPAddress.Loopback;
                CoreManager.GetServerCore().GetStandardOut().PrintWarning(
                    string.Format(
                        "Connection listener was unable to parse the given local IP address '{0}', now binding listener to '{1}'.",
                        localIP, ip.ToString()));
            }

            _listener = new TcpListener(ip, port);
            _connectionRequestCallback = new AsyncCallback(ConnectionRequest);
            _factory = new IonTcpConnectionFactory();
            _manager = manager;

            CoreManager.GetServerCore().GetStandardOut().PrintNotice(
                string.Format("IonTcpConnectionListener initialized and bound to {0}:{1}.", ip.ToString(),
                              port.ToString()));
        }

        #endregion

        #region Methods

        /// <summary>
        /// Starts listening for connections.
        /// </summary>
        public void Start()
        {
            if (IsListening)
                return;

            _listener.Start();
            IsListening = true;

            WaitForNextConnection();
        }

        /// <summary>
        /// Stops listening for connections.
        /// </summary>
        public void Stop()
        {
            if (!IsListening)
                return;

            IsListening = false;
            _listener.Stop();
        }

        /// <summary>
        /// Destroys all resources in the Connection listener.
        /// </summary>
        public void Destroy()
        {
            Stop();

            _listener = null;
            _manager = null;
            _factory = null;
        }

        /// <summary>
        /// Waits for the next Connection request in it's own thread.
        /// </summary>
        private void WaitForNextConnection()
        {
            if (IsListening)
                _listener.BeginAcceptSocket(_connectionRequestCallback, null);
        }

        /// <summary>
        /// Invoked when the listener asynchronously accepts a new Connection request.
        /// </summary>
        /// <param name="iAr">The IAsyncResult object holding the results of the asynchronous BeginAcceptSocket operation.</param>
        private void ConnectionRequest(IAsyncResult iAr)
        {
            try
            {
                var socket = _listener.EndAcceptSocket(iAr);
                // TODO: IP blacklist

                var connection = _factory.CreateConnection(socket);
                if (connection != null)
                {
                    _manager.HandleNewConnection(connection);
                }
            }
// ReSharper disable EmptyGeneralCatchClause
            catch
            {
            } // TODO: handle exceptions
// ReSharper restore EmptyGeneralCatchClause
            finally
            {
                if (IsListening)
                    WaitForNextConnection(); // Re-start the process for next Connection
            }
        }

        #endregion
    }
}