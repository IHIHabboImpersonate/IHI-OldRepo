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
using System;
using System.Net;
using System.Net.Sockets;

namespace IHI.Server.Networking
{
    /// <summary>
    ///   Listens for TCP connections at a given port, asynchronously accepting connections and optionally insert them in the Ion environment Connection manager.
    /// </summary>
    public class IonTcpConnectionListener
    {
        #region Fields

        private readonly AsyncCallback _connectionRequestCallback;

        /// <summary>
        ///   An IonTcpConnectionFactory instance that is capable of creating IonTcpConnections.
        /// </summary>
        private IonTcpConnectionFactory _factory;

        /// <summary>
        ///   A System.Networking.Sockets.TcpListener that listens for connections.
        /// </summary>
        private TcpListener _listener;

        private IonTcpConnectionManager _manager;

        #endregion

        #region Properties

        /// <summary>
        ///   Gets whether the listener is listening for new connections or not.
        /// </summary>
        public bool IsListening { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        ///   Constructs an IonTcpConnection listener and binds it to a given local IP address and TCP port.
        /// </summary>
        /// <param name = "localIP">The IP address string to parse and bind the listener to.</param>
        /// <param name = "port">The TCP port number to parse the listener to.</param>
        /// <param name = "manager">TODO: Document</param>
        public IonTcpConnectionListener(string localIP, int port, IonTcpConnectionManager manager)
        {
            IPAddress ip;
            if (!IPAddress.TryParse(localIP, out ip))
            {
                ip = IPAddress.Loopback;
                CoreManager.ServerCore.GetStandardOut().PrintWarning(
                    string.Format(
                        "Connection listener was unable to parse the given local IP address '{0}', now binding listener to '{1}'.",
                        localIP, ip));
            }

            _listener = new TcpListener(ip, port);
            _connectionRequestCallback = new AsyncCallback(ConnectionRequest);
            _factory = new IonTcpConnectionFactory();
            _manager = manager;

            CoreManager.ServerCore.GetStandardOut().PrintNotice(
                string.Format("IonTcpConnectionListener initialized and bound to {0}:{1}.", ip,
                              port));
        }

        #endregion

        #region Methods

        /// <summary>
        ///   Starts listening for connections.
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
        ///   Stops listening for connections.
        /// </summary>
        public void Stop()
        {
            if (!IsListening)
                return;

            IsListening = false;
            _listener.Stop();
        }

        /// <summary>
        ///   Destroys all resources in the Connection listener.
        /// </summary>
        public void Destroy()
        {
            Stop();

            _listener = null;
            _manager = null;
            _factory = null;
        }

        /// <summary>
        ///   Waits for the next Connection request in it's own thread.
        /// </summary>
        private void WaitForNextConnection()
        {
            if (IsListening)
                _listener.BeginAcceptSocket(_connectionRequestCallback, null);
        }

        /// <summary>
        ///   Invoked when the listener asynchronously accepts a new Connection request.
        /// </summary>
        /// <param name = "iAr">The IAsyncResult object holding the results of the asynchronous BeginAcceptSocket operation.</param>
        private void ConnectionRequest(IAsyncResult iAr)
        {
            try
            {
                Socket socket = _listener.EndAcceptSocket(iAr);
                // TODO: IP blacklist

                IonTcpConnection connection = _factory.CreateConnection(socket);
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