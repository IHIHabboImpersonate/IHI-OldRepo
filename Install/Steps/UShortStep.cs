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
using System.Collections.Generic;

namespace IHI.Server.Install
{
    public class UShortStep : Step
    {
        private ushort _default;
        private ushort _maximum;
        private ushort _minimum;

        public UShortStep(string title = "", string description = "", ICollection<string> examples = null,
                          ushort @default = (ushort) 0, ushort minimum = ushort.MinValue,
                          ushort maximum = ushort.MaxValue)
        {
            Title = title;
            Description = description;
            Examples = examples;
            _default = @default;
            _minimum = minimum;
            _maximum = maximum;
        }

        public UShortStep SetDefault(ushort @default)
        {
            _default = @default;
            return this;
        }

        public UShortStep SetMinimum(ushort minimum)
        {
            _minimum = minimum;
            return this;
        }

        public UShortStep SetMaximum(ushort maximum)
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

            ushort inputValue;
            if (ushort.TryParse(inputString, out inputValue))
            {
                if (inputValue < _minimum)
                    throw new InputException("Given input is lower than mimimum value [ " + inputValue + " < " +
                                             _minimum + " ]");
                if (inputValue > _maximum)
                    throw new InputException("Given input is higher than maximum value [ " + inputValue + " > " +
                                             _maximum + " ]");
                return inputValue;
            }
            throw new InputException("Given input is could not be parsed as an unsigned short [ " + inputValue + " ]");
        }
    }
}