using System;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

namespace IHI.Server
{
    public static class EntryPoint
    {
        [STAThreadAttribute]
        public static void Main(string[] arguments)
        {
            var monoMode = false;
            var configFile = "config.xml";
            var disableAutoExit = false;

            var nameValueRegex = new Regex("^--(?<name>[\\w-]+)=(?<value>.+)$");

            foreach (var argument in arguments)
            {
                var nameValueMatch = nameValueRegex.Match(argument);
                var name = nameValueMatch.Groups["name"].Value;
                var value = nameValueMatch.Groups["value"].Value;

                switch (name)
                {
                    case "mode":
                        {
                            if (value.ToLower() == "mono")
                                monoMode = true;
                            break;
                        }
                    case "config-file":
                        {
                            configFile = value;
                            break;
                        }
                    case "auto-exit":
                        {
                            switch (value.ToLower())
                            {
                                case "false":
                                case "off":
                                case "no":
                                case "disable":
                                case "disabled":
                                    disableAutoExit = true;
                                    break;
                            }
                            break;
                        }
                }
            }

            MonoAware.Init(monoMode);

            MonoAware.System.Console.BackgroundColor = ConsoleColor.Black;
            MonoAware.System.Console.ForegroundColor = ConsoleColor.Gray;

            MonoAware.System.Console.Clear();

            if (!monoMode)
            {
                Console.Title = "IHI | V" + Assembly.GetExecutingAssembly().GetName().Version.ToString(4);
                Console.WriteLine("\n    IHI | V" + Assembly.GetExecutingAssembly().GetName().Version.ToString(4) + "\n");
            }
            else
            {
                Console.Title = "IHI [Mono] | V" + Assembly.GetExecutingAssembly().GetName().Version.ToString(4);
                Console.WriteLine("\n    IHI [Mono] | V" + Assembly.GetExecutingAssembly().GetName().Version.ToString(4) +
                                  "\n");
            }

            Console.Beep();

            CoreManager.InitializeServerCore();
            CoreManager.InitializeInstallerCore();

            var bootResult =
                CoreManager.GetServerCore().Boot(Path.Combine(Environment.CurrentDirectory, configFile));
            CoreManager.DereferenceInstallerCore();
            GC.Collect();

            if (bootResult != BootResult.AllClear)
            {
                Console.WriteLine("\n\n\n    IHI has failed to boot!");

                if (disableAutoExit)
                {
                    Console.WriteLine("\n\n    Auto Exit Disabled - Press any key to exit!");
                    Console.ReadKey(true);
                }
                return;
            }

            // Reassign CTRL + C to safely shutdown.
            // CTRL + Break is still unsafe.
            Console.TreatControlCAsInput = false;
            Console.CancelKeyPress += ShutdownKey;

            while (true)
            {
                // Block all future input
                Console.ReadKey(true);
            }
        }


        private static void ShutdownKey(object sender, ConsoleCancelEventArgs e)

        {
            if(e.SpecialKey == ConsoleSpecialKey.ControlBreak)
                return;
            Console.CancelKeyPress -= ShutdownKey;
            e.Cancel = true;
            CoreManager.GetServerCore().Shutdown();
        }
    }

    internal enum BootResult
    {
        AllClear,
        MySQLConnectionFailure,
        MySQLMappingFailure,
        SocketBindingFailure,
        UnknownFailure
    }
}