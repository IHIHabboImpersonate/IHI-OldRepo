using System;
using System.Collections.Generic;

namespace IHI.Server.Install
{
    public class IntStep : Step
    {
        private int _default;
        private int _maximum;
        private int _minimum;

        public IntStep(string title = "", string description = "", ICollection<string> examples = null, int @default = 0,
                       int minimum = int.MinValue, int maximum = int.MaxValue)
        {
            Title = title;
            Description = description;
            Examples = examples;
            _default = @default;
            _minimum = minimum;
            _maximum = maximum;
        }

        public IntStep SetDefault(int @default)
        {
            _default = @default;
            return this;
        }

        public IntStep SetMinimum(int minimum)
        {
            _minimum = minimum;
            return this;
        }

        public IntStep SetMaximum(int maximum)
        {
            _maximum = maximum;
            return this;
        }

        public override object Run()
        {
            CoreManager.GetInstallerCore().GetStandardOut().SetPage(ToString(_default.ToString()));

            var inputString = Console.ReadLine();

            if (inputString.Length == 0)
                return _default;

            int inputValue;
            if (int.TryParse(inputString, out inputValue))
            {
                if (inputValue < _minimum)
                    throw new InputException("Given input is lower than mimimum value [ " + inputValue + " < " +
                                             _minimum + " ]");
                if (inputValue > _maximum)
                    throw new InputException("Given input is higher than maximum value [ " + inputValue + " > " +
                                             _maximum + " ]");
                return inputValue;
            }
            throw new InputException("Given input is could not be parsed as a signed int [ " + inputValue + " ]");
        }
    }
}