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

#endregion

namespace IHI.Server.Install
{
    public class ByteStep : Step
    {
        private byte _default;
        private byte _maximum;
        private byte _minimum;

        public ByteStep(string title = "", string description = "",
                        ICollection<string> examples = null, byte @default = (byte) 0, byte minimum = byte.MinValue,
                        byte maximum = byte.MaxValue)
        {
            Title = title;
            Description = description;
            Examples = examples;
            _default = @default;
            _minimum = minimum;
            _maximum = maximum;
        }

        public ByteStep SetDefault(byte @default)
        {
            _default = @default;
            return this;
        }

        public ByteStep SetMinimum(byte minimum)
        {
            _minimum = minimum;
            return this;
        }

        public ByteStep SetMaximum(byte maximum)
        {
            _maximum = maximum;
            return this;
        }

        public override object Run()
        {
            CoreManager.InstallerCore.Out.OverwritePageContents(ToString(_default.ToString()));

            string inputString = Console.ReadLine();

            if (inputString.Length == 0)
                return _default;

            byte inputValue;
            if (byte.TryParse(inputString, out inputValue))
            {
                if (inputValue < _minimum)
                    throw new InputException("Given input is lower than mimimum value [ " + inputValue + " < " +
                                             _minimum + " ]");
                if (inputValue > _maximum)
                    throw new InputException("Given input is higher than maximum value [ " + inputValue + " > " +
                                             _maximum + " ]");
                return inputValue;
            }
            throw new InputException("Given input is could not be parsed as an unsigned byte [ " + inputValue + " ]");
        }
    }
}