using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using IHI.Server.Configuration;
using IHI.Server.Habbos;
using IHI.Server.Habbos.Figure;
using IHI.Server.Install;
using IHI.Server.Networking;
using IHI.Server.Plugins;
using IHI.Server.Users.Permissions;
using IHI.Server.WebAdmin;
using MySql.Data.MySqlClient;
using NHibernate;

namespace IHI.Server
{
    public class Core : IShutdown
    {
        #region Fields

        private XmlConfig _config;
        private IonTcpConnectionManager _connectionManager;
        private HabboDistributor _habboDistributor;
        private HabboFigureFactory _habboFigureFactory;
        private ISessionFactory _nHibernateSessionFactory;
        private PermissionManager _permissionManager;
        private PluginManager _pluginManager;
        private StandardOut _standardOut;
        private Encoding _textEncoding;
        private WebAdminManager _webAdminManager;

        #endregion

        #region API

        public HabboDistributor GetHabboDistributor()
        {
            return _habboDistributor;
        }

        public StandardOut GetStandardOut()
        {
            return _standardOut;
        }

        public ISession GetDatabaseSession()
        {
            return _nHibernateSessionFactory.OpenSession();
        }

        public XmlConfig GetConfig()
        {
            return _config;
        }

        public IonTcpConnectionManager GetConnectionManager()
        {
            return _connectionManager;
        }

        public Encoding GetTextEncoding()
        {
            return _textEncoding;
        }

        public PermissionManager GetPermissionManager()
        {
            return _permissionManager;
        }

        public WebAdminManager GetWebAdminManager()
        {
            return _webAdminManager;
        }

        public PluginManager GetPluginManager()
        {
            return _pluginManager;
        }

        public HabboFigureFactory GetHabboFigureFactory()
        {
            return _habboFigureFactory;
        }

        #endregion

        #region InternalMethods

        public void Shutdown()
        {
            // TODO: Safe Shutdown
        }

        internal BootResult Boot(string configPath)
        {
            try
            {
                _textEncoding = Encoding.UTF8; // TODO: Move this to an external config.

                #region Standard Out

                _standardOut = new StandardOut();
                _standardOut.PrintNotice("Text Encoding => Set to " + _textEncoding.EncodingName);
                _standardOut.PrintNotice("Standard Out => Ready");

                #endregion

                _config = new XmlConfig(configPath);

                var mainInstallRequired = PreInstall(); // Register the main installation if required.

                #region Load Plugins

                _standardOut.PrintNotice("Plugin Loader => Starting...");
                _pluginManager = new PluginManager();
                foreach (var path in PluginManager.GetAllPotentialPluginPaths())
                {
                    GetPluginManager().LoadPluginAtPath(path);
                }
                _standardOut.PrintNotice("Plugin Loader => Started!");

                #endregion

                CoreManager.GetInstallerCore().Run();

                if (mainInstallRequired)
                    SaveConfigInstallation();

                #region Config

                _standardOut.PrintNotice("Config File => Loaded");
                _standardOut.SetImportance(
                    (StandardOutImportance)
                    _config.ValueAsByte("/config/standardout/importance", (byte) StandardOutImportance.Debug));


                _standardOut.PrintNotice("MySQL => Preparing database connection settings...");

                try
                {
                    var connectionString = new MySqlConnectionStringBuilder
                                               {
                                                   Server = _config.ValueAsString("/config/mysql/host"),
                                                   Port = _config.ValueAsUint("/config/mysql/port", 3306),
                                                   UserID = _config.ValueAsString("/config/mysql/user"),
                                                   Password = _config.ValueAsString("/config/mysql/password"),
                                                   Database = _config.ValueAsString("/config/mysql/database"),
                                                   Pooling = true,
                                                   MinimumPoolSize = _config.ValueAsUint("/config/mysql/minpoolsize", 1),
                                                   MaximumPoolSize =
                                                       _config.ValueAsUint("/config/mysql/maxpoolsize", 25)
                                               };

                    PrepareSessionFactory(connectionString.ConnectionString);

                    _standardOut.PrintNotice("MySQL => Testing connection...");

                    using (var db = GetDatabaseSession())
                    {
                        if (!db.IsConnected)
                            throw new Exception("Unknown cause");
                    }
                }
                catch
                {
                    _standardOut.PrintError("MySQL => Connection failed!");
                    throw;
                }
                _standardOut.PrintNotice("MySQL => Connected!");

                #endregion

                #region Distributors

                _standardOut.PrintNotice("Habbo Distributor => Constructing...");
                _habboDistributor = new HabboDistributor();
                _standardOut.PrintNotice("Habbo Distributor => Ready");

                #endregion

                #region Figure Factory

                _standardOut.PrintNotice("Habbo Figure Factory => Constructing...");
                _habboFigureFactory = new HabboFigureFactory();
                _standardOut.PrintNotice("Habbo Figure Factory => Ready");

                #endregion

                // TODO: Download Requirements

                #region Permissions

                _standardOut.PrintNotice("Permission Manager => Constructing...");
                _permissionManager = new PermissionManager();
                _standardOut.PrintNotice("Permission Manager => Ready");

                #endregion

                // TODO: Write Dynamic Client Files
                // TODO: Authenticate with IHINet

                #region Network

                _standardOut.PrintNotice("Connection Manager => Starting...");
                _connectionManager = new IonTcpConnectionManager(_config.ValueAsString("/config/network/host"),
                                                                 _config.ValueAsInt("/config/network/port", 14478),
                                                                 _config.ValueAsInt("/config/network/maxconnections", 2));
                _connectionManager.GetListener().Start();
                _standardOut.PrintNotice("Connection Manager => Ready!");

                _standardOut.PrintNotice("Web Admin => Starting...");
                _webAdminManager = new WebAdminManager(_config.ValueAsUshort("/config/webadmin/port", 14480));
                _standardOut.PrintNotice("Web Admin => Ready!");

                #endregion

                #region Start Plugins

                var pluginManager = GetPluginManager();
                _standardOut.PrintNotice("Plugin Starter => Working...");
                foreach (var plugin in pluginManager.GetLoadedPlugins())
                {
                    pluginManager.StartPlugin(plugin);
                }
                _standardOut.PrintNotice("Plugin Starter => Finished!");

                #endregion

                _standardOut.PrintImportant("IHI is now functional!");

                return BootResult.AllClear;
            }
            catch (Exception e)
            {
                _standardOut.PrintException(e);


                var t = e.GetType();

                if (t == typeof (MappingException))
                    return BootResult.MySQLMappingFailure;

                return BootResult.UnknownFailure;
            }
        }

