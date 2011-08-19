using System;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using IHI.Server;
using IHI.Server.Networking.Messages;
using IHI.Server.Habbos;
using Ion.Specialized.Encoding;
using Ion.Specialized.Utilities;
using IHI.Server.Networking;

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
        private const int RECEIVEDATA_BUFFER_SIZE = 512;
        /// <summary>
        /// The reply to the flash policy request.
        /// </summary>
        private static readonly byte[] PolicyReplyData = new byte[] { 60, 63, 120, 109, 108, 32, 118, 101, 114, 115, 105, 111, 110, 61, 34, 49, 46, 48, 34, 63, 62, 13, 10, 60, 33, 68, 79, 67, 84, 89, 80, 69, 32, 99, 114, 111, 115, 115, 45, 100, 111, 109, 97, 105, 110, 45, 112, 111, 108, 105, 99, 121, 32, 83, 89, 83, 84, 69, 77, 32, 34, 47, 120, 109, 108, 47, 100, 116, 100, 115, 47, 99, 114, 111, 115, 115, 45, 100, 111, 109, 97, 105, 110, 45, 112, 111, 108, 105, 99, 121, 46, 100, 116, 100, 34, 62, 13, 10, 60, 99, 114, 111, 115, 115, 45, 100, 111, 109, 97, 105, 110, 45, 112, 111, 108, 105, 99, 121, 62, 13, 10, 60, 97, 108, 108, 111, 119, 45, 97, 99, 99, 101, 115, 115, 45, 102, 114, 111, 109, 32, 100, 111, 109, 97, 105, 110, 61, 34, 105, 109, 97, 103, 101, 115, 46, 104, 97, 98, 98, 111, 46, 99, 111, 109, 34, 32, 116, 111, 45, 112, 111, 114, 116, 115, 61, 34, 49, 45, 53, 48, 48, 48, 48, 34, 32, 47, 62, 13, 10, 60, 97, 108, 108, 111, 119, 45, 97, 99, 99, 101, 115, 115, 45, 102, 114, 111, 109, 32, 100, 111, 109, 97, 105, 110, 61, 34, 42, 34, 32, 116, 111, 45, 112, 111, 114, 116, 115, 61, 34, 49, 45, 53, 48, 48, 48, 48, 34, 32, 47, 62, 13, 10, 60, 47, 99, 114, 111, 115, 115, 45, 100, 111, 109, 97, 105, 110, 45, 112, 111, 108, 105, 99, 121, 62, 0 };
        /// <summary>
        /// The amount of milliseconds to sleep when receiving data before processing the message. When this constant is 0, the data will be processed immediately.
        /// </summary>
        private static int RECEIVEDATA_MILLISECONDS_DELAY = 0;
        
        /// <summary>
        /// The ID of this Connection as a 32 bit unsigned integer.
        /// </summary>
        private readonly uint fID;
        /// <summary>
        /// A DateTime object representing the date and time this Connection was created.
        /// </summary>
        private readonly DateTime fCreatedAt;
       
        /// <summary>
        /// The System.Networking.Sockets.Socket object providing the Connection between client and server.
        /// </summary>
        private Socket fSocket = null;

        /// <summary>
        /// The byte array holding the buffer for receiving data from client.
        /// </summary>
        private byte[] fDataBuffer = null;
        /// <summary>
        /// The AsyncCallback instance for the thread for receiving data asynchronously.
        /// </summary>
        private AsyncCallback fDataReceivedCallback;
        /// <summary>
        /// The RouteReceivedDataCallback to route received data to another object.
        /// </summary>
        private RouteReceivedDataCallback fRouteReceivedDataCallback;
        /// <summary>
        /// The User that holds this Connection.
        /// </summary>
        internal Habbo fUser;

        private PacketHandler[,] fPacketHandlers;
        #endregion

        #region Members
        public delegate void RouteReceivedDataCallback(ref byte[] Data);
        #endregion

        #region API
        /// <summary>
        /// Returns the ID of this Connection as a 32 bit unsigned integer.
        /// </summary>
        public uint GetID()
        {
            return this.fID;
        }
        /// <summary>
        /// Returns the Habbo that is using this Connection.
        /// </summary>
        public Habbo GetHabbo()
        {
            return this.fUser;
        }
        /// <summary>
        /// Returns the raw binary representation of the remote IP Address in the form of a signed 32bit integer.
        /// </summary>
        public int GetIPAddressRaw()
        {
            // TODO: IPv6

            if (fSocket == null)
                return 0;

            byte[] Bytes = (fSocket.RemoteEndPoint as IPEndPoint).Address.GetAddressBytes();

            return ((Bytes[0] << 24) | (Bytes[1] << 16) | (Bytes[2] << 8) | Bytes[3]);
        }
        /// <summary>
        /// Returns the classical string representation of the remote IP Address.
        /// </summary>
        public string GetIPAddressString()
        {
            if (fSocket == null)
                return string.Empty;

            return (fSocket.RemoteEndPoint as IPEndPoint).Address.ToString();
        }
        /// <summary>
        /// Returns the DateTime object representing the date and time this Connection was created at.
        /// </summary>
        public DateTime GetCreationDateTime()
        {
            return this.fCreatedAt;
        }
        /// <summary>
        /// Returns the age of this Connection as a TimeSpan
        /// </summary>
        public TimeSpan GetAge()
        {
            return DateTime.Now - fCreatedAt;
        }
        /// <summary>
        /// Returns true if this Connection still possesses a socket object, false otherwise.
        /// </summary>
        public bool IsAlive()
        {
            return (fSocket != null);
        }

        /// <summary>
        /// Registers a packet handler.
        /// </summary>
        /// <param name="HeaderID">The numeric packet ID to register this handler on.</param>
        /// <param name="Priority">The priority this handler has.</param>
        /// <param name="HandlerDelegate">The delegate that points to the handler.</param>
        /// <returns>The current Connection. This allows chaining.</returns>
        public IonTcpConnection AddHandler(uint HeaderID, PacketHandlerPriority Priority, PacketHandler HandlerDelegate)
        {
            if (this.fPacketHandlers[HeaderID, (int)Priority] != null)
                lock (this.fPacketHandlers[HeaderID, (int)Priority])
                    this.fPacketHandlers[HeaderID, (int)Priority] += HandlerDelegate;
            else
                this.fPacketHandlers[HeaderID, (int)Priority] += HandlerDelegate;
            return this;
        }
        /// <summary>
        /// Remove a previously registered packet handler.
        /// </summary>
        /// <param name="HeaderID">The numeric packet ID  of the handler.</param>
        /// <param name="Priority">The priority of the handler.</param>
        /// <param name="HandlerDelegate">The delegate that points to the handler.</param>
        /// <returns>The current Connection. This allows chaining.</returns>
        public IonTcpConnection RemoveHandler(uint HeaderID, PacketHandlerPriority Priority, PacketHandler HandlerDelegate)
        {
            lock (this.fPacketHandlers[HeaderID, (int)Priority])
                this.fPacketHandlers[HeaderID, (int)Priority] -= HandlerDelegate;
            return this;
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Constructs a new instance of IonTcpConnection for a given Connection identifier and socket.
        /// </summary>
        /// <param name="ID">The unique ID used to identify this Connection in the environment.</param>
        /// <param name="Socket">The System.Networking.Sockets.Socket of the new Connection.</param>
        public IonTcpConnection(uint ID, Socket pSocket)
        {
            fID = ID;
            fSocket = pSocket;
            fCreatedAt = DateTime.Now;

            this.fPacketHandlers = new PacketHandler[2002 + 1, 4]; // "2002" copied from Ion.HabboHotel.Client.ClientMessageHandler.HIGHEST_MESSAGEID
        }
        #endregion

        #region Methods
        /// <summary>
        /// Starts the Connection, prepares the received data buffer and waits for data.
        /// </summary>
        internal void Start()
        {

            fDataBuffer = new byte[RECEIVEDATA_BUFFER_SIZE];
            fDataReceivedCallback = new AsyncCallback(DataReceived);
            fRouteReceivedDataCallback = new IonTcpConnection.RouteReceivedDataCallback(HandleConnectionData);


            this.fUser = new Habbo(this);
            WaitForData();
        }
        /// <summary>
        /// Stops the Connection, disconnects the socket and disposes used resources.
        /// </summary>
        internal void Stop()
        {
            if (!IsAlive())
                return; // Already stopped

            if (this.fUser.IsLoggedIn())
            {
                this.fUser.SetLoggedIn(false);
                CoreManager.GetCore().GetStandardOut().PrintNotice("Connection stopped [" + this.GetIPAddressString() + ", " + this.fUser.GetUsername() + ']');
            }
            else
            {
                CoreManager.GetCore().GetStandardOut().PrintNotice("Connection stopped [" + this.GetIPAddressString() + ", UNKNOWN]");
            }

            fSocket.Close();
            fSocket = null;
            fDataBuffer = null;
            fDataReceivedCallback = null;
        }
        internal bool TestConnection()
        {
            try
            {
                return fSocket.Send(new byte[] { 0 }) > 0;
            }
            catch { }

            return false;
        }
        internal void Close()
        {
            CoreManager.GetCore().GetConnectionManager().CloseConnection(this.GetID());
        }

        internal void SendData(byte[] Data)
        {
            if (IsAlive())
            {
                try
                {
                    fSocket.Send(Data);
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
                    CoreManager.GetCore().GetStandardOut().PrintException(ex);
                }
            }
        }
        internal void SendData(string sData)
        {
            SendData(CoreManager.GetCore().GetTextEncoding().GetBytes(sData));
        }

        internal void SendMessage(IInternalOutgoingMessage Imessage)
        {
            InternalOutgoingMessage message = Imessage as InternalOutgoingMessage;
            if (this.fUser.IsLoggedIn())
                CoreManager.GetCore().GetStandardOut().PrintDebug("[" + GetHabbo().GetUsername() + "] <-- " + message.Header + message.GetContentString());
            else
                CoreManager.GetCore().GetStandardOut().PrintDebug("[" + GetID() + "] <-- " + message.Header + message.GetContentString());

            SendData(message.GetBytes());
        }

        /// <summary>
        /// Starts the asynchronous wait for new data.
        /// </summary>
        private void WaitForData()
        {
            if (this.IsAlive())
            {
                try
                {
                    fSocket.BeginReceive(fDataBuffer, 0, RECEIVEDATA_BUFFER_SIZE, SocketFlags.None, fDataReceivedCallback, null);
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
                    CoreManager.GetCore().GetStandardOut().PrintException(ex);
                    Close();
                }
            }
        }
        private void DataReceived(IAsyncResult iAr)
        {
            // Connection not stopped yet?
            if (!IsAlive())
                return;

            // Do an optional wait before processing the data
            if (RECEIVEDATA_MILLISECONDS_DELAY > 0)
                Thread.Sleep(RECEIVEDATA_MILLISECONDS_DELAY);

            // How many bytes has server received?
            int numReceivedBytes = 0;
            try
            {
                numReceivedBytes = fSocket.EndReceive(iAr);
            }
            catch (ObjectDisposedException)
            {
                Close();
                return;
            }
            catch (Exception ex)
            {
                CoreManager.GetCore().GetStandardOut().PrintException(ex);
                
                Close();
                return;
            }

            if (numReceivedBytes > 0)
            {
                // Copy received data buffer
                byte[] dataToProcess = ByteUtility.ChompBytes(fDataBuffer, 0, numReceivedBytes);

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
                        CoreManager.GetCore().GetStandardOut().PrintDebug("[" + this.fID + "] --> Policy Request");
                        this.SendData(PolicyReplyData);
                        CoreManager.GetCore().GetStandardOut().PrintDebug("[" + this.fID + "] <-- Policy Sent");
                        this.Close();
                        return;
                    }

                    // Total length of message (without this): 3 Base64 bytes                    
                    int messageLength = Base64Encoding.DecodeInt32(new byte[] { data[pos++], data[pos++], data[pos++] });

                    // ID of message: 2 Base64 bytes
                    uint messageID = Base64Encoding.DecodeUInt32(new byte[] { data[pos++], data[pos++] });

                    // Data of message: (messageLength - 2) bytes
                    byte[] Content = new byte[messageLength - 2];
                    for (int i = 0; i < Content.Length; i++)
                    {
                        Content[i] = data[pos++];
                    }

                    // Create message object
                    IncomingMessage message = new IncomingMessage(messageID, Content);

                    if (this.fUser.IsLoggedIn())
                        CoreManager.GetCore().GetStandardOut().PrintDebug("[" + this.fUser.GetUsername() + "] --> " + message.GetHeader() + message.GetContentString());
                    else
                        CoreManager.GetCore().GetStandardOut().PrintDebug("[" + this.fID + "] --> " + message.GetHeader() + message.GetContentString());


                    // Handle message object
                    bool Unknown = true;

                    if (this.fPacketHandlers[messageID, 3] != null)
                    {
                        lock (this.fPacketHandlers[messageID, 3])
                        {
                            this.fPacketHandlers[messageID, 3].Invoke(this.fUser, message); // Execute High Priority
                            Unknown = false;
                        }
                    }

                    if (message.IsCancelled())
                        return;

                    if (this.fPacketHandlers[messageID, 2] != null)
                    {
                        lock (this.fPacketHandlers[messageID, 2])
                        {
                            this.fPacketHandlers[messageID, 2].Invoke(this.fUser, message); // Execute Low Priority
                            Unknown = false;
                        }
                    }

                    if (message.IsCancelled())
                        return;

                    if (this.fPacketHandlers[messageID, 1] != null)
                    {
                        lock (this.fPacketHandlers[messageID, 1])
                        {
                            this.fPacketHandlers[messageID, 1].Invoke(this.fUser, message); // Execute Default Action
                            Unknown = false;
                        }
                    }

                    if (this.fPacketHandlers[messageID, 0] != null)
                    {
                        lock (this.fPacketHandlers[messageID, 0])
                        {
                            this.fPacketHandlers[messageID, 0].Invoke(this.fUser, message); // Execute Watchers
                            Unknown = false;
                        }
                    }

                    if (Unknown)
                    {
                        CoreManager.GetCore().GetStandardOut().PrintWarning("Packet " + messageID + " ('" + message.GetHeader() + "') unhandled!");
                    }
                }
                catch (IndexOutOfRangeException) // Bad formatting!
                {
                    // TODO: Move this to IHI
                    //IonEnvironment.GetHabboHotel().GetClients().StopClient(fID, 0);
                }
                catch (Exception ex)
                {
                    CoreManager.GetCore().GetStandardOut().PrintException(ex);
                }
            }
        }

        /// <summary>
        /// Routes a byte array passed as reference to another object.
        /// </summary>
        /// <param name="Data">The byte array to route.</param>
        private void RouteData(ref byte[] Data)
        {
            if (fRouteReceivedDataCallback != null)
            {
                fRouteReceivedDataCallback.Invoke(ref Data);
            }
        }
        #endregion
        
        public void Disconnect()
        {
            CoreManager.GetCore().GetConnectionManager().CloseConnection(this.GetID());
            this.Stop();
        }
    }
}
