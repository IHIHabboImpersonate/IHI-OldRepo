using System;

namespace Ion.Specialized.Encoding
{
    public static class WireEncoding
    {
        #region Fields

        public const byte Negative = 72; // 'H'
        public const byte Positive = 73; // 'I'
        public const int MaxIntegerByteAmount = 6;

        #endregion

        #region Methods

        public static byte[] EncodeInt32(Int32 i)
        {
            var wf = new byte[MaxIntegerByteAmount];
            var pos = 0;
            var numBytes = 1;
            var startPos = pos;
            var negativeMask = i >= 0 ? 0 : 4;
            i = Math.Abs(i);
            wf[pos++] = (byte) (64 + (i & 3));
            for (i >>= 2; i != 0; i >>= MaxIntegerByteAmount)
            {
                numBytes++;
                wf[pos++] = (byte) (64 + (i & 0x3f));
            }
            wf[startPos] = (byte) (wf[startPos] | numBytes << 3 | negativeMask);

            // Skip the null bytes in the result
            var bzData = new byte[numBytes];
            for (var x = 0; x < numBytes; x++)
            {
                bzData[x] = wf[x];
            }

            return bzData;
        }

        public static Int32 DecodeInt32(byte[] bzData, out Int32 totalBytes)
        {
            var pos = 0;
            var negative = (bzData[pos] & 4) == 4;
            totalBytes = bzData[pos] >> 3 & 7;
            var v = bzData[pos] & 3;
            pos++;
            var shiftAmount = 2;
            for (var b = 1; b < totalBytes; b++)
            {
                v |= (bzData[pos] & 0x3f) << shiftAmount;
                shiftAmount = 2 + 6*b;
                pos++;
            }

            if (negative)
                v *= -1;

            return v;
        }

        #endregion
    }
}