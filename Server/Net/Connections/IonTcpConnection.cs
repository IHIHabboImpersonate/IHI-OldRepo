using System;
using System.Threading;
using System.Net.Sockets;
using System.Reflection;

using Ion.Specialized.Encoding;
using Ion.Specialized.Utilities;

using IHI.Server;
using IHI.Server.Net.Messages;


namespace Ion.Net.Connections
{
    /// <summary>
    /// Represents a TCP network connection accepted by an IonTcpConnectionListener. Provides methods for sending and receiving data, aswell as disconnecting the connection.
    /// </summary>
    public class IonTcpConnection
    {
        #region Fields
        /// <summary>
        /// The buffer size for receiving data.
        /// </summary>
        private const int RECEIVEDATA_BUFFER_SIZE = 512;
        /// <summary>
        /// The amount of milliseconds to sleep when receiving data before processing the message. When this constant is 0, the data will be processed immediately.
        /// </summary>
        private static int RECEIVEDATA_MILLISECONDS_DELAY = 0;
        
        /// <summary>
        /// The ID of this connection as a 32 bit unsigned integer.
        /// </summary>
        private readonly uint mID;
        /// <summary>
        /// A DateTime object representing the date and time this connection was created.
        /// </summary>
        private readonly DateTime mCreatedAt;
       
        /// <summary>
        /// The System.Net.Sockets.Socket object providing the connection between client and server.
        /// </summary>
        private Socket mSocket = null;

        /// <summary>
        /// The byte array holding the buffer for receiving data from client.
        /// </summary>
        private byte[] mDataBuffer = null;
        /// <summary>
        /// The AsyncCallback instance for the thread for receiving data asynchronously.
        /// </summary>
        private AsyncCallback mDataReceivedCallback;
        /// <summary>
        /// The RouteReceivedDataCallback to route received data to another object.
        /// </summary>
        private RouteReceivedDataCallback mRouteReceivedDataCallback;
        /// <summary>
        /// The User that holds this connection.
        /// </summary>
        private User mUser;

        private PacketHandler[,] mPacketHandlers;
        #endregion

        #region Members
        public delegate void RouteReceivedDataCallback(ref byte[] Data);
        #endregion