        private void PrepareSessionFactory(string connectionString)
        {
            IDictionary<string, string> properties = new Dictionary<string, string>
                                                         {
                                                             {
                                                                 "connection.driver_class",
                                                                 "NHibernate.Driver.MySqlDataDriver"
                                                                 },
                                                             {"connection.connection_string", connectionString},
                                                             {
                                                                 "proxyfactory.factory_class",
                                                                 "NHibernate.ByteCode.Castle.ProxyFactoryFactory,NHibernate.ByteCode.Castle"
                                                                 },
                                                             {"dialect", "NHibernate.Dialect.MySQL5Dialect"}
                                                         };

            var configuration = new NHibernate.Cfg.Configuration();
            configuration.SetProperties(properties);

            foreach (
                var file in
                    new DirectoryInfo(Path.Combine(Environment.CurrentDirectory, "database")).GetFiles("*.dll"))
            {
                configuration.AddAssembly(Assembly.LoadFile(file.FullName));
            }

            _nHibernateSessionFactory = configuration.BuildSessionFactory();
        }

        private bool PreInstall()
        {
            if (_config.WasCreated()) // Did the config file have to be created?
            {
                // Yes, add to the Installer Core.
                var installerCore = CoreManager.GetInstallerCore();

                installerCore.
                    AddCategory(
                        "StandardOut",
                        new Category().
                            AddStep("Importance",
                                    new StringStep(
                                        "Default Importance",
                                        "This is the minimum importance level that messages must have to be printed to standard out.",
                                        new[]
                                            {
                                                "DEBUG",
                                                "NOTICE",
                                                "IMPORTANT",
                                                "WARNING",
                                                "ERROR"
                                            },
                                        "NOTICE"))).
                    AddCategory(
                        "Database",
                        new Category().
                            AddStep("Host",
                                    new StringStep(
                                        "MySQL Host",
                                        "This is the Hostname or IP Address used to connect to the MySQL server.",
                                        new[]
                                            {
                                                "localhost",
                                                "127.0.0.1",
                                                "db.somedomain.com"
                                            },
                                        "localhost")).
                            AddStep("Port",
                                    new UShortStep(
                                        "MySQL Port",
                                        "This is the Port used to connect to the MySQL server.",
                                        new[]
                                            {
                                                "3306",
                                                "12345"
                                            },
                                        3306)).
                            AddStep("Username",
                                    new StringStep(
                                        "MySQL Username",
                                        "This is the Username used to authenticate with the MySQL server.",
                                        new[]
                                            {
                                                "ihi",
                                                "root",
                                                "chris"
                                            },
                                        "ihi")).
                            AddStep("Password",
                                    new PasswordStep(
                                        "MySQL Password",
                                        "This is the Password used to authenticate with the MySQL server.")).
                            AddStep("MinimumPoolSide",
                                    new IntStep(
                                        "MySQL Minimum Pool Side",
                                        "This is the minimum amount of MySQL connections to maintain in the pool.",
                                        new[]
                                            {
                                                "1",
                                                "5"
                                            },
                                        1,
                                        1)).
                            AddStep("MaximumPoolSide",
                                    new IntStep(
                                        "MySQL Maximum Pool Side",
                                        "This is the maximum amount of MySQL connections to maintain in the pool.",
                                        new[]
                                            {
                                                "1",
                                                "5"
                                            },
                                        1,
                                        1))).
                    AddCategory("Network",
                                new Category().
                                    AddStep(
                                        "GameHost",
                                        new StringStep(
                                            "Game Host",
                                            "This is the host (normally an IP) to bind the listener for normal game connections.",
                                            new[]
                                                {
                                                    "127.0.0.1",
                                                    "192.168.1.12",
                                                    "5.24.246.133"
                                                },
                                            "127.0.0.1")).
                                    AddStep(
                                        "GamePort",
                                        new UShortStep(
                                            "Game Port",
                                            "This is the port to bind the listener for normal game connections.",
                                            new[]
                                                {
                                                    "14478",
                                                    "30000"
                                                },
                                            14478)).
                                    AddStep(
                                        "WebAdminPort",
                                        new UShortStep(
                                            "WebAdmin Port",
                                            "This is the port to bind the WebAdmin listener.",
                                            new[]
                                                {
                                                    "14480",
                                                    "30002"
                                                },
                                            14478)));
                return true;
            }
            return false;
        }

