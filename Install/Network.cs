using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IHI.Server.Install
{
    internal static class Network
    {
        const byte STEPS = 4;

        /// <summary>
        /// Run the database configuration installer.
        /// </summary>
        /// <param name="Values">The Dictionary to save the inputted configuration values to.</param>
        internal static void Run(ref Dictionary<string, object> Values)
        {
            Output.SetTitle("Network Configuration");

            Values.Add("network.game.host", GetGameHost());
            Values.Add("network.game.port", GetGamePort());
            Values.Add("network.game.maxconnections", GetGameMaxConnections());
            Values.Add("network.webadmin.port", GetWebAdminPort());
        }

        private static string GetGameHost()
        {
            Output.SetStep(1, STEPS);

            Output.ClearPage();

            Console.WriteLine(">>>    Game Host    <<<");
            Console.WriteLine();
            Console.WriteLine("This is the host (normally an IP) to bind the listener for normal game connections.");
            Console.WriteLine("    WARNING: There is no IP validation in the installer! CHECK YOUR INPUT!");
            Console.WriteLine();
            Console.WriteLine("Examples: 127.0.0.1");
            Console.WriteLine("          192.168.1.123");
            Console.WriteLine("          5.24.246.133");
            Console.WriteLine();
            Console.WriteLine("Default Value: 127.0.0.1");
            Console.WriteLine();
            Console.Write("=> ");

            return Input.GetString("127.0.0.1");
        }

        private static ushort GetGamePort()
        {
            Output.SetStep(2, STEPS);

            Output.ClearPage();

            Console.WriteLine(">>>    Game Port    <<<");
            Console.WriteLine();
            Console.WriteLine("This is the port to bind the listener for normal game connections.");
            Console.WriteLine();
            Console.WriteLine("Examples: 14478");
            Console.WriteLine("          30000");
            Console.WriteLine();
            Console.WriteLine("Default Value: 14478");
            Console.WriteLine();
            Console.Write("=> ");

            return Input.GetUshort(14478);
        }

        private static int GetGameMaxConnections()
        {
            Output.SetStep(3, STEPS);

            Output.ClearPage();

            Console.WriteLine(">>>    Maximum Game Connections    <<<");
            Console.WriteLine();
            Console.WriteLine("This is the maximum amount of allowed concurrent normal game connections.");
            Console.WriteLine();
            Console.WriteLine("Examples: 10");
            Console.WriteLine("          200");
            Console.WriteLine();
            Console.WriteLine("Default Value: 100");
            Console.WriteLine();
            Console.Write("=> ");

            return Input.GetInt(100, 0);
        }

        private static ushort GetWebAdminPort()
        {
            Output.SetStep(4, STEPS);

            Output.ClearPage();

            Console.WriteLine(">>>    WebAdmin Port    <<<");
            Console.WriteLine();
            Console.WriteLine("This is the port to bind the WebAdmin listener.");
            Console.WriteLine();
            Console.WriteLine("Examples: 14480");
            Console.WriteLine("          30002");
            Console.WriteLine();
            Console.WriteLine("Default Value: 14480");
            Console.WriteLine();
            Console.Write("=> ");

            return Input.GetUshort(14480);
        }
    }
}
