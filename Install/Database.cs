using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IHI.Server.Install
{
    internal static class Database
    {
        const byte STEPS = 7;

        /// <summary>
        /// Run the database configuration installer.
        /// </summary>
        /// <param name="Values">The Dictionary to save the inputted configuration values to.</param>
        internal static void Run(ref Dictionary<string, object> Values)
        {
            Output.SetTitle("Database Configuration");

            Values.Add("database.host", GetHost());
            Values.Add("database.port", GetPort());
            Values.Add("database.username", GetUsername());
            Values.Add("database.password", GetPassword());
            Values.Add("database.database", GetDatabaseName());

            int MinimumPoolSize = GetMinimumPoolSize();

            Values.Add("database.minpool", MinimumPoolSize);
            Values.Add("database.maxpool", GetMaximumPoolSize(MinimumPoolSize));
        }

        private static string GetHost()
        {
            Output.SetStep(1, STEPS);

            Output.ClearPage();

            Console.WriteLine(">>>    MySQL Host    <<<");
            Console.WriteLine();
            Console.WriteLine("This is the Hostname or IP Address used to connect to the MySQL server.");
            Console.WriteLine();
            Console.WriteLine("Examples: localhost");
            Console.WriteLine("          127.0.0.1");
            Console.WriteLine("          db.somedomain.com");
            Console.WriteLine();
            Console.WriteLine("Default Value: localhost");
            Console.WriteLine();
            Console.Write("=> ");

            return Input.GetString("localhost");
        }

        private static ushort GetPort()
        {
            Output.SetStep(2, STEPS);

            Output.ClearPage();

            Console.WriteLine(">>>    MySQL Port    <<<");
            Console.WriteLine();
            Console.WriteLine("This is the Port used to connect to the MySQL server.");
            Console.WriteLine();
            Console.WriteLine("Examples: 3306");
            Console.WriteLine("          12345");
            Console.WriteLine();
            Console.WriteLine("Default Value: 3306");
            Console.WriteLine();
            Console.Write("=> ");

        Retry:
            try
            {
                return Input.GetUshort(3306);
            }
            catch (InputException e)
            {
                e.Display();

                Console.SetCursorPosition(3, 15);
                Console.Write(string.Empty.PadRight(MonoAware.System.Console.BufferWidth - 3));
                Console.SetCursorPosition(3, 15);

                goto Retry;
            }
        }

        private static string GetUsername()
        {
            Output.SetStep(3, STEPS);

            Output.ClearPage();

            Console.WriteLine(">>>    MySQL Username    <<<");
            Console.WriteLine();
            Console.WriteLine("This is the Username used to connect to the MySQL server.");
            Console.WriteLine();
            Console.WriteLine("Examples: ihi");
            Console.WriteLine("          root");
            Console.WriteLine("          chris");
            Console.WriteLine();
            Console.WriteLine("Default Value: ihi");
            Console.WriteLine();
            Console.Write("=> ");

            return Input.GetString("ihi");
        }

        private static string GetPassword()
        {
            Output.SetStep(4, STEPS);

            Output.ClearPage();

            Console.WriteLine(">>>    MySQL Password    <<<");
            Console.WriteLine();
            Console.WriteLine("This is the Password used to connect to the MySQL server.");
            Console.WriteLine();
            Console.WriteLine("Blank Value: NO PASSWORD");
            Console.WriteLine();
            Console.Write("=> ");

            return Input.GetPassword();
        }

        private static string GetDatabaseName()
        {
            Output.SetStep(5, STEPS);

            Output.ClearPage();

            Console.WriteLine(">>>    MySQL Database Name    <<<");
            Console.WriteLine();
            Console.WriteLine("This is the name of the database that IHI should use.");
            Console.WriteLine();
            Console.WriteLine("Examples: ihi");
            Console.WriteLine("          ihi_database");
            Console.WriteLine();
            Console.WriteLine("Default Value: ihi");
            Console.WriteLine();
            Console.Write("=> ");

            return Input.GetString("ihi");
        }

        private static int GetMinimumPoolSize()
        {
            Output.SetStep(6, STEPS);

            Output.ClearPage();

            Console.WriteLine(">>>    MySQL Minimum Pool Side    <<<");
            Console.WriteLine();
            Console.WriteLine("This is the minimum amount of connections to maintain with the MySQL server for pooling.");
            Console.WriteLine();
            Console.WriteLine("Examples: 1");
            Console.WriteLine("          5");
            Console.WriteLine();
            Console.WriteLine("Default Value: 1");
            Console.WriteLine();
            Console.Write("=> ");




        Retry:
            try
            {
                return Input.GetInt(1);
            }
            catch (InputException e)
            {
                e.Display();

                Console.SetCursorPosition(3, 15);
                Console.Write(string.Empty.PadRight(MonoAware.System.Console.BufferWidth - 3));
                Console.SetCursorPosition(3, 15);

                goto Retry;
            }
        }

        private static int GetMaximumPoolSize(int Minimum)
        {
            Output.SetStep(7, STEPS);

            Output.ClearPage();

            Console.WriteLine(">>>    MySQL Maximum Pool Side    <<<");
            Console.WriteLine();
            Console.WriteLine("This is the maximum amount of connections to maintain with the MySQL server for pooling.");
            Console.WriteLine();
            Console.WriteLine("Examples: 25");
            Console.WriteLine("          50");
            Console.WriteLine();
            Console.WriteLine("Default Value: 25");
            Console.WriteLine();
            Console.Write("=> ");


        Retry:
            try
            {
                return Input.GetInt(Math.Max(Minimum, 25), Minimum);
            }
            catch (InputException e)
            {
                e.Display();

                Console.SetCursorPosition(3, 15);
                Console.Write(string.Empty.PadRight(MonoAware.System.Console.BufferWidth - 3));
                Console.SetCursorPosition(3, 15);

                goto Retry;
            }
        }
    }
}
