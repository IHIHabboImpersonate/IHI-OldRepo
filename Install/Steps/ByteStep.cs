using System;
using System.Collections.Generic;

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

            var inputString = Console.ReadLine();

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