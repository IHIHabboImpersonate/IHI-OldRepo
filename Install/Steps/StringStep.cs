using System;
using System.Collections.Generic;

namespace IHI.Server.Install
{
    public class StringStep : Step
    {
        private string _default;

        public StringStep(string title = "", string description = "", ICollection<string> examples = null,
                          string @default = "")
        {
            Title = title;
            Description = description;
            Examples = examples;
            _default = @default;
        }

        public StringStep SetDefault(string @default)
        {
            _default = @default;
            return this;
        }

        public override object Run()
        {
            CoreManager.GetInstallerCore().GetStandardOut().SetPage(ToString(_default));

            var inputValue = Console.ReadLine();
            return (inputValue.Length == 0 ? _default : inputValue);
        }
    }
}