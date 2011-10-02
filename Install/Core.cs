using System;
using System.Collections.Generic;

namespace IHI.Server.Install
{
    public class Core
    {
        private readonly IDictionary<string, Category> _categories;
        private readonly IDictionary<string, IDictionary<string, object>> _installerOutputValues;
        private readonly StandardOut _standardOut;

        internal Core()
        {
            _categories = new Dictionary<string, Category>();
            _installerOutputValues = new Dictionary<string, IDictionary<string, object>>();
            _standardOut = new StandardOut();
        }

        public StandardOut GetStandardOut()
        {
            return _standardOut;
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
                    GetServerCore().
                    GetStandardOut().
                    PrintNotice("Installer => No installation tasks detected.");
                return this;
            }
            CoreManager.
                GetServerCore().
                GetStandardOut().
                PrintImportant("Installer => Installation tasks detected!").
                PrintNotice("Standard Out Formatting => Disabled (Installer)").
                SetHidden(true);

            Console.WriteLine("Press any key to continue.");

            Console.ReadKey();

            MonoAware.System.Console.Clear();
            MonoAware.System.Console.ForegroundColor = ConsoleColor.Gray;

            foreach (var category in _categories)
            {
                _installerOutputValues.Add(
                    category.Key,
                    category.Value.Run());
            }

            CoreManager.
                GetServerCore().
                GetStandardOut().
                SetHidden(false).
                PrintNotice("Standard Out Formatting => Enabled (Installer)");
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