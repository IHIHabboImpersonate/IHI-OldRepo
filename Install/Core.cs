using System;
using System.Collections.Generic;

namespace IHI.Server.Install
{
    public class Core
    {
        private readonly IDictionary<string, Category> _categories;
        private readonly IDictionary<string, IDictionary<string, object>> _installerOutputValues;
        public InstallerIn In
        {
            get;
            private set;
        }
        public InstallerOut Out
        {
            get;
            private set;
        }

        internal Core()
        {
            _categories = new Dictionary<string, Category>();
            _installerOutputValues = new Dictionary<string, IDictionary<string, object>>();
            
            In = new InstallerIn();
            Out = new InstallerOut();
        }

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

            foreach (var category in _categories)
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