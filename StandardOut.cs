using System;

namespace IHI.Server
{
    public class StandardOut
    {
        private bool fHidden;
        private object[,] fOutputHistory;
        private StandardOutImportance fImportance;

        internal StandardOut()
        {
            this.fOutputHistory = new object[Console.BufferHeight, 3];
        }

        /// <summary>
        /// Output a debug message.
        /// </summary>
        /// <param name="Message">The message to output.</param>
        public StandardOut PrintDebug(string Message)
        {
            if (this.fImportance <= StandardOutImportance.Debug)
                Raw("DEBUG", Message, ConsoleColor.White, true);
            return this;
        }
        /// <summary>
        /// Output a warning message.
        /// </summary>
        /// <param name="Message">The message to output.</param>
        public StandardOut PrintWarning(string Message)
        {
            if (this.fImportance <= StandardOutImportance.Warning)
                Raw("WARNING", Message, ConsoleColor.DarkYellow, true);
            return this;
        }
        /// <summary>
        /// Output an error message.
        /// </summary>
        /// <param name="Message">The message to output.</param>
        public StandardOut PrintError(string Message)
        {
            if (this.fImportance <= StandardOutImportance.Error)
                Raw("ERROR", Message, ConsoleColor.Red, true);
            return this;
        }
        /// <summary>
        /// Output an exception in a formatted manner.
        /// </summary>
        /// <param name="e">The exception to output.</param>
        public StandardOut PrintException(Exception e)
        {
            PrintError(e.Message);
            PrintDebug(e.StackTrace);
            return this;
        }
        /// <summary>
        /// Output a general message.
        /// Use this for most things.
        /// </summary>
        /// <param name="Message">The message to output.</param>
        public StandardOut PrintNotice(string Message)
        {
            if (this.fImportance <= StandardOutImportance.Notice)
                Raw("NOTICE", Message, ConsoleColor.Gray, true);
            return this;
        }

        /// <summary>
        /// Output an important message.
        /// Use when the message is important but not debugging, a warning or an error.
        /// </summary>
        /// <param name="Message">The message to output.</param>
        public StandardOut PrintImportant(string Message)
        {
            if (this.fImportance <= StandardOutImportance.Important)
                Raw("IMPORTANT", Message, ConsoleColor.Green, true);
            return this;
        }

        /// <summary>
        /// Clear the output.
        /// </summary>
        public StandardOut Clear()
        {
            lock (this)
            {
                MonoAware.System.Console.Clear();
                this.fOutputHistory = new object[Console.BufferHeight, 3];
                return this;
            }
        }

        /// <summary>
        /// Check if the output is hidden from the screen or not.
        /// </summary>
        /// <returns>True if the output is hidden, false otherwise.</returns>
        public bool IsHidden()
        {
            return this.fHidden;
        }
        /// <summary>
        /// Set whether the output is hidden from the screen or not.
        /// </summary>
        /// <returns>True to hide the output, false to show it.</returns>
        internal void SetHidden(bool Hidden)
        {
            if (this.fHidden != Hidden)
            {
                this.fHidden = Hidden;

                if (!fHidden)
                    PrintHistroy();
            }
        }

        private void Raw(string Header, string Message, ConsoleColor Colour, bool Record)
        {
            if (Record)
                PushHistroy(Header, Message, Colour);
            if (this.fHidden)
                return;

            lock (this)
            {
                MonoAware.System.Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write(DateTime.Now.ToLongTimeString() + " >> [" + Header + "] ");
                MonoAware.System.Console.ForegroundColor = Colour;
                Console.WriteLine(Message);
            }
        }
        private void PushHistroy(string Header, string Message, ConsoleColor Colour)
        {
            ushort Last = (ushort)this.fOutputHistory.GetLength(0);

            lock (this.fOutputHistory)
            {
                for (ushort i = 1; i < Last; i++)
                {
                    this.fOutputHistory[i - 1, 0] = this.fOutputHistory[i, 0];
                    this.fOutputHistory[i - 1, 1] = this.fOutputHistory[i, 1];
                    this.fOutputHistory[i - 1, 2] = this.fOutputHistory[i, 2];
                }


                this.fOutputHistory[Last - 1, 0] = Header;
                this.fOutputHistory[Last - 1, 1] = Message;
                this.fOutputHistory[Last - 1, 2] = Colour;
            }
        }
        private void PrintHistroy()
        {
            lock (this.fOutputHistory)
            {
                for (ushort i = 1; i < this.fOutputHistory.Length; i++)
                {
                    Raw((string)this.fOutputHistory[i, 0], (string)this.fOutputHistory[i, 1], (ConsoleColor)this.fOutputHistory[i, 2], false);
                }
            }
        }

        /// <summary>
        /// Check if the output is hidden from the screen or not.
        /// </summary>
        /// <returns>True if the output is hidden, false otherwise.</returns>
        public StandardOutImportance GetImportance()
        {
            return this.fImportance;
        }
        /// <summary>
        /// Set whether the output is hidden from the screen or not.
        /// </summary>
        /// <returns>True to hide the output, false to show it.</returns>
        public void SetImportance(StandardOutImportance Importance)
        {
            if (this.fImportance != Importance)
            {
                Raw("IMPORTANT", "StandardOut Importance Changed [ " + this.fImportance.ToString() + " -> " + Importance.ToString() + " ]", ConsoleColor.Yellow, true);
                this.fImportance = Importance;
            }
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
