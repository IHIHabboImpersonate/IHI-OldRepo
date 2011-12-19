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
            UnixAware.UnixMode = false;
            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.Unix:
                case PlatformID.MacOSX:
                    {
                        UnixAware.UnixMode = true;
                        break;
                    }
            }


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

            if (!UnixAware.UnixMode)
            {
                if (Type.GetType("Mono.Runtime") == null)
                {
                    Console.Title = "IHI [Windows] [.NET] | V" + Assembly.GetExecutingAssembly().GetName().Version.ToString(4);
                    Console.WriteLine("\n    IHI [Windows] [.NET] | V" + Assembly.GetExecutingAssembly().GetName().Version.ToString(4) + "\n");
                }
                else
                {
                    Console.Title = "IHI [Windows] [Mono] | V" + Assembly.GetExecutingAssembly().GetName().Version.ToString(4);
                    Console.WriteLine("\n    IHI [Windows] [Mono] | V" + Assembly.GetExecutingAssembly().GetName().Version.ToString(4) + "\n");
                }
            }
            else
            {
                Console.Title = "IHI [Unix] [Mono] | V" + Assembly.GetExecutingAssembly().GetName().Version.ToString(4);
                Console.WriteLine("\n    IHI [Unix] [Mono] | V" + Assembly.GetExecutingAssembly().GetName().Version.ToString(4) + "\n");
            }

            Console.Beep();

            CoreManager.InitialiseServerCore();
            CoreManager.InitialiseInstallerCore();

            BootResult bootResult = CoreManager.ServerCore.Boot(Path.Combine(Environment.CurrentDirectory, configFile));
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
            if (e.SpecialKey == ConsoleSpecialKey.ControlBreak)
                return;
            Console.CancelKeyPress -= ShutdownKey;
            e.Cancel = true;
            CoreManager.ServerCore.Shutdown();
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