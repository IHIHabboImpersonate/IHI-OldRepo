using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IHI.Server.Install
{
    internal static class StandardOut
    {
        const byte STEPS = 1;

        /// <summary>
        /// Run the database configuration installer.
        /// </summary>
        /// <param name="Values">The Dictionary to save the inputted configuration values to.</param>
        internal static void Run(ref Dictionary<string, object> Values)
        {
            Output.SetTitle("Standard Out Configuration");

            Values.Add("standardout.importance", GetImportance());
        }

        private static byte GetImportance()
        {
            Output.SetStep(1, STEPS);

            Output.ClearPage();

            Console.WriteLine(">>>    Default Importance    <<<");
            Console.WriteLine();
            Console.WriteLine("This is the minimum importance level that messages must have to be printed to standard out.");
            Console.WriteLine();
            Console.WriteLine("Values:   0: DEBUG");
            Console.WriteLine("          1: NOTICE");
            Console.WriteLine("          2: IMPORTANT");
            Console.WriteLine("          3: WARNING");
            Console.WriteLine("          4: ERROR");
            Console.WriteLine();
            Console.WriteLine("Default Value: 1");
            Console.WriteLine();
            Console.Write("=> ");

            return Input.GetByte(1, 0, 5);
        }
    }
}
