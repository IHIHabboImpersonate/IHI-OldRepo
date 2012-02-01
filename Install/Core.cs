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
    public class Core
    {
        private readonly IDictionary<string, Category> _categories;
        private readonly IDictionary<string, IDictionary<string, object>> _installerOutputValues;

        internal Core()
        {
            _categories = new Dictionary<string, Category>();
            _installerOutputValues = new Dictionary<string, IDictionary<string, object>>();

            In = new InstallerIn();
            Out = new InstallerOut();
        }

        public InstallerIn In { get; private set; }
        public InstallerOut Out { get; private set; }

        public Core AddCategory(string installerCategoryID, Category category)
        {
            _categories.Add(installerCategoryID, category);
            return this;
        }

        internal Core Run()
        {
            if (_categories.Count == 0)
            {
                CoreManager.
                    ServerCore.
                    GetStandardOut().
                    PrintNotice("Installer => No installation tasks detected.");
                return this;
            }
            CoreManager.
                ServerCore.
                GetStandardOut().
                PrintImportant("Installer => Installation tasks detected!").
                PrintNotice("Standard Out => Formatting Disabled (Installer)").
                SetHidden(true);

            Console.WriteLine("Press any key to continue.");

            Console.ReadKey();

            UnixAware.System.Console.Clear();
            UnixAware.System.Console.ForegroundColor = ConsoleColor.Gray;

            foreach (KeyValuePair<string, Category> category in _categories)
            {
                _installerOutputValues.Add(
                    category.Key,
                    category.Value.Run());
            }

            CoreManager.
                ServerCore.
                GetStandardOut().
                SetHidden(false).
                PrintNotice("Standard Out => Formatting Enabled (Installer)");
            return this;
        }

        public object GetInstallerOutputValue(string category, string name)
        {
            if (!_installerOutputValues.ContainsKey(category))
                return null;
            if (!_installerOutputValues[category].ContainsKey(name))
                return null;

            return _installerOutputValues[category][name];
        }
    }
}