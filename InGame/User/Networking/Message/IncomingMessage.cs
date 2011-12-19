using System;
using System.Text;
using Ion.Specialized.Encoding;

namespace IHI.Server.Networking.Messages
{
    /// <summary>
    /// Represents a Habbo client > server protocol structured message, providing methods to identify and 'read' the message.
    /// </summary>
    public class IncomingMessage
    {
        #region Fields

        /// <summary>
        /// The content of this message as a byte array.
        /// </summary>
        private readonly byte[] _content;

        /// <summary>
        /// The ID of this message as an unsigned 32 bit integer.
        /// </summary>
        private readonly uint _id;

        /// <summary>
        /// If set to true then lower proirity handlers will not be called.
        /// </summary>
        private bool _cancelled;

        /// <summary>
        /// The current index in the content array, used when reading the message.
        /// </summary>
        private int _contentCursor;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the ID of this message as an unsigned 32 bit integer.
        /// </summary>
        public uint GetID()
        {
            return _id;
        }

        /// <summary>
        /// Gets the header of this message, by Base64 encoding the message ID to a 2 byte string.
        /// </summary>
        public string GetHeader()
        {
            return CoreManager.ServerCore.GetTextEncoding().GetString(Base64Encoding.EncodeuUInt32(_id, 2));
        }

        /// <summary>
        /// Gets the length of the content in this message.
        /// </summary>
        public int GetContentLength()
        {
            return _content.Length;
        }

        /// <summary>
        /// Gets the amount of unread content bytes.
        /// </summary>
        public int GetRemainingContent()
        {
            return (_content.Length - _contentCursor);
        }
        #endregion

        #region Constructors

