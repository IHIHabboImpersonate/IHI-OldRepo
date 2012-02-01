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
using System.Net;
using System.Net.Sockets;
using IHI.Server.Habbos;
using IHI.Server.Networking.Messages;
using Ion.Specialized.Encoding;
using Ion.Specialized.Utilities;

#endregion

namespace IHI.Server.Networking
{
    /// <summary>
    ///   Represents a TCP network Connection accepted by an IonTcpConnectionListener. Provides methods for sending and receiving data, aswell as disconnecting the Connection.
    /// </summary>
    public class IonTcpConnection
    {
        #region Fields

        /// <summary>
        ///   The buffer size for receiving data.
        /// </summary>
        private const int ReceivedataBufferSize = 512;

        /// <summary>
        ///   The reply to the flash policy request.
        /// </summary>
        private static readonly byte[] PolicyReplyData = new byte[]
                                                             {
                                                                 60, 63, 120, 109, 108, 32, 118, 101, 114, 115, 105, 111
                                                                 ,
                                                                 110, 61, 34, 49, 46, 48, 34, 63, 62, 13, 10, 60, 33, 68
                                                                 ,
                                                                 79, 67, 84, 89, 80, 69, 32, 99, 114, 111, 115, 115, 45,
                                                                 100, 111, 109, 97, 105, 110, 45, 112, 111, 108, 105, 99
                                                                 ,
                                                                 121, 32, 83, 89, 83, 84, 69, 77, 32, 34, 47, 120, 109,
                                                                 108
                                                                 , 47, 100, 116, 100, 115, 47, 99, 114, 111, 115, 115,
                                                                 45,
                                                                 100, 111, 109, 97, 105, 110, 45, 112, 111, 108, 105, 99
                                                                 ,
                                                                 121, 46, 100, 116, 100, 34, 62, 13, 10, 60, 99, 114,
                                                                 111,
                                                                 115, 115, 45, 100, 111, 109, 97, 105, 110, 45, 112, 111
                                                                 ,
                                                                 108, 105, 99, 121, 62, 13, 10, 60, 97, 108, 108, 111,
                                                                 119,
                                                                 45, 97, 99, 99, 101, 115, 115, 45, 102, 114, 111, 109,
                                                                 32,
                                                                 100, 111, 109, 97, 105, 110, 61, 34, 105, 109, 97, 103,
                                                                 101, 115, 46, 104, 97, 98, 98, 111, 46, 99, 111, 109,
                                                                 34,
                                                                 32, 116, 111, 45, 112, 111, 114, 116, 115, 61, 34, 49,
                                                                 45,
                                                                 53, 48, 48, 48, 48, 34, 32, 47, 62, 13, 10, 60, 97, 108
                                                                 ,
                                                                 108, 111, 119, 45, 97, 99, 99, 101, 115, 115, 45, 102,
                                                                 114
                                                                 , 111, 109, 32, 100, 111, 109, 97, 105, 110, 61, 34, 42
                                                                 ,
                                                                 34, 32, 116, 111, 45, 112, 111, 114, 116, 115, 61, 34,
                                                                 49,
                                                                 45, 53, 48, 48, 48, 48, 34, 32, 47, 62, 13, 10, 60, 47,
                                                                 99
                                                                 , 114, 111, 115, 115, 45, 100, 111, 109, 97, 105, 110,
                                                                 45,
                                                                 112, 111, 108, 105, 99, 121, 62, 0
                                                             };

        /// <summary>
        ///   A DateTime object representing the date and time this Connection was created.
        /// </summary>
        private readonly DateTime _createdAt;

        /// <summary>
        ///   The ID of this Connection as a 32 bit unsigned integer.
        /// </summary>
        private readonly uint _id;

        /// <summary>
        ///   The User that holds this Connection.
        /// </summary>
        internal Habbo Habbo;

        /// <summary>
        ///   The byte array holding the buffer for receiving data from client.
        /// </summary>
        private byte[] _dataBuffer;

        /// <summary>
        ///   The AsyncCallback instance for the thread for receiving data asynchronously.
        /// </summary>
        private AsyncCallback _dataReceivedCallback;

        private PacketHandler[,] _packetHandlers;

        /// <summary>
        ///   The RouteReceivedDataCallback to route received data to another object.
        /// </summary>
        private RouteReceivedDataCallback _routeReceivedDataCallback;

