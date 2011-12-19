using System;
using System.Text;

namespace IHI.Server.Install
{
    public class StandardOut
    {
        internal StandardOut SetCategoryTitle(string text)
        {
            var requiredPadding = Console.BufferWidth - text.Length;

            if ((requiredPadding & 1) == 1) // Is RequiredPadding odd?
            {
                text += " "; // Yes, make it even.
                requiredPadding--;
            }

            text =
                text.PadLeft(text.Length + requiredPadding/2).PadRight(Console.BufferWidth).PadRight(
                    Console.BufferWidth*2, '=');

            Console.SetCursorPosition(0, 0);
            Console.Write(text);
            return this;
        }

        internal StandardOut SetStep(byte current, byte total)
        {
            var text = current + "/" + total;

            Console.SetCursorPosition(0, 2);
            Console.Write(text.PadLeft(Console.BufferWidth));
            return this;
        }

        internal StandardOut SetStatus(string text, ConsoleColor foreground = ConsoleColor.Gray,
                                       ConsoleColor background = ConsoleColor.Black)
        {
            UnixAware.System.Console.ForegroundColor = foreground;
            UnixAware.System.Console.BackgroundColor = background;

            text = text.Length > Console.BufferWidth - 1 ? text.Substring(0, Console.BufferWidth - 1) : text.PadRight(Console.BufferWidth - 1);

            Console.SetCursorPosition(0, Console.BufferHeight - 1);
            Console.Write(text);

            UnixAware.System.Console.ForegroundColor = ConsoleColor.Gray;
            UnixAware.System.Console.BackgroundColor = ConsoleColor.Black;
            return this;
        }

        internal StandardOut ClearPage()
        {
            Console.SetCursorPosition(0, 6);

            var blankness = new StringBuilder
                                {
                                    Length = Console.BufferWidth*(Console.BufferHeight - 7)
                                };

            Console.Write(blankness.ToString());
            Console.SetCursorPosition(0, 6);
            return this;
        }

        internal StandardOut SetPage(string contents)
        {
            ClearPage();
            Console.SetCursorPosition(0, 6);
            Console.Write(contents);
            return this;
        }
    }
}