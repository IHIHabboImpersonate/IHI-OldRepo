using System;
using System.Text;
using System.Collections.Generic;

using Ion.Specialized.Encoding;

namespace IHI.Server.Net.Messages
{
    public class OutgoingMessage
    {
        #region Fields
        /// <summary>
        /// The ID of this message as an unsigned 32 bit integer.
        /// </summary>
        private uint fID;
        /// <summary>
        /// The content of this message as a System.Collections.Generic.List(byte).
        /// </summary>
        private List<byte> fContent;
        #endregion

        #region Properties
        /// <summary>
        /// Gets the ID of this message as an unsigned 32 bit integer.
        /// </summary>
        internal uint ID
        {
            get { return fID; }
        }
        /// <summary>
        /// Gets the header of this message, by Base64 encoding the message ID to a 2 byte string.
        /// </summary>
        internal string Header
        {
            get
            {
                return Core.GetTextEncoding().GetString(Base64Encoding.EncodeuUInt32(fID, 2));
            }
        }
        /// <summary>
        /// Gets the length of the content in this message.
        /// </summary>
        internal int contentLength
        {
            get { return fContent.Count; }
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Constructs a weak ServerMessage, which is not useable until Initialize() is called.
        /// </summary>
        internal OutgoingMessage()
        {
            // Requires a call to Initialize before usage
        }
        /// <summary>
        /// Constructs a ServerMessage object with a given ID and no content.
        /// </summary>
        /// <param name="ID">The ID of this message as an unsigned 32 bit integer.</param>
        internal OutgoingMessage(uint ID)
        {
            Initialize(ID);
        }
        #endregion

        #region Methods
        /// <summary>
        /// Clears all the content in the message and sets the message ID.
        /// </summary>
        /// <param name="ID">The ID of this message as an unsigned 32 bit integer.</param>
        internal void Initialize(uint ID)
        {
            fID = ID;
            fContent = new List<byte>();
        }
        /// <summary>
        /// Clears the message content.
        /// </summary>
        internal void Clear()
        {
            fContent.Clear();
        }
        /// <summary>
        /// Returns the total content of this message as a string.
        /// </summary>
        /// <returns>String</returns>
        internal string GetContentString()
        {
            return Core.GetTextEncoding().GetString(fContent.ToArray());
        }

        /// <summary>
        /// Appends a single byte to the message content.
        /// </summary>
        /// <param name="i">The byte to append.</param>
        internal void Append(byte b)
        {
            fContent.Add(b);
        }
        /// <summary>
        /// Appends a byte array to the message content.
        /// </summary>
        /// <param name="bzData">The byte array to append.</param>
        internal void Append(byte[] bzData)
        {
            if (bzData != null && bzData.Length > 0)
                fContent.AddRange(bzData);
        }
        /// <summary>
        /// Encodes a string with the environment's default text encoding and appends it to the message content.
        /// </summary>
        /// <param name="s">The string to append.</param>
        internal void Append(string s)
        {
            Append(s, null);
        }
        /// <summary>
        /// Encodes a string with a given text encoding and appends it to the message content.
        /// </summary>
        /// <param name="s">The string to append.</param>
        /// <param name="pEncoding">A System.Text.Encoding to use for encoding the string.</param>
        internal void Append(string s, Encoding Encoding)
        {
            if (s != null && s.Length > 0)
            {
                if (Encoding == null)
                    Encoding = Core.GetTextEncoding();

                Append(Encoding.GetBytes(s));
            }
        }
        /// <summary>
        /// Appends a 32 bit integer in it's string representation to the message content.
        /// </summary>
        /// <param name="i">The 32 bit integer to append.</param>
        internal void Append(Int32 i)
        {
            Append(i.ToString(), Encoding.UTF8);
        }
        /// <summary>
        /// Appends a 32 bit unsigned integer in it's string representation to the message content.
        /// </summary>
        /// <param name="i">The 32 bit unsigned integer to append.</param>
        internal void Append(uint i)
        {
            Append((Int32)i);
        }

        /// <summary>
        /// Appends a wire encoded boolean to the message content.
        /// </summary>
        /// <param name="i">The boolean to encode and append.</param>
        internal void AppendBoolean(bool b)
        {
            if (b == true)
                fContent.Add(WireEncoding.POSITIVE);
            else
                fContent.Add(WireEncoding.NEGATIVE);
        }
        /// <summary>
        /// Appends a wire encoded 32 bit integer to the message content.
        /// </summary>
        /// <param name="i">The 32 bit integer to encode and append.</param>
        internal void AppendInt32(Int32 i)
        {
            Append(WireEncoding.EncodeInt32(i));
        }
        /// <summary>
        /// Appends a wire encoded 32 bit unsigned integer to the message content.
        /// </summary>
        /// <param name="i">The 32 bit unsigned integer to encode and append.</param>
        /// <seealso>AppendInt32</seealso>
        internal void AppendUInt32(uint i)
        {
            AppendInt32((Int32)i);
        }

        /// <summary>
        /// Appends a string with the default string breaker byte to the message content.
        /// </summary>
        /// <param name="s">The string to append.</param>
        internal void AppendString(string s)
        {
            AppendString(s, 2);
        }
        /// <summary>
        /// Appends a string with a given string breaker byte to the message content.
        /// </summary>
        /// <param name="s">The string to append.</param>
        /// <param name="Breaker">The byte used to mark the end of the string.</param>
        internal void AppendString(string s, byte Breaker)
        {
            Append(s); // Append string with default encoding
            Append(Breaker); // Append breaker
        }

        internal void AppendObject(ISerializableObject obj)
        {
            obj.Serialize(this);
        }

        internal byte[] GetBytes()
        {
            byte[] Data = new byte[contentLength + 2 + 1];
            
            byte[] Header = Base64Encoding.EncodeuUInt32(this.fID, 2);
            Data[0] = Header[0];
            Data[1] = Header[1];

            for (int i = 0; i < this.contentLength; i++)
            {
                Data[i + 2] = fContent[i]; 
            }

            Data[Data.Length - 1] = 1;

            return Data;
        }
        #endregion
    }
}