        #region Properties
        /// <summary>
        /// Gets the ID of this connection as a 32 bit unsigned integer.
        /// </summary>
        public uint ID
        {
            get { return mID; }
        }
        /// <summary>
        /// Gets the DateTime object representing the date and time this connection was created at.
        /// </summary>
        public DateTime createdAt
        {
            get { return mCreatedAt; }
        }
        /// <summary>
        /// Gets the age of this connection in whole seconds, by comparing the current date and time to the date and time this connection was created at.
        /// </summary>
        public int ageInSeconds
        {
            get
            {
                int Seconds = (int)(DateTime.Now - mCreatedAt).TotalSeconds;
                if (Seconds <= 0)
                    return 0;

                return Seconds;
            }
        }
        /// <summary>
        /// Gets the IP address of this connection as a string.
        /// </summary>
        public string ipAddress
        {
            get
            {
                if (mSocket == null)
                    return "";

                return mSocket.RemoteEndPoint.ToString().Split(':')[0];
            }
        }
        /// <summary>
        /// Gets if this connection still posesses a socket object.
        /// </summary>
        public bool Alive
        {
            get { return (mSocket != null); }
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Constructs a new instance of IonTcpConnection for a given connection identifier and socket.
        /// </summary>
        /// <param name="ID">The unique ID used to identify this connection in the environment.</param>
        /// <param name="pSocket">The System.Net.Sockets.Socket of the new connection.</param>
        public IonTcpConnection(uint ID, Socket pSocket)
        {
            mID = ID;
            mSocket = pSocket;
            mCreatedAt = DateTime.Now;

            this.mPacketHandlers = new PacketHandler[2002 + 1, 4]; // "2002" copied from Ion.HabboHotel.Client.ClientMessageHandler.HIGHEST_MESSAGEID
        }
        #endregion

        #region Methods
        /// <summary>
        /// Starts the connection, prepares the received data buffer and waits for data.
        /// </summary>
        public void Start()
        {

            mDataBuffer = new byte[RECEIVEDATA_BUFFER_SIZE];
            mDataReceivedCallback = new AsyncCallback(DataReceived);
            mRouteReceivedDataCallback = new IonTcpConnection.RouteReceivedDataCallback(HandleConnectionData);


            this.mUser = new User(this);
            this.mUser.StartLoggedInValues(this);
            WaitForData();
        }
        /// <summary>
        /// Stops the connection, disconnects the socket and disposes used resources.
        /// </summary>
        public void Stop()
        {
            if (!this.Alive)
                return; // Already stopped

            if (this.mUser.IsLoggedIn())
            {
                this.mUser.StopLoggedInValues();
                this.mUser.SetLoggedIn(false);
            }

            mSocket.Close();
            mSocket = null;
            mDataBuffer = null;
            mDataReceivedCallback = null;
        }
        public bool TestConnection()
        {
            try
            {
                return mSocket.Send(new byte[] { 0 }) > 0;
                //mSocket.Send(new byte[] { 0 });
                //return true;
            }
            catch { }

            return false;
        }
        private void ConnectionDead()
        {
            // TODO: Move this to IHI
            //IonEnvironment.GetHabboHotel().GetClients().StopClient(mID);
        }

        public void SendData(byte[] Data)
        {
            if (this.Alive)
            {
                try
                {
                    mSocket.Send(Data);
                }
                catch (SocketException)
                {
                    ConnectionDead();
                }
                catch (ObjectDisposedException)
                {
                    ConnectionDead();
                }
                catch (Exception ex)
                {
                    Core.GetStandardOut().PrintError(ex.Message);
                }
            }
        }
        public void SendData(string sData)
        {
            SendData(Core.GetTextEncoding().GetBytes(sData));
        }
        public void SendMessage(OutgoingMessage message)
        {
            Core.GetStandardOut().PrintNotice(" [" + mID + "] <-- " + message.Header + message.GetContentString());

            SendData(message.GetBytes());
        }

        /// <summary>
        /// Starts the asynchronous wait for new data.
        /// </summary>
        private void WaitForData()
        {
            if (this.Alive)
            {
                try
                {
                    mSocket.BeginReceive(mDataBuffer, 0, RECEIVEDATA_BUFFER_SIZE, SocketFlags.None, mDataReceivedCallback, null);
                }
                catch (SocketException)
                {
                    ConnectionDead();
                }
                catch (ObjectDisposedException)
                {
                    ConnectionDead();
                }
                catch (Exception ex)
                {
                    Core.GetStandardOut().PrintError(ex.Message);
                    ConnectionDead();
                }
            }
        }
        private void DataReceived(IAsyncResult iAr)
        {
            // Connection not stopped yet?
            if (this.Alive == false)
                return;

            // Do an optional wait before processing the data
            if (RECEIVEDATA_MILLISECONDS_DELAY > 0)
                Thread.Sleep(RECEIVEDATA_MILLISECONDS_DELAY);

            // How many bytes has server received?
            int numReceivedBytes = 0;
            try
            {
                numReceivedBytes = mSocket.EndReceive(iAr);
            }
            catch (ObjectDisposedException)
            {
                ConnectionDead();
                return;
            }
            catch (Exception ex)
            {
                Core.GetStandardOut().PrintError(ex.Message);
                
                ConnectionDead();
                return;
            }

            if (numReceivedBytes > 0)
            {
                // Copy received data buffer
                byte[] dataToProcess = ByteUtility.ChompBytes(mDataBuffer, 0, numReceivedBytes);

                // Route data to GameClient to parse and process messages
                RouteData(ref dataToProcess);
                
                HandleConnectionData(ref dataToProcess);
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

                    if(this.mUser.IsLoggedIn())
                        Core.GetStandardOut().PrintDebug("[" + this.mUser.GetUsername() + "] -- > " + message.Header + message.GetContentString());
                    else
                        Core.GetStandardOut().PrintDebug("[No Name] -> " + message.Header + message.GetContentString());


                    // Handle message object
                    this.mPacketHandlers[messageID, 3].Invoke(message); // Execute High Priority
                    
                    if (message.IsCancelled())
                        return;
                    
                    this.mPacketHandlers[messageID, 2].Invoke(message); // Execute Low Priority

                    if (message.IsCancelled())
                        return;

                    this.mPacketHandlers[messageID, 1].Invoke(message); // Execute Default Action
                    this.mPacketHandlers[messageID, 0].Invoke(message); // Execute Watchers
                }
                catch (IndexOutOfRangeException) // Bad formatting!
                {
                    // TODO: Move this to IHI
                    //IonEnvironment.GetHabboHotel().GetClients().StopClient(mID, 0);
                }
                catch (Exception ex)
                {
                    Core.GetStandardOut().PrintError(ex.Message);
                }
            }
        }

        /// <summary>
        /// Routes a byte array passed as reference to another object.
        /// </summary>
        /// <param name="Data">The byte array to route.</param>
        private void RouteData(ref byte[] Data)
        {
            if (mRouteReceivedDataCallback != null)
            {
                mRouteReceivedDataCallback.Invoke(ref Data);
            }
        }


        public IonTcpConnection AddHandler(uint HeaderID, byte Priority, MethodInfo Method)
        {
            this.mPacketHandlers[HeaderID, Priority] += (PacketHandler)Delegate.CreateDelegate(typeof(PacketHandler), Method);
            return this;
        }
        public IonTcpConnection RemoveHandler(uint HeaderID, byte Priority, MethodInfo Method)
        {
            // TODO: Look into how this would be done safely.
            return this;
        }
        #endregion
    }
}
