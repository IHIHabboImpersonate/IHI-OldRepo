using System;
using System.Text.RegularExpressions;
using System.Threading;

namespace IHI.Server
{
    public static class EntryPoint
    {
        [STAThreadAttribute]
        public static void Main(string[] Arguments)
        {
            bool MonoMode = false;
            string ConfigFile = "config.xml";
            bool DisableAutoExit = false;

            Regex NVRegex = new Regex("^--(?<name>[\\w-]+)=(?<value>.+)$");

            foreach (string Argument in Arguments)
            {
                Match NVMatch = NVRegex.Match(Argument);
                string Name = NVMatch.Groups["name"].Value;
                string Value = NVMatch.Groups["value"].Value;

                switch (Name)
                {
                    case "mode":
                        {
                            if (Value.ToLower() == "mono")
                                MonoMode = true;
                            break;
                        }
                    case "config-file":
                        {
                            ConfigFile = Value;
                            break;
                        }
                    case "auto-exit":
                        {
                            switch (Value.ToLower())
                            {
                                case "false":
                                case "off":
                                case "no":
                                case "disable":
                                case "disabled":
                                    DisableAutoExit = true;
                                    break;
                            }
                            break;
                        }
                }
            }

            MonoAware.Init(MonoMode);

            MonoAware.System.Console.BackgroundColor = ConsoleColor.Black;
            MonoAware.System.Console.ForegroundColor = ConsoleColor.Gray;
            
            MonoAware.System.Console.Clear();

            if (!MonoMode)
            {
                Console.Title = "IHI | V" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString(4);
                Console.WriteLine("\n    IHI | V" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString(4) + "\n");
            }
            else
            {
                Console.Title = "IHI [Mono] | V" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString(4);
                Console.WriteLine("\n    IHI [Mono] | V" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString(4) + "\n");
            }

            Console.Beep();

            bool BootResult = new Core().Boot(System.IO.Path.Combine(Environment.CurrentDirectory, ConfigFile));

            if (!BootResult)
            {
                Console.WriteLine("\n\n\n    IHI has failed to boot!");
                Thread.Sleep(1000);
                Console.Beep(1000, 250);
                Thread.Sleep(125);
                Console.Beep(1000, 250);
                Thread.Sleep(125);
                Console.Beep(1000, 250);

                if (DisableAutoExit)
                {
                    Console.WriteLine("\n\n    Auto Exit Disabled - Press any key to exit!");
                    Console.ReadKey(true);
                }
                return;
            }

            Console.TreatControlCAsInput = false;
            Console.CancelKeyPress += new ConsoleCancelEventHandler(ShutdownKey);
            
            while (true)
            {
                Console.ReadKey(true);
            }
        }

        private static void ShutdownKey(object sender, ConsoleCancelEventArgs e)
        {
            CoreManager.GetCore().Shutdown();
        }
    }
}
