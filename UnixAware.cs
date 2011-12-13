using System;
using System.Text;
using IHI.Server.MA;

namespace IHI.Server
{
    public static class UnixAware
    {
        public static MA.System System { get; private set; }

        internal static void Init(bool mono)
        {
            if (!mono)
            {
                System = new NetSystem();
            }
            else
            {
                System = new MonoSystem();
            }
        }
    }
}

namespace IHI.Server.MA
{
    public abstract class System
    {
        public abstract SystemConsole Console { get; }
    }

    public abstract class SystemConsole
    {
        public abstract ConsoleColor ForegroundColor { get; set; }
        public abstract ConsoleColor BackgroundColor { get; set; }
        public abstract int WindowWidth { get; set; }
        public abstract int WindowHeight { get; set; }
        public abstract int BufferWidth { get; set; }
        public abstract int BufferHeight { get; set; }
        public abstract void Clear();
    }

    public class NetSystem : System
    {
        private readonly SystemConsole _console = new NetConsole();

        public override SystemConsole Console
        {
            get { return _console; }
        }
    }

    public class NetConsole : SystemConsole
    {
        public override ConsoleColor ForegroundColor
        {
            get { return Console.ForegroundColor; }
            set { Console.ForegroundColor = value; }
        }

        public override ConsoleColor BackgroundColor
        {
            get { return Console.BackgroundColor; }
            set { Console.BackgroundColor = value; }
        }

        public override int WindowWidth
        {
            get { return Console.WindowWidth; }
            set { Console.WindowWidth = value; }
        }

        public override int WindowHeight
        {
            get { return Console.WindowHeight; }
            set { Console.WindowHeight = value; }
        }

        public override int BufferWidth
        {
            get { return Console.BufferWidth; }
            set { Console.BufferWidth = value; }
        }

        public override int BufferHeight
        {
            get { return Console.BufferHeight; }
            set { Console.BufferHeight = value; }
        }

        public override void Clear()
        {
            Console.SetCursorPosition(0, 0);
            var blankness = new StringBuilder {Length = BufferWidth*BufferHeight};
            Console.Write(blankness.ToString());

            Console.SetCursorPosition(0, 0);
        }
    }

    public class MonoSystem : System
    {
        private readonly SystemConsole _console = new MonoConsole();

        public override SystemConsole Console
        {
            get { return _console; }
        }
    }

    public class MonoConsole : SystemConsole
    {
        private ConsoleColor _lastSetBackground = ConsoleColor.Black;
        private ConsoleColor _lastSetForeground = ConsoleColor.White;

        public override ConsoleColor ForegroundColor
        {
            get { return _lastSetForeground; }
            set
            {
                _lastSetForeground = value;
                switch (value)
                {
                    case ConsoleColor.Black:
                        Console.Write("\x1b[30m");
                        break;
                    case ConsoleColor.DarkRed:
                        Console.Write("\x1b[31m");
                        break;
                    case ConsoleColor.DarkGreen:
                        Console.Write("\x1b[32m");
                        break;
                    case ConsoleColor.DarkYellow:
                        Console.Write("\x1b[33m");
                        break;
                    case ConsoleColor.DarkBlue:
                        Console.Write("\x1b[34m");
                        break;
                    case ConsoleColor.DarkMagenta:
                        Console.Write("\x1b[35m");
                        break;
                    case ConsoleColor.DarkCyan:
                        Console.Write("\x1b[36m");
                        break;
                    case ConsoleColor.Gray:
                        Console.Write("\x1b[37m");
                        break;

                    case ConsoleColor.DarkGray:
                        Console.Write("\x1b[1;30m");
                        break;
                    case ConsoleColor.Red:
                        Console.Write("\x1b[1;31m");
                        break;
                    case ConsoleColor.Green:
                        Console.Write("\x1b[1;32m");
                        break;
                    case ConsoleColor.Yellow:
                        Console.Write("\x1b[1;33m");
                        break;
                    case ConsoleColor.Blue:
                        Console.Write("\x1b[1;34m");
                        break;
                    case ConsoleColor.Magenta:
                        Console.Write("\x1b[1;35m");
                        break;
                    case ConsoleColor.Cyan:
                        Console.Write("\x1b[1;36m");
                        break;
                    case ConsoleColor.White:
                        Console.Write("\x1b[1;37m");
                        break;
                }
            }
        }

        public override ConsoleColor BackgroundColor
        {
            get { return _lastSetBackground; }
            set
            {
                _lastSetBackground = value;
                switch (value)
                {
                    case ConsoleColor.Black:
                        Console.Write("\x1b[40m");
                        break;
                    case ConsoleColor.DarkRed:
                        Console.Write("\x1b[41m");
                        break;
                    case ConsoleColor.DarkGreen:
                        Console.Write("\x1b[42m");
                        break;
                    case ConsoleColor.DarkYellow:
                        Console.Write("\x1b[43m");
                        break;
                    case ConsoleColor.DarkBlue:
                        Console.Write("\x1b[44m");
                        break;
                    case ConsoleColor.DarkMagenta:
                        Console.Write("\x1b[45m");
                        break;
                    case ConsoleColor.DarkCyan:
                        Console.Write("\x1b[46m");
                        break;
                    case ConsoleColor.Gray:
                        Console.Write("\x1b[47m");
                        break;

                    case ConsoleColor.DarkGray:
                        Console.Write("\x1b[1;40m");
                        break;
                    case ConsoleColor.Red:
                        Console.Write("\x1b[1;41m");
                        break;
                    case ConsoleColor.Green:
                        Console.Write("\x1b[1;42m");
                        break;
                    case ConsoleColor.Yellow:
                        Console.Write("\x1b[1;43m");
                        break;
                    case ConsoleColor.Blue:
                        Console.Write("\x1b[1;44m");
                        break;
                    case ConsoleColor.Magenta:
                        Console.Write("\x1b[1;45m");
                        break;
                    case ConsoleColor.Cyan:
                        Console.Write("\x1b[1;46m");
                        break;
                    case ConsoleColor.White:
                        Console.Write("\x1b[1;47m");
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
            var blankness = new StringBuilder {Length = BufferWidth*BufferHeight};
            Console.Write(blankness.ToString());

            Console.SetCursorPosition(0, 0);
        }
    }
}