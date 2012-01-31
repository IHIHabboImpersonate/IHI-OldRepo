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
using System;
using System.Text;

namespace IHI.Server.Install
{
    public class InstallerOut
    {
        public string Title
        {
            set
            {
                int requiredPadding = Console.BufferWidth - value.Length;

                if ((requiredPadding & 1) == 1) // Is RequiredPadding odd?
                {
                    value += " "; // Yes, make it even.
                    requiredPadding--;
                }

                value =
                    value.PadLeft(value.Length + requiredPadding/2)
                        .PadRight(Console.BufferWidth)
                        .PadRight(Console.BufferWidth*2, '=');

                Console.SetCursorPosition(0, 0);
                Console.Write(value);
            }
        }

        public InstallerOut SetStep(byte current, byte total)
        {
            string text = current + "/" + total;

            Console.SetCursorPosition(0, 2);
            Console.Write(text.PadLeft(Console.BufferWidth));
            return this;
        }

        public InstallerOut SetStatus(string text, ConsoleColor foreground = ConsoleColor.Gray,
                                      ConsoleColor background = ConsoleColor.Black)
        {
            UnixAware.System.Console.ForegroundColor = foreground;
            UnixAware.System.Console.BackgroundColor = background;

            text = text.Length > Console.BufferWidth - 1
                       ? text.Substring(0, Console.BufferWidth - 1)
                       : text.PadRight(Console.BufferWidth - 1);

            Console.SetCursorPosition(0, Console.BufferHeight - 1);
            Console.Write(text);

            UnixAware.System.Console.ForegroundColor = ConsoleColor.Gray;
            UnixAware.System.Console.BackgroundColor = ConsoleColor.Black;
            return this;
        }

        public InstallerOut ClearPage()
        {
            Console.SetCursorPosition(0, 6);

            StringBuilder blankness = new StringBuilder
                                          {
                                              Length = Console.BufferWidth*(Console.BufferHeight - 7)
                                          };

            Console.Write(blankness.ToString());
            Console.SetCursorPosition(0, 6);
            return this;
        }

        public InstallerOut OverwritePageContents(string contents)
        {
            ClearPage();
            Console.SetCursorPosition(0, 6);
            Console.Write(contents);
            return this;
        }
    }
}