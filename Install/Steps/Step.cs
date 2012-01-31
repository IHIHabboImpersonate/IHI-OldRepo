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
using System.Collections.Generic;
using System.Text;

namespace IHI.Server.Install
{
    public abstract class Step
    {
        protected string Description;
        protected ICollection<string> Examples;
        protected string Title;

        public Step SetTitle(string title)
        {
            Title = title;
            return this;
        }

        public Step SetDescription(string description)
        {
            Description = description;
            return this;
        }

        public Step AddExample(string example)
        {
            Examples.Add(example);
            return this;
        }

        public Step RemoveExample(string example)
        {
            Examples.Remove(example);
            return this;
        }

        protected string ToString(string defaultValue)
        {
            StringBuilder outputBuilder = new StringBuilder();

            outputBuilder.
                Append(">>>   ").
                Append(Title).
                AppendLine("   <<<").
                AppendLine().
                AppendLine(Description).
                Append("Examples: ");

            bool firstExample = true;
            foreach (string example in Examples)
            {
                if (!firstExample)
                    outputBuilder.Append("          ");
                else
                    firstExample = false;
                outputBuilder.AppendLine(example);
            }

            outputBuilder.
                AppendLine().
                Append("Default Value: ").
                AppendLine(defaultValue).
                AppendLine().
                Append("=> ");

            return outputBuilder.ToString();
        }

        public abstract object Run();
    }
}