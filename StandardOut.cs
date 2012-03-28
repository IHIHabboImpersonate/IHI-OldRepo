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
using System.IO;
using System.Text;
using System.Threading;

#endregion

namespace IHI.Server
{
    public class StandardOut
    {
        private bool _hidden;

        /// <summary>
        ///   The past message colours.
        /// </summary>
        private ConsoleColor[] _historyColours;

        /// <summary>
        ///   The past header text.
        /// </summary>
        private string[] _historyHeaders;

        /// <summary>
        ///   The past message text.
        /// </summary>
        private string[] _historyMessages;

        /// <summary>
        ///   The past message timestamps.
        /// </summary>
        private DateTime?[] _historyTimestamps;

        private StandardOutImportance _importance;

        /// <summary>
        ///   The last index of the rolling history arrays that was written to.
        /// </summary>
        private int _lastIndexWritten;

        internal StandardOut()
        {
            _historyHeaders = new string[Console.BufferHeight];
            _historyMessages = new string[Console.BufferHeight];
            _historyColours = new ConsoleColor[Console.BufferHeight];
            _historyTimestamps = new DateTime?[Console.BufferHeight];
            _lastIndexWritten = 0;
        }

        /// <summary>
        ///   Output a debug message.
        /// </summary>
        /// <param name = "message">The message to output.</param>
        public StandardOut PrintDebug(string message)
        {
            if (_importance <= StandardOutImportance.Debug)
                Raw("DEBUG", message, ConsoleColor.White);
            return this;
        }

        /// <summary>
        ///   Output a warning message.
        /// </summary>
        /// <param name = "message">The message to output.</param>
        public StandardOut PrintWarning(string message)
        {
            if (_importance <= StandardOutImportance.Warning)
                Raw("WARNING", message, ConsoleColor.DarkYellow);
            return this;
        }

        /// <summary>
        ///   Output an error message.
        /// </summary>
        /// <param name = "message">The message to output.</param>
        public StandardOut PrintError(string message)
        {
            if (_importance <= StandardOutImportance.Error)
                Raw("ERROR", message, ConsoleColor.Red);
            return this;
        }

        /// <summary>
        ///   Output an exception in a formatted manner.
        /// </summary>
        /// <param name = "e">The exception to output.</param>
        public StandardOut PrintException(Exception e)
        {
            PrintError("Exception => " + e.GetType().FullName);
            PrintError("             " + e.Message);
            PrintError("             Saved to " + SaveExceptionToFile(e));
            return this;
        }

        /// <summary>
        ///   Output a general message.
        ///   Use this for most things.
        /// </summary>
        /// <param name = "message">The message to output.</param>
        public StandardOut PrintNotice(string message)
        {
            if (_importance <= StandardOutImportance.Notice)
                Raw("NOTICE", message, ConsoleColor.Gray);
            return this;
        }

        /// <summary>
        ///   Output an important message.
        ///   Use when the message is important but not debugging, a warning or an error.
        /// </summary>
        /// <param name = "message">The message to output.</param>
        public StandardOut PrintImportant(string message)
        {
            if (_importance <= StandardOutImportance.Important)
                Raw("IMPORTANT", message, ConsoleColor.Green);
            return this;
        }

        /// <summary>
        ///   Clear the output.
        /// </summary>
        public StandardOut Clear()
        {
            lock (this)
            {
                UnixAware.System.Console.Clear();
                _historyHeaders = new string[Console.BufferHeight];
                _historyMessages = new string[Console.BufferHeight];
                _historyColours = new ConsoleColor[Console.BufferHeight];
                _historyTimestamps = new DateTime?[Console.BufferHeight];
                _lastIndexWritten = 0;
                return this;
            }
        }

        /// <summary>
        ///   Check if the output is hidden from the screen or not.
        /// </summary>
        /// <returns>True if the output is hidden, false otherwise.</returns>
        public bool IsHidden()
        {
            return _hidden;
        }