        /// <summary>
        ///   The System.Networking.Sockets.Socket object providing the Connection between client and server.
        /// </summary>
        private Socket _socket;

        #endregion

        #region Members

        private delegate void RouteReceivedDataCallback(ref byte[] data);

        #endregion

        #region API

        /// <summary>
        ///   Returns the ID of this Connection as a 32 bit unsigned integer.
        /// </summary>
        public uint GetID()
        {
            return _id;
        }

        /// <summary>
        ///   Returns the Habbo that is using this Connection.
        /// </summary>
        public Habbo GetHabbo()
        {
            return Habbo;
        }

        /// <summary>
        ///   Returns the raw binary representation of the remote IP Address in the form of a signed 32bit integer.
        /// </summary>
        public int GetIPAddressRaw()
        {
            // TODO: IPv6

            IPEndPoint remoteIP;
            if (_socket == null || (remoteIP = (_socket.RemoteEndPoint as IPEndPoint)) == null)
                return 0;

            byte[] bytes = remoteIP.Address.GetAddressBytes();

            return ((bytes[0] << 24) | (bytes[1] << 16) | (bytes[2] << 8) | bytes[3]);
        }

        /// <summary>
        ///   Returns the classical string representation of the remote IP Address.
        /// </summary>
        public string GetIPAddressString()
        {
            IPEndPoint remoteIP;
            if (_socket == null || (remoteIP = (_socket.RemoteEndPoint as IPEndPoint)) == null)
                return "";

            return remoteIP.Address.ToString();
        }

        /// <summary>
        ///   Returns the DateTime object representing the date and time this Connection was created at.
        /// </summary>
        public DateTime GetCreationDateTime()
        {
            return _createdAt;
        }

        /// <summary>
        ///   Returns the age of this Connection as a TimeSpan
        /// </summary>
        public TimeSpan GetAge()
        {
            return DateTime.Now - _createdAt;
        }

        /// <summary>
        ///   Returns true if this Connection still possesses a socket object, false otherwise.
        /// </summary>
        public bool IsAlive()
        {
            return (_socket != null);
        }

        /// <summary>
        ///   Registers a packet handler.
        /// </summary>
        /// <param name = "headerID">The numeric packet ID to register this handler on.</param>
        /// <param name = "priority">The priority this handler has.</param>
        /// <param name = "handlerDelegate">The delegate that points to the handler.</param>
        /// <returns>The current Connection. This allows chaining.</returns>
        public IonTcpConnection AddHandler(int headerID, PacketHandlerPriority priority, PacketHandler handlerDelegate)
        {
            lock (_packetHandlers)
            {
                if (headerID >= _packetHandlers.GetLength(0))
                {
                    SetHighestHeaderID(headerID + 1);
                }
            }
            if (_packetHandlers[headerID, (int) priority] != null)
                lock (_packetHandlers[headerID, (int) priority])
                    _packetHandlers[headerID, (int) priority] += handlerDelegate;
            else
                _packetHandlers[headerID, (int) priority] += handlerDelegate;

            return this;
        }

        /// <summary>
        ///   Remove a previously registered packet handler.
        /// </summary>
        /// <param name = "headerID">The numeric packet ID  of the handler.</param>
        /// <param name = "priority">The priority of the handler.</param>
        /// <param name = "handlerDelegate">The delegate that points to the handler.</param>
        /// <returns>The current Connection. This allows chaining.</returns>
        public IonTcpConnection RemoveHandler(int headerID, PacketHandlerPriority priority,
                                              PacketHandler handlerDelegate)
        {
            lock (_packetHandlers[headerID, (int) priority])
                _packetHandlers[headerID, (int) priority] -= handlerDelegate;

            lock (_packetHandlers)
                if (headerID < _packetHandlers.GetLength(0))
                    SetHighestHeaderID();
            return this;
        }

        #endregion

        #region Constructors

        /// <summary>
        ///   Constructs a new instance of IonTcpConnection for a given Connection identifier and socket.
        /// </summary>
        /// <param name = "id">The unique ID used to identify this Connection in the environment.</param>
        /// <param name = "socket">The System.Networking.Sockets.Socket of the new Connection.</param>
        public IonTcpConnection(uint id, Socket socket)
        {
            _id = id;

            _socket = socket;
            _createdAt = DateTime.Now;

            _packetHandlers = new PacketHandler[0,4];
        }

        #endregion

        #region Methods

