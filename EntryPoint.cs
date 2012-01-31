using System;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;

namespace IHI.Server
{
    public static class EntryPoint
    {

        [STAThreadAttribute]
        public static void Main(string[] arguments)
        {
            AppDomain.CurrentDomain.UnhandledException += UnhandledException;
            
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
                    Console.Title = "IHI [Windows] [.NET] | V" +
                                    Assembly.GetExecutingAssembly().GetName().Version.ToString(4);
                    Console.WriteLine("\n    IHI [Windows] [.NET] | V" +
                                      Assembly.GetExecutingAssembly().GetName().Version.ToString(4) + "\n");
                }
                else
                {
                    Console.Title = "IHI [Windows] [Mono] | V" +
                                    Assembly.GetExecutingAssembly().GetName().Version.ToString(4);
                    Console.WriteLine("\n    IHI [Windows] [Mono] | V" +
                                      Assembly.GetExecutingAssembly().GetName().Version.ToString(4) + "\n");
                }
            }
            else
            {
                Console.Title = "IHI [Unix] [Mono] | V" + Assembly.GetExecutingAssembly().GetName().Version.ToString(4);
                Console.WriteLine("\n    IHI [Unix] [Mono] | V" +
                                  Assembly.GetExecutingAssembly().GetName().Version.ToString(4) + "\n");
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

        private static void UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (!e.IsTerminating) 
                return;

            Console.WindowWidth = Console.BufferWidth = Console.WindowWidth * 2;

            Exception exception = e.ExceptionObject as Exception;

            UnixAware.System.Console.BackgroundColor = ConsoleColor.Blue;
            UnixAware.System.Console.ForegroundColor = ConsoleColor.White;
            UnixAware.System.Console.Clear();

            Console.WriteLine("[===[ IHI STOP ERROR ]===]");
            Console.WriteLine("An unhandled exception has caused IHI to close!");
            Console.WriteLine("Details of the exception are below.");
            Console.WriteLine();
            Console.WriteLine("Time (UTC): " + DateTime.UtcNow);

            Console.WriteLine("Loaded Assemblies: ");
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if(Assembly.GetCallingAssembly() == assembly)
                    Console.WriteLine("!!" + assembly.FullName);
                else
                    Console.WriteLine("  " + assembly.FullName);
            }

            Console.WriteLine();
            Console.WriteLine("Exception Assembly: " + Assembly.GetCallingAssembly().FullName);
            Console.WriteLine("Exception Thread: " + Thread.CurrentThread.Name);
            Console.WriteLine("Exception Type: " + exception.GetType().FullName);
            Console.WriteLine("Exception Message: " + exception.Message);

            Console.Write("Has Inner Exception: ");
            Console.WriteLine(exception.InnerException == null ? "NO" : "YES");

            Console.WriteLine("Stack Trace:");
            Console.WriteLine("  " + exception.StackTrace.Replace(Environment.NewLine, Environment.NewLine + "  "));

            string  logText = "IHISTOPERROR\x01";
            logText += "TIME\x02" + DateTime.UtcNow + "\x01";
            logText += "ASSEMBLIES\x02";

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                logText += "  " + assembly.FullName + "\x02";
            }
            logText += "\x01";
            logText += "EXCEPTION-ASSEMBLY\x02" + Assembly.GetCallingAssembly().FullName + "\x01";
            logText += "EXCEPTION-THREAD\x02" + Thread.CurrentThread.Name + "\x01";

            int i = 0;
            while(exception != null)
            {
                logText += "EXCEPTION[" + i + "]-TYPE\x02" + exception.GetType().FullName + "\x01";
                logText += "EXCEPTION[" + i + "]-MESSAGE\x02" + exception.Message + "\x01";
                logText += "EXCEPTION[" + i + "]-STACKTRACE\x02" + exception.StackTrace + "\x01";

                i++;
                exception = exception.InnerException;
            }

            string path = Path.Combine(Environment.CurrentDirectory, "dumps", "stoperror-" + DateTime.UtcNow.Ticks + ".ihidump");

            File.WriteAllText(path, logText);
            Console.WriteLine();
            Console.WriteLine("Crash log saved to file:");
            Console.WriteLine("  " + path);
            Console.WriteLine("Press any key to exit");
            Console.ReadKey(true);
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