        /// <summary>
        /// Constructs a ClientMessage object for a given message ID and a given content byte array.
        /// </summary>
        /// <param name="id">The ID of the message as an unsigned 32 bit integer.</param>
        /// <param name="bzContent">The content as a byte array. If null is supplied, an empty byte array will be created.</param>
        internal IncomingMessage(uint id, byte[] bzContent)
        {
            if (bzContent == null)
                bzContent = new byte[0];

            _id = id;
            _content = bzContent;
            _contentCursor = 0;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Resets the client message to it's state when it was constructed by resetting the content reader cursor. This allows to re-read read data.
        /// </summary>
        public void Reset()
        {
            _contentCursor = 0;
        }

        /// <summary>
        /// Advances the content cursor by a given amount of bytes.
        /// </summary>
        /// <param name="n">The amount of bytes to 'skip'.</param>
        public void Advance(int n)
        {
            _contentCursor += n;
        }

        /// <summary>
        /// Returns the total content of this message as a string.
        /// </summary>
        /// <returns>String</returns>
        public string GetContentString()
        {
            return CoreManager.ServerCore.GetTextEncoding().GetString(_content);
        }

        /// <summary>
        /// Returns the header and total content of this message as a string.
        /// </summary>
        public string GetFullString()
        {
            return GetHeader() + GetContentString();
        }

        /// <summary>
        /// Reads a given amount of bytes from the remaining message content and returns it in a byte array. The reader cursor is incremented during reading.
        /// </summary>
        /// <param name="numBytes">The amount of bytes to read, advance and return. If there is less remaining data than this value, all remaining data will be read.</param>
        /// <returns>byte[]</returns>
        public byte[] ReadBytes(int numBytes)
        {
            if (numBytes > GetRemainingContent())
                numBytes = GetRemainingContent();

            var bzData = new byte[numBytes];
            for (var x = 0; x < numBytes; x++)
            {
                bzData[x] = _content[_contentCursor++];
            }

            return bzData;
        }

        /// <summary>
        /// Reads a given amount of bytes from the remaining message content and returns it in a byte array. The reader cursor does not increment during reading.
        /// </summary>
        /// <param name="numBytes">The amount of bytes to read, advance and return. If there is less remaining data than this value, all remaining data will be read.</param>
        /// <returns>byte[]</returns>
        public byte[] ReadBytesFreezeCursor(int numBytes)
        {
            if (numBytes > GetRemainingContent())
                numBytes = GetRemainingContent();

            var bzData = new byte[numBytes];
            for (int x = 0, y = _contentCursor; x < numBytes; x++, y++)
            {
                bzData[x] = _content[y];
            }

            return bzData;
        }

        /// <summary>
        /// Reads a length-prefixed (Base64) value from the message and returns it as a byte array.
        /// </summary>
        /// <returns>byte[]</returns>
        public byte[] ReadPrefixedValue()
        {
            var length = Base64Encoding.DecodeInt32(ReadBytes(2));
            return ReadBytes(length);
        }

        /// <summary>
        /// Reads a Base64 boolean and returns it. False is returned if there is no remaining content.
        /// </summary>
        /// <returns>Boolean</returns>
        public Boolean PopBase64Boolean()
        {
            return (GetRemainingContent() > 0 && _content[_contentCursor++] == Base64Encoding.Positive);
        }

        public Int32 PopInt32()
        {
            return Base64Encoding.DecodeInt32(ReadBytes(2));
        }

        public UInt32 PopUInt32()
        {
            return (UInt32) PopInt32();
        }

        /// <summary>
        /// Reads a length prefixed string from the message content and encodes it with a given System.Text.Encoding.
        /// </summary>
        /// <param name="pEncoding">The System.Text.Encoding to encode the string with.</param>
        /// <returns>String</returns>
        public String PopPrefixedString(Encoding pEncoding)
        {
            if (pEncoding == null)
                pEncoding = CoreManager.ServerCore.GetTextEncoding();

            return pEncoding.GetString(ReadPrefixedValue());
        }

        /// <summary>
        /// Reads a length prefixed string from the message content and encodes it with the IonHabboImpersonate.Server environment default text encoding.
        /// </summary>
        /// <returns>String</returns>
        public String PopPrefixedString()
        {
            var pEncoding = CoreManager.ServerCore.GetTextEncoding();
            return PopPrefixedString(pEncoding);
        }

        /// <summary>
        /// Reads a length prefixed string 32 bit integer from the message content and tries to parse it to integer. No exceptions are thrown if parsing fails.
        /// </summary>
        /// <returns>Int32</returns>
        public Int32 PopPrefixedInt32()
        {
            Int32 i;
            var s = PopPrefixedString(Encoding.UTF8);
            Int32.TryParse(s, out i);

            return i;
        }

        /// <summary>
        /// Reads a length prefixed string 32 bit unsigned integer from the message content and tries to parse it to integer. No exceptions are thrown if parsing fails.
        /// </summary>
        /// <returns>Int32</returns>
        /// <seealso>PopFixedInt32</seealso>
        public uint PopPrefixedUInt32()
        {
            return (uint) PopPrefixedInt32();
        }

        /// <summary>
        /// Reads a wire format boolean and returns it. False is returned if there is no remaining content.
        /// </summary>
        /// <returns>Boolean</returns>
        internal Boolean PopWiredBoolean()
        {
            return (GetRemainingContent() > 0 && _content[_contentCursor++] == WireEncoding.Positive);
        }

        /// <summary>
        /// Reads the next wire encoded 32 bit integer from the message content and advances the reader cursor.
        /// </summary>
        /// <returns>Int32</returns>
        public Int32 PopWiredInt32()
        {
            if (GetRemainingContent() == 0)
                return 0;

            var bzData = ReadBytesFreezeCursor(WireEncoding.MaxIntegerByteAmount);
            int totalBytes;
            var i = WireEncoding.DecodeInt32(bzData, out totalBytes);
            _contentCursor += totalBytes;

            return i;
        }

        /// <summary>
        /// Reads the next wire encoded unsigned 32 bit integer from the message content and advances the reader cursor.
        /// </summary>
        /// <returns>Int32</returns>
        /// <see>PopWiredInt32()</see>
        public uint PopWiredUInt32()
        {
            return (uint) PopWiredInt32();
        }

        #endregion

        /// <summary>
        /// Returns true if the packet has been cancelled.
        /// </summary>
        /// <returns></returns>
        public bool IsCancelled()
        {
            return _cancelled;
        }

        public IncomingMessage Cancel()
        {
            _cancelled = true;
            return this;
        }
    }
}