        private void SaveConfigInstallation()
        {
            var installer = CoreManager.GetInstallerCore();


            _standardOut.PrintImportant("Updating configuration file... (Install)");

            var doc = _config.GetInternalDocument();
            var rootElement = doc.SelectSingleNode("/");

            var standardOutElement = doc.CreateElement("standardout");
            var mySQLElement = doc.CreateElement("mysql");
            var networkElement = doc.CreateElement("network");
            var webAdminElement = doc.CreateElement("webadmin");

            #region StandardOut

            #region Importance

            var valueElement = doc.CreateElement("importance");
            valueElement.InnerText = installer.GetInstallerOutputValue("StandardOut", "Importance").ToString();
            standardOutElement.AppendChild(valueElement);

            #endregion

            #endregion

            #region MySQL

            #region Host

            valueElement = doc.CreateElement("host");
            valueElement.InnerText = installer.GetInstallerOutputValue("Database", "Host").ToString();
            mySQLElement.AppendChild(valueElement);

            #endregion

            #region Port

            valueElement = doc.CreateElement("port");
            valueElement.InnerText = installer.GetInstallerOutputValue("Database", "Port").ToString();
            mySQLElement.AppendChild(valueElement);

            #endregion

            #region User

            valueElement = doc.CreateElement("user");
            valueElement.InnerText = installer.GetInstallerOutputValue("Database", "Username").ToString();
            mySQLElement.AppendChild(valueElement);

            #endregion

            #region Password

            valueElement = doc.CreateElement("password");
            valueElement.InnerText = installer.GetInstallerOutputValue("Database", "Password").ToString();
            mySQLElement.AppendChild(valueElement);

            #endregion

            #region Database

            valueElement = doc.CreateElement("database");
            valueElement.InnerText = installer.GetInstallerOutputValue("Database", "DatabaseName").ToString();
            mySQLElement.AppendChild(valueElement);

            #endregion

            #region MinPoolSize

            valueElement = doc.CreateElement("minpoolsize");
            valueElement.InnerText = installer.GetInstallerOutputValue("Database", "MinPoolSize").ToString();
            mySQLElement.AppendChild(valueElement);

            #endregion

            #region MaxPoolSize

            valueElement = doc.CreateElement("maxpoolsize");
            valueElement.InnerText = installer.GetInstallerOutputValue("Database", "MaxPoolSize").ToString();
            mySQLElement.AppendChild(valueElement);

            #endregion

            #endregion

            #region Network

            #region Host

            valueElement = doc.CreateElement("host");
            valueElement.InnerText = installer.GetInstallerOutputValue("Network", "GameHost").ToString();
            networkElement.AppendChild(valueElement);

            #endregion

            #region Port

            valueElement = doc.CreateElement("port");
            valueElement.InnerText = installer.GetInstallerOutputValue("Network", "GamePort").ToString();
            networkElement.AppendChild(valueElement);

            #endregion

            #region MaxConnections

            valueElement = doc.CreateElement("maxconnections");
            valueElement.InnerText = installer.GetInstallerOutputValue("Network", "MaxGameConnections").ToString();
            networkElement.AppendChild(valueElement);

            #endregion

            #endregion

            #region WebAdmin

            #region Port

            valueElement = doc.CreateElement("port");
            valueElement.InnerText = installer.GetInstallerOutputValue("Network", "WebAdminPort").ToString();
            webAdminElement.AppendChild(valueElement);

            #endregion

            #endregion

            rootElement.AppendChild(standardOutElement);
            rootElement.AppendChild(mySQLElement);
            rootElement.AppendChild(networkElement);
            rootElement.AppendChild(webAdminElement);

            _config.Save();
            _standardOut.PrintImportant("Configuration file saved!");
        }

        #endregion
    }
}