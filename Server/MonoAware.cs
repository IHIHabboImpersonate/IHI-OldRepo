using System;

using IHI.Server.MA;
using System.Text;

namespace IHI.Server
{
    public static class MonoAware
    {
        private static ISystem fSystem;

        public static ISystem System
        {
            get
            {
                return fSystem;
            }
        }

        internal static void Init(bool Mono)
        {
            if (!Mono)
            {
                fSystem = new NETSystem();
            }
            else
            {
                fSystem = new MonoSystem();
            }
        }
    }
}

namespace IHI.Server.MA
{
    public abstract class ISystem
    {
        public abstract ISystem_Console Console { get; }
    }
    public abstract class ISystem_Console
    {
        public abstract ConsoleColor ForegroundColor { get;  set; }
        public abstract ConsoleColor BackgroundColor { get;  set; }
        public abstract int WindowWidth { get;  set; }
        public abstract int WindowHeight { get;  set; }
        public abstract int BufferWidth { get;  set; }
        public abstract int BufferHeight { get; set; }
        public abstract void Clear();
    }

    public class NETSystem : ISystem
    {
        private readonly ISystem_Console fConsole = new NetConsole();
        public override ISystem_Console Console
        {
            get
            {
                return fConsole;
            }
        }

    }
    public class NetConsole : ISystem_Console
    {
        public override ConsoleColor ForegroundColor
        {
            get
            {
                return System.Console.ForegroundColor;
            }
            set
            {
                System.Console.ForegroundColor = value;
            }
        }
        public override ConsoleColor BackgroundColor
        {
            get
            {
                return System.Console.BackgroundColor;
            }
            set
            {
                System.Console.BackgroundColor = value;
            }
        }

        public override int WindowWidth
        {
            get { return System.Console.WindowWidth; }
            set { System.Console.WindowWidth = value; }
        }
        public override int WindowHeight
        {
            get { return System.Console.WindowHeight; }
            set { System.Console.WindowHeight = value; }
        }
        public override int BufferWidth
        {
            get { return System.Console.BufferWidth; }
            set { System.Console.BufferWidth = value; }
        }
        public override int BufferHeight
        {
            get { return System.Console.BufferHeight; }
            set { System.Console.BufferHeight = value; }
        }

        public override void Clear()
        {
            Console.SetCursorPosition(0, 0);
            StringBuilder Blankness = new StringBuilder();
            Blankness.Length = BufferWidth * BufferHeight;
            Console.Write(Blankness.ToString());

            Console.SetCursorPosition(0, 0);
        }
    }

    public class MonoSystem : ISystem
    {
        private readonly ISystem_Console fConsole = new MonoConsole();
        public override ISystem_Console Console
        {
            get
            {
                return fConsole;
            }
        }
    }
    public class MonoConsole : ISystem_Console
    {
        private ConsoleColor fLastSetForeground = ConsoleColor.White;
        private ConsoleColor fLastSetBackground = ConsoleColor.Black;

        public override ConsoleColor ForegroundColor
        {
            get
            {
                return this.fLastSetForeground;
            }
            set
            {
                switch (value)
                {
                    case ConsoleColor.Black:
                        System.Console.Write("\x1b[30m");
                        break;
                    case ConsoleColor.DarkRed:
                        System.Console.Write("\x1b[31m");
                        break;
                    case ConsoleColor.DarkGreen:
                        System.Console.Write("\x1b[32m");
                        break;
                    case ConsoleColor.DarkYellow:
                        System.Console.Write("\x1b[33m");
                        break;
                    case ConsoleColor.DarkBlue:
                        System.Console.Write("\x1b[34m");
                        break;
                    case ConsoleColor.DarkMagenta:
                        System.Console.Write("\x1b[35m");
                        break;
                    case ConsoleColor.DarkCyan:
                        System.Console.Write("\x1b[36m");
                        break;
                    case ConsoleColor.Gray:
                        System.Console.Write("\x1b[37m");
                        break;

                    case ConsoleColor.DarkGray:
                        System.Console.Write("\x1b[1;30m");
                        break;
                    case ConsoleColor.Red:
                        System.Console.Write("\x1b[1;31m");
                        break;
                    case ConsoleColor.Green:
                        System.Console.Write("\x1b[1;32m");
                        break;
                    case ConsoleColor.Yellow:
                        System.Console.Write("\x1b[1;33m");
                        break;
                    case ConsoleColor.Blue:
                        System.Console.Write("\x1b[1;34m");
                        break;
                    case ConsoleColor.Magenta:
                        System.Console.Write("\x1b[1;35m");
                        break;
                    case ConsoleColor.Cyan:
                        System.Console.Write("\x1b[1;36m");
                        break;
                    case ConsoleColor.White:
                        System.Console.Write("\x1b[1;37m");
                        break;
                }
            }
        }
        public override ConsoleColor BackgroundColor
        {
            get
            {
                return this.fLastSetBackground;
            }
            set
            {
                switch (value)
                {
                    case ConsoleColor.Black:
                        System.Console.Write("\x1b[40m");
                        break;
                    case ConsoleColor.DarkRed:
                        System.Console.Write("\x1b[41m");
                        break;
                    case ConsoleColor.DarkGreen:
                        System.Console.Write("\x1b[42m");
                        break;
                    case ConsoleColor.DarkYellow:
                        System.Console.Write("\x1b[43m");
                        break;
                    case ConsoleColor.DarkBlue:
                        System.Console.Write("\x1b[44m");
                        break;
                    case ConsoleColor.DarkMagenta:
                        System.Console.Write("\x1b[45m");
                        break;
                    case ConsoleColor.DarkCyan:
                        System.Console.Write("\x1b[46m");
                        break;
                    case ConsoleColor.Gray:
                        System.Console.Write("\x1b[47m");
                        break;

                    case ConsoleColor.DarkGray:
                        System.Console.Write("\x1b[1;40m");
                        break;
                    case ConsoleColor.Red:
                        System.Console.Write("\x1b[1;41m");
                        break;
                    case ConsoleColor.Green:
                        System.Console.Write("\x1b[1;42m");
                        break;
                    case ConsoleColor.Yellow:
                        System.Console.Write("\x1b[1;43m");
                        break;
                    case ConsoleColor.Blue:
                        System.Console.Write("\x1b[1;44m");
                        break;
                    case ConsoleColor.Magenta:
                        System.Console.Write("\x1b[1;45m");
                        break;
                    case ConsoleColor.Cyan:
                        System.Console.Write("\x1b[1;46m");
                        break;
                    case ConsoleColor.White:
                        System.Console.Write("\x1b[1;47m");
                        break;
                }
            }
        }

        // TODO: Make this safe (unsafe defaults).
        public override int WindowWidth
        {
            get { return 80; }
            set { }
        }
        public override int WindowHeight
        {
            get { return 20; }
            set { }
        }
        public override int BufferWidth
        {
            get { return 80; }
            set { }
        }
        public override int BufferHeight
        {
            get { return 300; }
            set { }
        }

        public override void Clear()
        {
            Console.SetCursorPosition(0, 0);            
            StringBuilder Blankness = new StringBuilder();
            Blankness.Length = BufferWidth * BufferHeight;
            Console.Write(Blankness.ToString());

            Console.SetCursorPosition(0, 0);
        }
    }
}