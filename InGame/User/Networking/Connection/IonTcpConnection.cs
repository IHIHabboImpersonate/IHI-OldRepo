﻿using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using IHI.Server.Habbos;
using IHI.Server.Networking.Messages;
using Ion.Specialized.Encoding;
using Ion.Specialized.Utilities;

namespace IHI.Server.Networking
{
    /// <summary>
    /// Represents a TCP network Connection accepted by an IonTcpConnectionListener. Provides methods for sending and receiving data, aswell as disconnecting the Connection.
    /// </summary>
    public class IonTcpConnection
    {
        #region Fields

        /// <summary>
        /// The buffer size for receiving data.
        /// </summary>
        private const int ReceivedataBufferSize = 512;

        /// <summary>
        /// The reply to the flash policy request.
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
        /// The amount of milliseconds to sleep when receiving data before processing the message. When this constant is 0, the data will be processed immediately.
        /// </summary>
        private static int _receivedataMillisecondsDelay;

        /// <summary>
        /// A DateTime object representing the date and time this Connection was created.
        /// </summary>
        private readonly DateTime _createdAt;

        /// <summary>
        /// The ID of this Connection as a 32 bit unsigned integer.
        /// </summary>
        private readonly uint _id;

        private readonly PacketHandler[,] _packetHandlers;

        /// <summary>
        /// The byte array holding the buffer for receiving data from client.
        /// </summary>
        private byte[] _dataBuffer;

        /// <summary>
        /// The AsyncCallback instance for the thread for receiving data asynchronously.
        /// </summary>
        private AsyncCallback _dataReceivedCallback;

        /// <summary>
        /// The RouteReceivedDataCallback to route received data to another object.
        /// </summary>
        private RouteReceivedDataCallback _routeReceivedDataCallback;

        /// <summary>
        /// The System.Networking.Sockets.Socket object providing the Connection between client and server.
        /// </summary>
        private Socket _socket;

        /// <summary>
        /// The User that holds this Connection.
        /// </summary>
        internal Habbo Habbo;

        #endregion

        #region Members

        private delegate void RouteReceivedDataCallback(ref byte[] data);

        #endregion

        #region API

        /// <summary>
        /// Returns the ID of this Connection as a 32 bit unsigned integer.
        /// </summary>
        public uint GetID()
        {
            return _id;
        }

        /// <summary>
        /// Returns the Habbo that is using this Connection.
        /// </summary>
        public Habbo GetHabbo()
        {
            return Habbo;
        }

        /// <summary>
        /// Returns the raw binary representation of the remote IP Address in the form of a signed 32bit integer.
        /// </summary>
        public int GetIPAddressRaw()
        {
            // TODO: IPv6

            IPEndPoint remoteIP;
            if (_socket == null || (remoteIP = (_socket.RemoteEndPoint as IPEndPoint)) == null)
                return 0;

            var bytes = remoteIP.Address.GetAddressBytes();

            return ((bytes[0] << 24) | (bytes[1] << 16) | (bytes[2] << 8) | bytes[3]);
        }

        /// <summary>
        /// Returns the classical string representation of the remote IP Address.
        /// </summary>
        public string GetIPAddressString()
        {
            IPEndPoint remoteIP;
            if (_socket == null || (remoteIP = (_socket.RemoteEndPoint as IPEndPoint)) == null)
                return "";

            return remoteIP.Address.ToString();
        }

        /// <summary>
        /// Returns the DateTime object representing the date and time this Connection was created at.
        /// </summary>
        public DateTime GetCreationDateTime()
        {
            return _createdAt;
        }

        /// <summary>
        /// Returns the age of this Connection as a TimeSpan
        /// </summary>
        public TimeSpan GetAge()
        {
            return DateTime.Now - _createdAt;
        }

        /// <summary>
        /// Returns true if this Connection still possesses a socket object, false otherwise.
        /// </summary>
        public bool IsAlive()
        {
            return (_socket != null);
        }

        /// <summary>
        /// Registers a packet handler.
        /// </summary>
        /// <param name="headerID">The numeric packet ID to register this handler on.</param>
        /// <param name="priority">The priority this handler has.</param>
        /// <param name="handlerDelegate">The delegate that points to the handler.</param>
        /// <returns>The current Connection. This allows chaining.</returns>
        public IonTcpConnection AddHandler(uint headerID, PacketHandlerPriority priority, PacketHandler handlerDelegate)
        {
            if (_packetHandlers[headerID, (int) priority] != null)
                lock (_packetHandlers[headerID, (int) priority])
                    _packetHandlers[headerID, (int) priority] += handlerDelegate;
            else
                _packetHandlers[headerID, (int) priority] += handlerDelegate;
            return this;
        }