        /// <summary>
        ///   Starts the Connection, prepares the received data buffer and waits for data.
        /// </summary>
        internal void Start()
        {
            _dataBuffer = new byte[ReceivedataBufferSize];
            _dataReceivedCallback = new AsyncCallback(DataReceived);
            _routeReceivedDataCallback = new RouteReceivedDataCallback(HandleConnectionData);


            Habbo = new Habbo(this);
            WaitForData();
        }

        /// <summary>
        ///   Stops the Connection, disconnects the socket and disposes used resources.
        /// </summary>
        internal void Stop()
        {
            if (!IsAlive())
                return; // Already stopped

            if (Habbo.IsLoggedIn())
            {
                Habbo.SetLoggedIn(false);
                CoreManager.ServerCore.GetStandardOut().PrintNotice("Connection stopped [" + GetIPAddressString() +
                                                                    ", " + Habbo.GetUsername() + ']');
            }
            else
            {
                CoreManager.ServerCore.GetStandardOut().PrintNotice("Connection stopped [" + GetIPAddressString() +
                                                                    ", UNKNOWN]");
            }
            try
            {
                _socket.Close();
            }
            catch (NullReferenceException)
            {
                throw;
            }
            _socket = null;
            _dataBuffer = null;
            _dataReceivedCallback = null;
        }

        internal bool TestConnection()
        {
            try
            {
                return _socket.Send(new byte[] {0}) > 0;
            }
            catch
            {
                return false;
            }
        }

        internal void Close()
        {
            CoreManager.ServerCore.GetConnectionManager().CloseConnection(GetID());
        }

        private void SendData(byte[] data)
        {
            if (!IsAlive())
                return;
            try
            {
                _socket.Send(data);
            }
            catch (SocketException)
            {
                Close();
            }
            catch (ObjectDisposedException)
            {
                Close();
            }
            catch (Exception ex)
            {
                CoreManager.ServerCore.GetStandardOut().PrintException(ex);
            }
        }

        internal void SendData(string data)
        {
            SendData(CoreManager.ServerCore.GetTextEncoding().GetBytes(data));
        }

        internal void SendMessage(IInternalOutgoingMessage internalMessage)
        {
            InternalOutgoingMessage message = internalMessage as InternalOutgoingMessage;
            if (Habbo.IsLoggedIn())
                CoreManager.ServerCore.GetStandardOut().PrintDebug("[" + GetHabbo().GetUsername() + "] <-- " +
                                                                   message.Header + message.GetContentString());
            else
                CoreManager.ServerCore.GetStandardOut().PrintDebug("[" + GetID() + "] <-- " + message.Header +
                                                                   message.GetContentString());

            SendData(message.GetBytes());
        }

        /// <summary>
        ///   Starts the asynchronous wait for new data.
        /// </summary>
        private void WaitForData()
        {
            if (!IsAlive())
                return;
            try
            {
                _socket.BeginReceive(_dataBuffer, 0, ReceivedataBufferSize, SocketFlags.None,
                                     _dataReceivedCallback, null);
            }
            catch (SocketException)
            {
                Close();
            }
            catch (ObjectDisposedException)
            {
                Close();
            }
            catch (Exception ex)
            {
                CoreManager.ServerCore.GetStandardOut().PrintException(ex);
                Close();
            }
        }

        private void DataReceived(IAsyncResult iAr)
        {
            // Connection not stopped yet?
            if (!IsAlive())
                return;

            // How many bytes has server received?
            int numReceivedBytes;
            try
            {
                numReceivedBytes = _socket.EndReceive(iAr);
            }
            catch (ObjectDisposedException)
            {
                Close();
                return;
            }
            catch (Exception ex)
            {
                CoreManager.ServerCore.GetStandardOut().PrintException(ex);

                Close();
                return;
            }

            if (numReceivedBytes > 0)
            {
                // Copy received data buffer
                byte[] dataToProcess = ByteUtility.ChompBytes(_dataBuffer, 0, numReceivedBytes);

                // Route data to GameClient to parse and process messages
                RouteData(ref dataToProcess);
            }

            // Wait for new data
            WaitForData();
        }

