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
    public class Category
    {
        private readonly IDictionary<string, Step> _steps;
        private int _currentStep = 1;

        public Category()
        {
            _steps = new Dictionary<string, Step>();
        }

        public Category AddStep(string installerValueID, Step step)
        {
            _steps.Add(installerValueID, step);
            return this;
        }

        public IDictionary<string, object> Run()
        {
            IDictionary<string, object> installerOutputValues = new Dictionary<string, object>();

            foreach (KeyValuePair<string, Step> step in _steps)
            {
                Retry:
                try
                {
                    object value = step.Value.Run();
                    installerOutputValues.Add(step.Key, value);
                    _currentStep++;
                }
                catch (InputException e)
                {
                    e.Display();

                    Console.SetCursorPosition(3, 15);
                    Console.Write("".PadRight(UnixAware.System.Console.BufferWidth - 3));
                    Console.SetCursorPosition(3, 15);

                    goto Retry;
                }
            }
            return installerOutputValues;
        }
    }
}