using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IHI.Server.Install
{
    internal static class Output
    {
        internal static void SetTitle(string Text)
        {
            int RequiredPadding = Console.BufferWidth - Text.Length;
            
            if((RequiredPadding & 1) == 1) // Is RequiredPadding odd?
            {
                Text += " "; // Yes, make it even.
                RequiredPadding--;
            }

            Text = Text.PadLeft(Text.Length + RequiredPadding/2).PadRight(Console.BufferWidth).PadRight(Console.BufferWidth*2, '=');

            Console.SetCursorPosition(0, 0);
            Console.Write(Text);            
        }

        internal static void SetStep(byte Current, byte Total)
        {
            string Text = Current + "/" + Total;

            Console.SetCursorPosition(0, 2);
            Console.Write(Text.PadLeft(Console.BufferWidth));
        }
        
        internal static void SetStatus(string Text, ConsoleColor Foreground = ConsoleColor.Gray, ConsoleColor Background = ConsoleColor.Black)
        {
            MonoAware.System.Console.ForegroundColor = Foreground;
            MonoAware.System.Console.BackgroundColor = Background;

            if (Text.Length > Console.BufferWidth - 1)
                Text = Text.Substring(0, Console.BufferWidth-1);
            else
                Text = Text.PadRight(Console.BufferWidth-1);

            Console.SetCursorPosition(0, Console.BufferHeight-1);
            Console.Write(Text);

            MonoAware.System.Console.ForegroundColor = ConsoleColor.Gray;
            MonoAware.System.Console.BackgroundColor = ConsoleColor.Black;
        }
        
        internal static void ClearPage()
        {
            Console.SetCursorPosition(0, 6);

            StringBuilder Blankness = new StringBuilder();
            Blankness.Length = Console.BufferWidth * (Console.BufferHeight - 7);

            Console.Write(Blankness.ToString());
            Console.SetCursorPosition(0, 6);
        }
    }
}