        /// <summary>
        /// Remove a previously registered packet handler.
        /// </summary>
        /// <param name="headerID">The numeric packet ID  of the handler.</param>
        /// <param name="priority">The priority of the handler.</param>
        /// <param name="handlerDelegate">The delegate that points to the handler.</param>
        /// <returns>The current Connection. This allows chaining.</returns>
        public IonTcpConnection RemoveHandler(uint headerID, PacketHandlerPriority priority,
                                              PacketHandler handlerDelegate)
        {
            lock (_packetHandlers[headerID, (int) priority])
                _packetHandlers[headerID, (int) priority] -= handlerDelegate;
            return this;
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructs a new instance of IonTcpConnection for a given Connection identifier and socket.
        /// </summary>
        /// <param name="id">The unique ID used to identify this Connection in the environment.</param>
        /// <param name="socket">The System.Networking.Sockets.Socket of the new Connection.</param>
        public IonTcpConnection(uint id, Socket socket)
        {
            _id = id;
            _socket = socket;
            _createdAt = DateTime.Now;

            _packetHandlers = new PacketHandler[2002 + 1,4];
            // "2002" copied from Ion.HabboHotel.Client.ClientMessageHandler.HIGHEST_MESSAGEID
        }

        #endregion

        #region Methods

        /// <summary>
        /// Starts the Connection, prepares the received data buffer and waits for data.
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
        /// Stops the Connection, disconnects the socket and disposes used resources.
        /// </summary>
        internal void Stop()
        {
            if (!IsAlive())
                return; // Already stopped

            if (Habbo.IsLoggedIn())
            {
                Habbo.SetLoggedIn(false);
                CoreManager.GetServerCore().GetStandardOut().PrintNotice("Connection stopped [" + GetIPAddressString() +
                                                                         ", " + Habbo.GetUsername() + ']');
            }
            else
            {
                CoreManager.GetServerCore().GetStandardOut().PrintNotice("Connection stopped [" + GetIPAddressString() +
                                                                         ", UNKNOWN]");
            }

            _socket.Close();
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
            CoreManager.GetServerCore().GetConnectionManager().CloseConnection(GetID());
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
                CoreManager.GetServerCore().GetStandardOut().PrintException(ex);
            }
        }

        internal void SendData(string data)
        {
            SendData(CoreManager.GetServerCore().GetTextEncoding().GetBytes(data));
        }

        internal void SendMessage(IInternalOutgoingMessage internalMessage)
        {
            var message = internalMessage as InternalOutgoingMessage;
            if (Habbo.IsLoggedIn())
                CoreManager.GetServerCore().GetStandardOut().PrintDebug("[" + GetHabbo().GetUsername() + "] <-- " +
                                                                        message.Header + message.GetContentString());
            else
                CoreManager.GetServerCore().GetStandardOut().PrintDebug("[" + GetID() + "] <-- " + message.Header +
                                                                        message.GetContentString());

            SendData(message.GetBytes());
        }

        /// <summary>
        /// Starts the asynchronous wait for new data.
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
                CoreManager.GetServerCore().GetStandardOut().PrintException(ex);
                Close();
            }
        }

        private void DataReceived(IAsyncResult iAr)
        {
            // Connection not stopped yet?
            if (!IsAlive())
                return;

            // Do an optional wait before processing the data
            if (_receivedataMillisecondsDelay > 0)
                Thread.Sleep(_receivedataMillisecondsDelay);

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
                CoreManager.GetServerCore().GetStandardOut().PrintException(ex);

                Close();
                return;
            }

            if (numReceivedBytes > 0)
            {
                // Copy received data buffer
                var dataToProcess = ByteUtility.ChompBytes(_dataBuffer, 0, numReceivedBytes);

                // Route data to GameClient to parse and process messages
                RouteData(ref dataToProcess);
            }

            // Wait for new data
            WaitForData();
        }

        private void HandleConnectionData(ref byte[] data)
        {
            var pos = 0;
            while (pos < data.Length)
            {
                try
                {
                    if (data[0] == 60)
                    {
                        CoreManager.GetServerCore().GetStandardOut().PrintDebug("[" + _id + "] --> Policy Request");
                        SendData(PolicyReplyData);
                        CoreManager.GetServerCore().GetStandardOut().PrintDebug("[" + _id + "] <-- Policy Sent");
                        Close();
                        return;
                    }

                    // Total length of message (without this): 3 Base64 bytes                    
                    var messageLength = Base64Encoding.DecodeInt32(new[] {data[pos++], data[pos++], data[pos++]});

                    // ID of message: 2 Base64 bytes
                    var messageID = Base64Encoding.DecodeUInt32(new[] {data[pos++], data[pos++]});

                    // Data of message: (messageLength - 2) bytes
                    var content = new byte[messageLength - 2];
                    for (var i = 0; i < content.Length; i++)
                    {
                        content[i] = data[pos++];
                    }

                    // Create message object
                    var message = new IncomingMessage(messageID, content);

                    if (Habbo.IsLoggedIn())
                        CoreManager.GetServerCore().GetStandardOut().PrintDebug("[" + Habbo.GetUsername() + "] --> " +
                                                                                message.GetHeader() +
                                                                                message.GetContentString());
                    else
                        CoreManager.GetServerCore().GetStandardOut().PrintDebug("[" + _id + "] --> " +
                                                                                message.GetHeader() +
                                                                                message.GetContentString());


                    // Handle message object
                    var unknown = true;

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

                    if (unknown)
                    {
                        CoreManager.GetServerCore().GetStandardOut().PrintWarning("Packet " + messageID + " ('" +
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
                    CoreManager.GetServerCore().GetStandardOut().PrintException(ex);
                }
            }
        }

        /// <summary>
        /// Routes a byte array passed as reference to another object.
        /// </summary>
        /// <param name="data">The byte array to route.</param>
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
            CoreManager.GetServerCore().GetConnectionManager().CloseConnection(GetID());
            Stop();
        }
    }
}