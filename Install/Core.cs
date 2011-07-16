using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IHI.Server.Install
{
    internal static class Core
    {
        private static Dictionary<string, object> ReturnValues;

        internal static Dictionary<string, object> Run()
        {
            Console.WriteLine("Installer Ready, press any key");
            Console.ReadKey();

            MonoAware.System.Console.Clear();

            MonoAware.System.Console.ForegroundColor = ConsoleColor.Gray;

            ReturnValues = new Dictionary<string, object>();
            StandardOut.Run(ref ReturnValues);
            Database.Run(ref ReturnValues);
            Network.Run(ref ReturnValues);
            return ReturnValues;
        }
    }
}
