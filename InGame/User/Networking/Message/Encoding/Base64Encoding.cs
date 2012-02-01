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

#endregion

namespace Ion.Specialized.Encoding
{
    /// <summary>
    ///   Provides methods for encoding and decoding integers to byte arrays. This class is static.
    /// </summary>
    public class Base64Encoding
    {
        #region Fields

        public const byte Negative = 64; // '@'
        public const byte Positive = 65; // 'A'

        #endregion

        #region Methods

        /// <summary>
        ///   Encodes a 32 bit integer to a Base64 byte array of a given length.
        /// </summary>
        /// <param name = "i">The integer to encode.</param>
        /// <param name = "numBytes">The length of the byte array.</param>
        /// <returns>A byte array holding the encoded integer.</returns>
        public static byte[] EncodeInt32(Int32 i, int numBytes)
        {
            byte[] bzRes = new byte[numBytes];
            for (int j = 1; j <= numBytes; j++)
            {
                int k = ((numBytes - j)*6);
                bzRes[j - 1] = (byte) (0x40 + ((i >> k) & 0x3f));
            }

            return bzRes;
        }

        public static byte[] EncodeuUInt32(uint i, int numBytes)
        {
            return EncodeInt32((Int32) i, numBytes);
        }

        /// <summary>
        ///   Base64 decodes a byte array to a 32 bit integer.
        /// </summary>
        /// <param name = "bzData">The input byte array to decode.</param>
        /// <returns>The decoded 32 bit integer.</returns>
        public static Int32 DecodeInt32(byte[] bzData)
        {
            int i = 0;
            int j = 0;
            for (int k = bzData.Length - 1; k >= 0; k--)
            {
                int x = bzData[k] - 0x40;
                if (j > 0)
                    x *= (int) Math.Pow(64.0, j);

                i += x;
                j++;
            }

            return i;
        }

        public static uint DecodeUInt32(byte[] bzData)
        {
            return (uint) DecodeInt32(bzData);
        }

        #endregion
    }
}