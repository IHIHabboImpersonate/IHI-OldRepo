using System;
using System.Collections.Generic;
using System.Text;
using Ion.Specialized.Encoding;

namespace IHI.Server.Networking.Messages
{
    public interface IInternalOutgoingMessage
    {
        #region Properties

        /// <summary>
        /// Gets the ID of this message as an unsigned 32 bit integer.
        /// </summary>
        uint ID { get; }

        /// <summary>
        /// Gets the header of this message, by Base64 encoding the message ID to a 2 byte string.
        /// </summary>
        string Header { get; }

        /// <summary>
        /// Gets the length of the content in this message.
        /// </summary>
        int ContentLength { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Clears all the content in the message and sets the message ID.
        /// </summary>
        /// <param name="id">The ID of this message as an unsigned 32 bit integer.</param>
        IInternalOutgoingMessage Initialize(uint id);

        /// <summary>
        /// Clears the message content.
        /// </summary>
        IInternalOutgoingMessage Clear();

        /// <summary>
        /// Returns the total content of this message as a string.
        /// </summary>
        /// <returns>String</returns>
        string GetContentString();

        /// <summary>
        /// Appends a single byte to the message content.
        /// </summary>
        /// <param name="b">The byte to append.</param>
        IInternalOutgoingMessage Append(byte b);

        /// <summary>
        /// Appends a byte array to the message content.
        /// </summary>
        /// <param name="bzData">The byte array to append.</param>
        IInternalOutgoingMessage Append(byte[] bzData);

        /// <summary>
        /// Encodes a string with the environment's default text encoding and appends it to the message content.
        /// </summary>
        /// <param name="s">The string to append.</param>
        IInternalOutgoingMessage Append(string s);

        /// <summary>
        /// Encodes a string with a given text encoding and appends it to the message content.
        /// </summary>
        /// <param name="s">The string to append.</param>
        /// <param name="encoding">A System.Text.Encoding to use for encoding the string.</param>
        IInternalOutgoingMessage Append(string s, Encoding encoding);

        /// <summary>
        /// Appends a 32 bit integer in it's string representation to the message content.
        /// </summary>
        /// <param name="i">The 32 bit integer to append.</param>
        IInternalOutgoingMessage Append(Int32 i);

        /// <summary>
        /// Appends a 32 bit unsigned integer in it's string representation to the message content.
        /// </summary>
        /// <param name="i">The 32 bit unsigned integer to append.</param>
        IInternalOutgoingMessage Append(uint i);

        /// <summary>
        /// Appends a wire encoded boolean to the message content.
        /// </summary>
        /// <param name="b">The boolean to encode and append.</param>
        IInternalOutgoingMessage AppendBoolean(bool b);

        /// <summary>
        /// Appends a wire encoded 32 bit integer to the message content.
        /// </summary>
        /// <param name="i">The 32 bit integer to encode and append.</param>
        IInternalOutgoingMessage AppendInt32(Int32 i);

        /// <summary>
        /// Appends a wire encoded 32 bit unsigned integer to the message content.
        /// </summary>
        /// <param name="i">The 32 bit unsigned integer to encode and append.</param>
        /// <seealso>AppendInt32</seealso>
        IInternalOutgoingMessage AppendUInt32(uint i);

        /// <summary>
        /// Appends a string with the default string breaker byte to the message content.
        /// </summary>
        /// <param name="s">The string to append.</param>
        IInternalOutgoingMessage AppendString(string s);

        /// <summary>
        /// Appends a string with a given string breaker byte to the message content.
        /// </summary>
        /// <param name="s">The string to append.</param>
        /// <param name="breaker">The byte used to mark the end of the string.</param>
        IInternalOutgoingMessage AppendString(string s, byte breaker);

        byte[] GetBytes();

        #endregion
    }

    public class InternalOutgoingMessage : IInternalOutgoingMessage
    {
        #region Fields

        /// <summary>
        /// The content of this message as a System.Collections.Generic.List(byte).
        /// </summary>
        private List<byte> _content;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the ID of this message as an unsigned 32 bit integer.
        /// </summary>
        public uint ID { get; private set; }

        /// <summary>
        /// Gets the header of this message, by Base64 encoding the message ID to a 2 byte string.
        /// </summary>
        public string Header
        {
            get { return CoreManager.ServerCore.GetTextEncoding().GetString(Base64Encoding.EncodeuUInt32(ID, 2)); }
        }