        /// <summary>
        ///   Set whether the output is hidden from the screen or not.
        /// </summary>
        /// <param name = "hidden">True to hide the output, false to show it.</param>
        internal StandardOut SetHidden(bool hidden)
        {
            if (_hidden != hidden)
            {
                _hidden = hidden;

                if (!_hidden)
                    PrintHistroy();
            }
            return this;
        }

        private void Raw(string header, string message, ConsoleColor colour, bool record = true,
                         DateTime? timestamp = null)
        {
            timestamp = timestamp ?? DateTime.Now;

            if (record)
                PushHistroy(header, message, colour, timestamp.Value);
            if (_hidden)
                return;
            if (String.IsNullOrEmpty(header))
            {
                Console.WriteLine();
                return;
            }

            lock (this)
            {
                UnixAware.System.Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write(timestamp.Value.ToLongTimeString() + " >> [" + header + "] ");
                UnixAware.System.Console.ForegroundColor = colour;
                Console.WriteLine(message);
            }
        }

        private void PushHistroy(string header, string message, ConsoleColor colour, DateTime timestamp)
        {
            lock (_historyHeaders)
            {
                if (_lastIndexWritten < Console.BufferHeight - 1)
                    _lastIndexWritten++;
                else
                    _lastIndexWritten = 0;

                _historyHeaders[_lastIndexWritten] = header;
                _historyMessages[_lastIndexWritten] = message;
                _historyColours[_lastIndexWritten] = colour;
                _historyTimestamps[_lastIndexWritten] = timestamp;
            }
        }

        private void PrintHistroy()
        {
            lock (_historyHeaders)
            {
                UnixAware.System.Console.Clear();
                for (int i = _lastIndexWritten + 1; i < _historyHeaders.Length; i++)
                {
                    Raw(_historyHeaders[i], _historyMessages[i], _historyColours[i], false);
                }
                for (int i = 0; i <= _lastIndexWritten; i++)
                {
                    Raw(_historyHeaders[i], _historyMessages[i], _historyColours[i], false);
                }
            }
        }

        /// <summary>
        ///   Check if the output is hidden from the screen or not.
        /// </summary>
        /// <returns>True if the output is hidden, false otherwise.</returns>
        public StandardOutImportance GetImportance()
        {
            return _importance;
        }

        /// <summary>
        ///   Set whether the output is hidden from the screen or not.
        /// </summary>
        /// <returns>True to hide the output, false to show it.</returns>
        public void SetImportance(StandardOutImportance importance)
        {
            if (_importance == importance) return;
            Raw("IMPORTANT", "StandardOut Importance Changed [ " + _importance + " -> " + importance + " ]",
                ConsoleColor.Yellow);
            _importance = importance;
        }


        /// <summary>
        ///   Save data about an exception to file.
        /// </summary>
        /// <param name = "exception">The exception to save.</param>
        /// <returns>The path the file was saved at.</returns>
        private static string SaveExceptionToFile(Exception exception)
        {
            StringBuilder logText = new StringBuilder("IHIEXCEPTION\x01");
            logText.Append("TIME\x02" + DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds + "\x01");

            logText.Append("EXCEPTION-THREAD\x02" + Thread.CurrentThread.Name + "\x01");

            int i = 0;
            while (exception != null)
            {
                logText.Append("EXCEPTION[" + i + "]-TYPE\x02" + exception.GetType().FullName + "\x01");
                logText.Append("EXCEPTION[" + i + "]-MESSAGE\x02" + exception.Message + "\x01");
                logText.Append("EXCEPTION[" + i + "]-STACKTRACE\x02" + exception.StackTrace + "\x01");

                i++;
                exception = exception.InnerException;
            }

            string path = Path.Combine(Environment.CurrentDirectory, "dumps",
                                       "exception-" + DateTime.UtcNow.Ticks + ".ihidump");

            File.WriteAllText(path, logText.ToString());
            return path;
        }
    }


    public enum StandardOutImportance
    {
        Debug,
        Notice,
        Important,
        Warning,
        Error
    }
}