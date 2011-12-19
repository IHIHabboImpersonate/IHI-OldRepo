using System;
using NET_System = System;
using System.Text;

namespace IHI.Server
{
    public static class UnixAware
    {
        public static bool UnixMode
        {
            get;
            internal set;
        }
        public static class System
        {
            public static class Console
            {
                private static ConsoleColor _lastSetBackground = ConsoleColor.Black;
                private static ConsoleColor _lastSetForeground = ConsoleColor.White;

                public static ConsoleColor ForegroundColor
                {
                    get
                    {
                        if(!UnixMode)
                            return NET_System.Console.ForegroundColor;
                        return _lastSetForeground;
                    }
                    set
                    {
                        if (!UnixMode)
                        {
                            NET_System.Console.ForegroundColor = value;
                            return;
                        }
                        _lastSetForeground = value;
                        switch (value)
                        {
                            case ConsoleColor.Black:
                                NET_System.Console.Write("\x1b[30m");
                                break;
                            case ConsoleColor.DarkRed:
                                NET_System.Console.Write("\x1b[31m");
                                break;
                            case ConsoleColor.DarkGreen:
                                NET_System.Console.Write("\x1b[32m");
                                break;
                            case ConsoleColor.DarkYellow:
                                NET_System.Console.Write("\x1b[33m");
                                break;
                            case ConsoleColor.DarkBlue:
                                NET_System.Console.Write("\x1b[34m");
                                break;
                            case ConsoleColor.DarkMagenta:
                                NET_System.Console.Write("\x1b[35m");
                                break;
                            case ConsoleColor.DarkCyan:
                                NET_System.Console.Write("\x1b[36m");
                                break;
                            case ConsoleColor.Gray:
                                NET_System.Console.Write("\x1b[37m");
                                break;

                            case ConsoleColor.DarkGray:
                                NET_System.Console.Write("\x1b[1;30m");
                                break;
                            case ConsoleColor.Red:
                                NET_System.Console.Write("\x1b[1;31m");
                                break;
                            case ConsoleColor.Green:
                                NET_System.Console.Write("\x1b[1;32m");
                                break;
                            case ConsoleColor.Yellow:
                                NET_System.Console.Write("\x1b[1;33m");
                                break;
                            case ConsoleColor.Blue:
                                NET_System.Console.Write("\x1b[1;34m");
                                break;
                            case ConsoleColor.Magenta:
                                NET_System.Console.Write("\x1b[1;35m");
                                break;
                            case ConsoleColor.Cyan:
                                NET_System.Console.Write("\x1b[1;36m");
                                break;
                            case ConsoleColor.White:
                                NET_System.Console.Write("\x1b[1;37m");
                                break;
                        }
                    }
                }
                public static ConsoleColor BackgroundColor
                {
                    get
                    {
                        if (!UnixMode)
                            return NET_System.Console.BackgroundColor;
                        return _lastSetBackground;
                    }
                    set
                    {
                        if (!UnixMode){
                            NET_System.Console.BackgroundColor = value;
                            return;
                        }

                        _lastSetBackground = value;
                        switch (value)
                        {
                            case ConsoleColor.Black:
                                NET_System.Console.Write("\x1b[40m");
                                break;
                            case ConsoleColor.DarkRed:
                                NET_System.Console.Write("\x1b[41m");
                                break;
                            case ConsoleColor.DarkGreen:
                                NET_System.Console.Write("\x1b[42m");
                                break;
                            case ConsoleColor.DarkYellow:
                                NET_System.Console.Write("\x1b[43m");
                                break;
                            case ConsoleColor.DarkBlue:
                                NET_System.Console.Write("\x1b[44m");
                                break;
                            case ConsoleColor.DarkMagenta:
                                NET_System.Console.Write("\x1b[45m");
                                break;
                            case ConsoleColor.DarkCyan:
                                NET_System.Console.Write("\x1b[46m");
                                break;
                            case ConsoleColor.Gray:
                                NET_System.Console.Write("\x1b[47m");
                                break;

                            case ConsoleColor.DarkGray:
                                NET_System.Console.Write("\x1b[1;40m");
                                break;
                            case ConsoleColor.Red:
                                NET_System.Console.Write("\x1b[1;41m");
                                break;
                            case ConsoleColor.Green:
                                NET_System.Console.Write("\x1b[1;42m");
                                break;
                            case ConsoleColor.Yellow:
                                NET_System.Console.Write("\x1b[1;43m");
                                break;
                            case ConsoleColor.Blue:
                                NET_System.Console.Write("\x1b[1;44m");
                                break;
                            case ConsoleColor.Magenta:
                                NET_System.Console.Write("\x1b[1;45m");
                                break;
                            case ConsoleColor.Cyan:
                                NET_System.Console.Write("\x1b[1;46m");
                                break;
                            case ConsoleColor.White:
                                NET_System.Console.Write("\x1b[1;47m");
                                break;
                        }
                    }
                }

                // TODO: Make this safe (unsafe defaults).
                public static int WindowWidth
                {
                    get
                    {
                        if (!UnixMode)
                            return NET_System.Console.WindowWidth;
                        return 80;
                    }
                    set
                    {
                        if (!UnixMode){
                            NET_System.Console.WindowWidth = value;
                            return;
                        }
                    }
                }

                public static int WindowHeight
                {
                    get
                    {
                        if (!UnixMode)
                            return NET_System.Console.WindowHeight;
                        return 20;
                    }
                    set
                    {
                        if (!UnixMode){
                            NET_System.Console.WindowHeight = value;
                            return;
                        }
                    }
                }

                public static int BufferWidth
                {
                    get
                    {
                        if (!UnixMode)
                            return NET_System.Console.BufferWidth;
                        return 80;
                    }
                    set
                    {
                        if (!UnixMode){
                            NET_System.Console.BufferWidth = value;
                            return;
                        }
                    }
                }

                public static int BufferHeight
                {
                    get
                    {
                        if (!UnixMode)
                            return NET_System.Console.BufferHeight;
                        return 300;
                    }
                    set
                    {
                        if (!UnixMode)
                        {
                            NET_System.Console.BufferHeight = value;
                            return;
                        }
                    }
                }

                public static void Clear()
                {
                    NET_System.Console.SetCursorPosition(0, 0);
                    var blankness = new StringBuilder { Length = BufferWidth * BufferHeight };
                    NET_System.Console.Write(blankness.ToString());

                    NET_System.Console.SetCursorPosition(0, 0);
                }
            }
        }
    }
}