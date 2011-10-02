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

            foreach (var step in _steps)
            {
                Retry:
                try
                {
                    var value = step.Value.Run();
                    installerOutputValues.Add(step.Key, value);
                    _currentStep++;
                }
                catch (InputException e)
                {
                    e.Display();

                    Console.SetCursorPosition(3, 15);
                    Console.Write("".PadRight(MonoAware.System.Console.BufferWidth - 3));
                    Console.SetCursorPosition(3, 15);

                    goto Retry;
                }
            }
            return installerOutputValues;
        }
    }
}