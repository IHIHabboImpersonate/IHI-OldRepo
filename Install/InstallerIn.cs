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
    public class InstallerIn
    {
        /// <summary>
        ///   Read an unrestricted string.
        /// </summary>
        /// <param name = "Default">A value to return if the read string is blank.</param>
        internal string GetString(string Default = "")
        {
            string Input = Console.ReadLine();
            if (Input.Length == 0)
                return Default;
            return Input;
        }

        /// <summary>
        ///   Read an unsigned short.
        ///   If the read line is not a valid ushort between Minimum and Maximum an exception of type IHI.Server.Install.InputException is thrown.
        /// </summary>
        /// <param name = "Default">A value to return if the read line is blank.</param>
        /// <param name = "Minimum">The minimum value to accept.</param>
        /// <param name = "Maximum">The maxumum value to accept.</param>
        internal ushort GetUshort(ushort Default = 0, ushort Minimum = ushort.MinValue, ushort Maximum = ushort.MaxValue)
        {
            string Input = Console.ReadLine();

            if (Input.Length == 0)
                return Default;

            ushort Value = 0;

            if (ushort.TryParse(Input, out Value))
            {
                if (Value < Minimum)
                {
                    throw new InputException("Given input is lower than mimimum value [ " + Value + " < " + Minimum +
                                             " ]");
                }
                if (Value > Maximum)
                {
                    throw new InputException("Given input is higher than maximum value [ " + Value + " > " + Maximum +
                                             " ]");
                }
                return Value;
            }
            throw new InputException("Given input is could not be parsed as an unsigned short [ " + Input + " ]");
        }

        /// <summary>
        ///   Read a byte.
        ///   If the read line is not a valid byte between Minimum and Maximum an exception of type IHI.Server.Install.InputException is thrown.
        /// </summary>
        /// <param name = "Default">A value to return if the read line is blank.</param>
        /// <param name = "Minimum">The minimum value to accept.</param>
        /// <param name = "Maximum">The maxumum value to accept.</param>
        internal byte GetByte(byte Default = 0, byte Minimum = byte.MinValue, byte Maximum = byte.MaxValue)
        {
            string Input = Console.ReadLine();

            if (Input.Length == 0)
                return Default;

            byte Value = 0;

            if (byte.TryParse(Input, out Value))
            {
                if (Value < Minimum)
                {
                    throw new InputException("Given input is lower than mimimum value [ " + Value + " < " + Minimum +
                                             " ]");
                }
                if (Value > Maximum)
                {
                    throw new InputException("Given input is higher than maximum value [ " + Value + " > " + Maximum +
                                             " ]");
                }
                return Value;
            }
            throw new InputException("Given input is could not be parsed as a byte [ " + Input + " ]");
        }

        /// <summary>
        ///   Read a signed int.
        ///   If the read line is not a valid int between Minimum and Maximum an exception of type IHI.Server.Install.InputException is thrown.
        /// </summary>
        /// <param name = "Default">A value to return if the read line is blank.</param>
        /// <param name = "Minimum">The minimum value to accept.</param>
        /// <param name = "Maximum">The maxumum value to accept.</param>
        internal int GetInt(int Default = 0, int Minimum = int.MinValue, int Maximum = int.MaxValue)
        {
            string Input = Console.ReadLine();

            if (Input.Length == 0)
                return Default;

            int Value = 0;

            if (int.TryParse(Input, out Value))
            {
                if (Value < Minimum)
                {
                    throw new InputException("Given input is lower than mimimum value [ " + Value + " < " + Minimum +
                                             " ]");
                }
                if (Value > Maximum)
                {
                    throw new InputException("Given input is higher than maximum value [ " + Value + " > " + Maximum +
                                             " ]");
                }
                return Value;
            }
            throw new InputException("Given input is could not be parsed as a signed int [ " + Input + " ]");
        }

        /// <summary>
        ///   Read a line but mask the password as it is typed.
        /// </summary>
        internal string GetPassword()
        {
            StringBuilder Password = new StringBuilder();

            Stack<Point> CursorHistory = new Stack<Point>();

            // TODO: Mono Test;
            ConsoleKeyInfo Key;
            while ((Key = Console.ReadKey(true)).Key != ConsoleKey.Enter)
            {
                if (Key.Key != ConsoleKey.Backspace)
                {
                    CursorHistory.Push(new Point(Console.CursorLeft, Console.CursorTop));
                    Password.Append(Key.KeyChar);
                    Console.Write('*');
                }
                else
                {
                    if (Password.Length == 0)
                        continue;

                    Password.Length--;

                    Point BackCursor = CursorHistory.Pop();
                    Console.SetCursorPosition(BackCursor.X, BackCursor.Y);
                    Console.Write(' ');
                    Console.SetCursorPosition(BackCursor.X, BackCursor.Y);
                }
            }

            return Password.ToString();
        }
    }
}