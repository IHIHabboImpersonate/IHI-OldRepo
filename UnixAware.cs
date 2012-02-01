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
using System.Text;
using NET_System = System;

#endregion

namespace IHI.Server
{
    public static class UnixAware
    {
        public static bool UnixMode { get; internal set; }

        #region Nested type: System

        public static class System
        {
            #region Nested type: Console

            public static class Console
            {
                private static ConsoleColor _lastSetBackground = ConsoleColor.Black;
                private static ConsoleColor _lastSetForeground = ConsoleColor.White;

                public static ConsoleColor ForegroundColor
                {
                    get
                    {
                        if (!UnixMode)
                            return global::System.Console.ForegroundColor;
                        return _lastSetForeground;
                    }
                    set
                    {
                        if (!UnixMode)
                        {
                            global::System.Console.ForegroundColor = value;
                            return;
                        }
                        _lastSetForeground = value;
                        switch (value)
                        {
                            case ConsoleColor.Black:
                                global::System.Console.Write("\x1b[30m");
                                break;
                            case ConsoleColor.DarkRed:
                                global::System.Console.Write("\x1b[31m");
                                break;
                            case ConsoleColor.DarkGreen:
                                global::System.Console.Write("\x1b[32m");
                                break;
                            case ConsoleColor.DarkYellow:
                                global::System.Console.Write("\x1b[33m");
                                break;
                            case ConsoleColor.DarkBlue:
                                global::System.Console.Write("\x1b[34m");
                                break;
                            case ConsoleColor.DarkMagenta:
                                global::System.Console.Write("\x1b[35m");
                                break;
                            case ConsoleColor.DarkCyan:
                                global::System.Console.Write("\x1b[36m");
                                break;
                            case ConsoleColor.Gray:
                                global::System.Console.Write("\x1b[37m");
                                break;

                            case ConsoleColor.DarkGray:
                                global::System.Console.Write("\x1b[1;30m");
                                break;
                            case ConsoleColor.Red:
                                global::System.Console.Write("\x1b[1;31m");
                                break;
                            case ConsoleColor.Green:
                                global::System.Console.Write("\x1b[1;32m");
                                break;
                            case ConsoleColor.Yellow:
                                global::System.Console.Write("\x1b[1;33m");
                                break;
                            case ConsoleColor.Blue:
                                global::System.Console.Write("\x1b[1;34m");
                                break;
                            case ConsoleColor.Magenta:
                                global::System.Console.Write("\x1b[1;35m");
                                break;
                            case ConsoleColor.Cyan:
                                global::System.Console.Write("\x1b[1;36m");
                                break;
                            case ConsoleColor.White:
                                global::System.Console.Write("\x1b[1;37m");
                                break;
                        }
                    }
                }

                public static ConsoleColor BackgroundColor
                {
                    get
                    {
                        if (!UnixMode)
                            return global::System.Console.BackgroundColor;
                        return _lastSetBackground;
                    }
                    set
                    {
                        if (!UnixMode)
                        {
                            global::System.Console.BackgroundColor = value;
                            return;
                        }

                        _lastSetBackground = value;
                        switch (value)
                        {
                            case ConsoleColor.Black:
                                global::System.Console.Write("\x1b[40m");
                                break;
                            case ConsoleColor.DarkRed:
                                global::System.Console.Write("\x1b[41m");
                                break;
                            case ConsoleColor.DarkGreen:
                                global::System.Console.Write("\x1b[42m");
                                break;
                            case ConsoleColor.DarkYellow:
                                global::System.Console.Write("\x1b[43m");
                                break;
                            case ConsoleColor.DarkBlue:
                                global::System.Console.Write("\x1b[44m");
                                break;
                            case ConsoleColor.DarkMagenta:
                                global::System.Console.Write("\x1b[45m");
                                break;
                            case ConsoleColor.DarkCyan:
                                global::System.Console.Write("\x1b[46m");
                                break;
                            case ConsoleColor.Gray:
                                global::System.Console.Write("\x1b[47m");
                                break;

                            case ConsoleColor.DarkGray:
                                global::System.Console.Write("\x1b[1;40m");
                                break;
                            case ConsoleColor.Red:
                                global::System.Console.Write("\x1b[1;41m");
                                break;
                            case ConsoleColor.Green:
                                global::System.Console.Write("\x1b[1;42m");
                                break;
                            case ConsoleColor.Yellow:
                                global::System.Console.Write("\x1b[1;43m");
                                break;
                            case ConsoleColor.Blue:
                                global::System.Console.Write("\x1b[1;44m");
                                break;
                            case ConsoleColor.Magenta:
                                global::System.Console.Write("\x1b[1;45m");
                                break;
                            case ConsoleColor.Cyan:
                                global::System.Console.Write("\x1b[1;46m");
                                break;
                            case ConsoleColor.White:
                                global::System.Console.Write("\x1b[1;47m");
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
                            return global::System.Console.WindowWidth;
                        return 80;
                    }
                    set
                    {
                        if (!UnixMode)
                        {
                            global::System.Console.WindowWidth = value;
                            return;
                        }
                    }
                }

                public static int WindowHeight
                {
                    get
                    {
                        if (!UnixMode)
                            return global::System.Console.WindowHeight;
                        return 20;
                    }
                    set
                    {
                        if (!UnixMode)
                        {
                            global::System.Console.WindowHeight = value;
                            return;
                        }
                    }
                }

                public static int BufferWidth
                {
                    get
                    {
                        if (!UnixMode)
                            return global::System.Console.BufferWidth;
                        return 80;
                    }
                    set
                    {
                        if (!UnixMode)
                        {
                            global::System.Console.BufferWidth = value;
                            return;
                        }
                    }
                }

                public static int BufferHeight
                {
                    get
                    {
                        if (!UnixMode)
                            return global::System.Console.BufferHeight;
                        return 300;
                    }
                    set
                    {
                        if (!UnixMode)
                        {
                            global::System.Console.BufferHeight = value;
                            return;
                        }
                    }
                }

                public static void Clear()
                {
                    global::System.Console.SetCursorPosition(0, 0);
                    StringBuilder blankness = new StringBuilder {Length = BufferWidth*BufferHeight};
                    global::System.Console.Write(blankness.ToString());

                    global::System.Console.SetCursorPosition(0, 0);
                }
            }

            #endregion
        }

        #endregion
    }
}