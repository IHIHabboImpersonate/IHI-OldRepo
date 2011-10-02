namespace Ion.Specialized.Utilities
{
    /// <summary>
    /// Provides various common methods for working with bytes.
    /// </summary>
    public static class ByteUtility
    {
        #region Methods

        public static byte[] ChompBytes(byte[] bzBytes, int offset, int numBytes)
        {
            var end = (offset + numBytes);
            if (end > bzBytes.Length)
                end = bzBytes.Length;

            if (numBytes > bzBytes.Length)
                numBytes = bzBytes.Length;
            if (numBytes < 0)
                numBytes = 0;

            var bzChunk = new byte[numBytes];
            for (var x = 0; x < numBytes; x++)
            {
                bzChunk[x] = bzBytes[offset++];
            }

            return bzChunk;
        }

        #endregion
    }
}