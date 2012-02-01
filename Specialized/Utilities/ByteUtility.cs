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

namespace Ion.Specialized.Utilities
{
    /// <summary>
    ///   Provides various common methods for working with bytes.
    /// </summary>
    public static class ByteUtility
    {
        #region Methods

        public static byte[] ChompBytes(byte[] bzBytes, int offset, int numBytes)
        {
            if (numBytes > bzBytes.Length)
                numBytes = bzBytes.Length;
            if (numBytes < 0)
                numBytes = 0;

            byte[] bzChunk = new byte[numBytes];
            for (int x = 0; x < numBytes; x++)
            {
                bzChunk[x] = bzBytes[offset++];
            }

            return bzChunk;
        }

        #endregion
    }
}