        /// <summary>
        /// Gets the length of the content in this message.
        /// </summary>
        public int ContentLength
        {
            get { return _content.Count; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructs a weak ServerMessage, which is not useable until Initialize() is called.
        /// </summary>
        public InternalOutgoingMessage()
        {
            // Requires a call to Initialize before usage
        }

        /// <summary>
        /// Constructs a ServerMessage object with a given ID and no content.
        /// </summary>
        /// <param name="id">The ID of this message as an unsigned 32 bit integer.</param>
        public InternalOutgoingMessage(uint id)
        {
            Initialize(id);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Clears all the content in the message and sets the message ID.
        /// </summary>
        /// <param name="id">The ID of this message as an unsigned 32 bit integer.</param>
        public IInternalOutgoingMessage Initialize(uint id)
        {
            ID = id;
            _content = new List<byte>();

            return this;
        }

        /// <summary>
        /// Clears the message content.
        /// </summary>
        public IInternalOutgoingMessage Clear()
        {
            _content.Clear();

            return this;
        }

        /// <summary>
        /// Returns the total content of this message as a string.
        /// </summary>
        /// <returns>String</returns>
        public string GetContentString()
        {
            return CoreManager.ServerCore.GetTextEncoding().GetString(_content.ToArray());
        }

        /// <summary>
        /// Appends a single byte to the message content.
        /// </summary>
        /// <param name="b">The byte to append.</param>
        public IInternalOutgoingMessage Append(byte b)
        {
            _content.Add(b);

            return this;
        }

        /// <summary>
        /// Appends a byte array to the message content.
        /// </summary>
        /// <param name="bzData">The byte array to append.</param>
        public IInternalOutgoingMessage Append(byte[] bzData)
        {
            if (bzData != null && bzData.Length > 0)
                _content.AddRange(bzData);

            return this;
        }

        /// <summary>
        /// Encodes a string with the environment's default text encoding and appends it to the message content.
        /// </summary>
        /// <param name="s">The string to append.</param>
        public IInternalOutgoingMessage Append(string s)
        {
            Append(s, null);

            return this;
        }

        /// <summary>
        /// Encodes a string with a given text encoding and appends it to the message content.
        /// </summary>
        /// <param name="s">The string to append.</param>
        /// <param name="encoding">A System.Text.Encoding to use for encoding the string.</param>
        public IInternalOutgoingMessage Append(string s, Encoding encoding)
        {
            if (!string.IsNullOrEmpty(s))
            {
                if (encoding == null)
                    encoding = CoreManager.ServerCore.GetTextEncoding();

                Append(encoding.GetBytes(s));
            }

            return this;
        }

        /// <summary>
        /// Appends a 32 bit integer in it's string representation to the message content.
        /// </summary>
        /// <param name="i">The 32 bit integer to append.</param>
        public IInternalOutgoingMessage Append(Int32 i)
        {
            Append(i.ToString(), Encoding.UTF8);

            return this;
        }

        /// <summary>
        /// Appends a 32 bit unsigned integer in it's string representation to the message content.
        /// </summary>
        /// <param name="i">The 32 bit unsigned integer to append.</param>
        public IInternalOutgoingMessage Append(uint i)
        {
            Append((Int32) i);

            return this;
        }

        /// <summary>
        /// Appends a wire encoded boolean to the message content.
        /// </summary>
        /// <param name="b">The boolean to encode and append.</param>
        public IInternalOutgoingMessage AppendBoolean(bool b)
        {
            _content.Add(b ? WireEncoding.Positive : WireEncoding.Negative);

            return this;
        }

        /// <summary>
        /// Appends a wire encoded 32 bit integer to the message content.
        /// </summary>
        /// <param name="i">The 32 bit integer to encode and append.</param>
        public IInternalOutgoingMessage AppendInt32(Int32 i)
        {
            Append(WireEncoding.EncodeInt32(i));

            return this;
        }

        /// <summary>
        /// Appends a wire encoded 32 bit unsigned integer to the message content.
        /// </summary>
        /// <param name="i">The 32 bit unsigned integer to encode and append.</param>
        /// <seealso>AppendInt32</seealso>
        public IInternalOutgoingMessage AppendUInt32(uint i)
        {
            AppendInt32((Int32) i);

            return this;
        }

        /// <summary>
        /// Appends a string with the default string breaker byte to the message content.
        /// </summary>
        /// <param name="s">The string to append.</param>
        public IInternalOutgoingMessage AppendString(string s)
        {
            AppendString(s, 2);

            return this;
        }

        /// <summary>
        /// Appends a string with a given string breaker byte to the message content.
        /// </summary>
        /// <param name="s">The string to append.</param>
        /// <param name="breaker">The byte used to mark the end of the string.</param>
        public IInternalOutgoingMessage AppendString(string s, byte breaker)
        {
            Append(s); // Append string with default encoding
            Append(breaker); // Append breaker

            return this;
        }

        public byte[] GetBytes()
        {
            var data = new byte[ContentLength + 2 + 1];

            var headerBytes = Base64Encoding.EncodeuUInt32(ID, 2);
            data[0] = headerBytes[0];
            data[1] = headerBytes[1];

            for (var i = 0; i < ContentLength; i++)
            {
                data[i + 2] = _content[i];
            }

            data[data.Length - 1] = 1;

            return data;
        }

        #endregion
    }
}