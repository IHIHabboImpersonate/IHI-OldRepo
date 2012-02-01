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
using System.Collections.Generic;
using System.Drawing;
using System.Text;

#endregion

namespace IHI.Server.Install
{
    public class PasswordStep : Step
    {
        public PasswordStep(string title = "", string description = "")
        {
            Title = title;
            Description = description;
            Examples = new[] {"No examples!", "That would be a security risk for morons who might use them!"};
        }

        public override object Run()
        {
            CoreManager.InstallerCore.Out.OverwritePageContents(
                ToString("No default! If you leave it blank the password is considered blank!"));

            StringBuilder password = new StringBuilder();

            Stack<Point> cursorHistory = new Stack<Point>();

            // TODO: Mono Test;
            ConsoleKeyInfo key;
            while ((key = Console.ReadKey(true)).Key != ConsoleKey.Enter)
            {
                if (key.Key != ConsoleKey.Backspace)
                {
                    cursorHistory.Push(new Point(Console.CursorLeft, Console.CursorTop));
                    password.Append(key.KeyChar);
                    Console.Write('*');
                }
                else
                {
                    if (password.Length == 0)
                        continue;

                    password.Length--;

                    Point backCursor = cursorHistory.Pop();
                    Console.SetCursorPosition(backCursor.X, backCursor.Y);
                    Console.Write(' ');
                    Console.SetCursorPosition(backCursor.X, backCursor.Y);
                }
            }
            return password.ToString();
        }
    }
}