        private void HandleConnectionData(ref byte[] data)
        {
            int pos = 0;
            while (pos < data.Length)
            {
                try
                {
                    if (data[0] == 60)
                    {
                        CoreManager.ServerCore.GetStandardOut().PrintDebug("[" + _id + "] --> Policy Request");
                        SendData(PolicyReplyData);
                        CoreManager.ServerCore.GetStandardOut().PrintDebug("[" + _id + "] <-- Policy Sent");
                        Close();
                        return;
                    }

                    // Total length of message (without this): 3 Base64 bytes                    
                    int messageLength = Base64Encoding.DecodeInt32(new[] {data[pos++], data[pos++], data[pos++]});

                    // ID of message: 2 Base64 bytes
                    uint messageID = Base64Encoding.DecodeUInt32(new[] {data[pos++], data[pos++]});

                    // Data of message: (messageLength - 2) bytes
                    byte[] content = new byte[messageLength - 2];
                    for (int i = 0; i < content.Length; i++)
                    {
                        content[i] = data[pos++];
                    }

                    // Create message object
                    IncomingMessage message = new IncomingMessage(messageID, content);

                    if (Habbo.IsLoggedIn())
                        CoreManager.ServerCore.GetStandardOut().PrintDebug("[" + Habbo.GetUsername() + "] --> " +
                                                                           message.GetHeader() +
                                                                           message.GetContentString());
                    else
                        CoreManager.ServerCore.GetStandardOut().PrintDebug("[" + _id + "] --> " +
                                                                           message.GetHeader() +
                                                                           message.GetContentString());


                    // Handle message object
                    bool unknown = true;

                    if (_packetHandlers.GetLength(0) > messageID)
                    {
                        if (_packetHandlers[messageID, 3] != null)
                        {
                            lock (_packetHandlers[messageID, 3])
                            {
                                _packetHandlers[messageID, 3].Invoke(Habbo, message); // Execute High Priority
                                unknown = false;
                            }
                        }

                        if (message.IsCancelled())
                            return;

                        if (_packetHandlers[messageID, 2] != null)
                        {
                            lock (_packetHandlers[messageID, 2])
                            {
                                _packetHandlers[messageID, 2].Invoke(Habbo, message); // Execute Low Priority
                                unknown = false;
                            }
                        }

                        if (message.IsCancelled())
                            return;

                        if (_packetHandlers[messageID, 1] != null)
                        {
                            lock (_packetHandlers[messageID, 1])
                            {
                                _packetHandlers[messageID, 1].Invoke(Habbo, message); // Execute Default Action
                                unknown = false;
                            }
                        }

                        if (_packetHandlers[messageID, 0] != null)
                        {
                            lock (_packetHandlers[messageID, 0])
                            {
                                _packetHandlers[messageID, 0].Invoke(Habbo, message); // Execute Watchers
                                unknown = false;
                            }
                        }
                    }

                    if (unknown)
                    {
                        CoreManager.ServerCore.GetStandardOut().PrintWarning("Packet " + messageID + " ('" +
                                                                             message.GetHeader() + "') unhandled!");
                    }
                }
                catch (IndexOutOfRangeException) // Bad formatting!
                {
                    // TODO: Move this to IHI
                    //IonEnvironment.GetHabboHotel().GetClients().StopClient(_id, 0);
                }
                catch (Exception ex)
                {
                    CoreManager.ServerCore.GetStandardOut().PrintException(ex);
                }
            }
        }

        /// <summary>
        ///   Routes a byte array passed as reference to another object.
        /// </summary>
        /// <param name = "data">The byte array to route.</param>
        private void RouteData(ref byte[] data)
        {
            if (_routeReceivedDataCallback != null)
            {
                _routeReceivedDataCallback.Invoke(ref data);
            }
        }

        #endregion

        public void Disconnect()
        {
            CoreManager.ServerCore.GetConnectionManager().CloseConnection(GetID());
            Stop();
        }

        private void SetHighestHeaderID(int headerID)
        {
            lock (_packetHandlers)
            {
                PacketHandler[,] array = new PacketHandler[headerID + 1,4];
                if (headerID < _packetHandlers.GetLength(0))
                    Array.Copy(_packetHandlers, array, headerID);
                else
                    Array.Copy(_packetHandlers, array, _packetHandlers.GetLength(0));
                _packetHandlers = array;
            }
        }

        private void SetHighestHeaderID()
        {
            lock (_packetHandlers)
            {
                int headerID = _packetHandlers.GetLength(0);
                while (
                    _packetHandlers[headerID, 0] == null &&
                    _packetHandlers[headerID, 1] == null &&
                    _packetHandlers[headerID, 2] == null &&
                    _packetHandlers[headerID, 3] == null)
                    headerID--;

                SetHighestHeaderID(headerID);
            }
        }
    }
}