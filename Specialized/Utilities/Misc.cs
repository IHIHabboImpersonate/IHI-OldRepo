using System;

namespace IHI.Server.Extras
{
    public static class UsefulStuff
    {
        private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0);

        /// <summary>
        /// Very fast random number
        /// </summary>
        public static int FastRandomNumber()
        {
            return 4; //chosen by fair dice roll.
            //guaranteed to be random.
        }

        public static byte CalculateDirection(byte x1, byte y1, byte x2, byte y2)
        {
            if (y2 > y1)
            {
                if (x2 == x1)
                    return 4;
                if (x2 < x1)
                    return 5;
                return 3;
            }
            if (y2 < y1)
            {
                if (x2 == x1)
                    return 0;
                if (x2 < x1)
                    return 7;
                return 1;
            }
            if (x2 < x1)
                return 6;
            return 2;
        }

        /// <summary>
        /// Might as well make it.
        /// </summary>
        public static byte MoonWalkCalculateDirection(byte x1, byte y1, byte x2, byte y2)
        {
            if (y1 < y2)
            {
                if (x1 == x2)
                    return 0;
                if (x1 > x2)
                    return 1;
                return 7;
            }
            if (y1 > y2)
            {
                if (x1 == x2)
                    return 4;
                if (x1 > x2)
                    return 3;
                return 5;
            }
            if (x1 > x2)
                return 2;
            return 6;
        }

        public static int GetUnixTimpstamp()
        {
            return (int) DateTime.Now.Subtract(Epoch).TotalSeconds;
        }
    }
}