#region GPLv3

// 
// Copyright (C) 2012  Chris Chenery
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
// 

#endregion

#region Usings

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
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

#endregion

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

        public void Shutdown()
        {
            // TODO: Safe Shutdown
        }

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

                bool mainInstallRequired = PreInstall(); // Register the main installation if required.

                #region Load Plugins

                _standardOut.PrintNotice("Plugin Manager => Loading plugins...");
                _pluginManager = new PluginManager();
                foreach (string path in PluginManager.GetAllPotentialPluginPaths())
                {
                    GetPluginManager().LoadPluginAtPath(path);
                }
                _standardOut.PrintNotice("Plugin Manager => Plugins loaded!");

                #endregion

                CoreManager.InstallerCore.Run();

                if (mainInstallRequired)
                    SaveConfigInstallation();

                #region Config

                _standardOut.PrintNotice("Config File => Loaded");
                _standardOut.SetImportance(
                    (StandardOutImportance)
                    _config.ValueAsByte("/config/standardout/importance", (byte) StandardOutImportance.Debug));

                #endregion

                #region Database

                _standardOut.PrintNotice("MySQL => Preparing database connection settings...");

                try
                {
                    MySqlConnectionStringBuilder connectionString = new MySqlConnectionStringBuilder
                                                                        {
                                                                            Server =
                                                                                _config.ValueAsString(
                                                                                    "/config/mysql/host"),
                                                                            Port =
                                                                                _config.ValueAsUint(
                                                                                    "/config/mysql/port", 3306),
                                                                            UserID =
                                                                                _config.ValueAsString(
                                                                                    "/config/mysql/user"),
                                                                            Password =
                                                                                _config.ValueAsString(
                                                                                    "/config/mysql/password"),
                                                                            Database =
                                                                                _config.ValueAsString(
                                                                                    "/config/mysql/database"),
                                                                            Pooling = true,
                                                                            MinimumPoolSize =
                                                                                _config.ValueAsUint(
                                                                                    "/config/mysql/minpoolsize", 1),
                                                                            MaximumPoolSize =
                                                                                _config.ValueAsUint(
                                                                                    "/config/mysql/maxpoolsize", 25)
                                                                        };

                    PrepareSessionFactory(connectionString.ConnectionString);

                    _standardOut.PrintNotice("MySQL => Testing connection...");

                    using (ISession db = GetDatabaseSession())
                    {
                        if (!db.IsConnected)
                            throw new Exception("Unknown cause");
                    }
                }
                catch (Exception ex)
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
                                                                 _config.ValueAsInt("/config/network/port", 14478));
                _connectionManager.GetListener().Start();
                _standardOut.PrintNotice("Connection Manager => Ready!");

                _standardOut.PrintNotice("Web Admin => Starting...");
                _webAdminManager = new WebAdminManager(_config.ValueAsUshort("/config/webadmin/port", 14480));
                _standardOut.PrintNotice("Web Admin => Ready!");

                #endregion

                #region Start Plugins

                PluginManager pluginManager = GetPluginManager();
                _standardOut.PrintNotice("Plugin Manager => Starting plugins...");
                foreach (Plugin plugin in pluginManager.GetLoadedPlugins())
                {
                    pluginManager.StartPlugin(plugin);
                }
                _standardOut.PrintNotice("Plugin Manager => Plugins started!");

                #endregion

                _standardOut.PrintImportant("IHI is now functional!");

                return BootResult.AllClear;
            }
            catch (Exception e)
            {
                _standardOut.PrintException(e);


                if (e is MappingException)
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

            NHibernate.Cfg.Configuration configuration = new NHibernate.Cfg.Configuration();
            configuration.SetProperties(properties);

            foreach (
                FileInfo file in
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
                CoreManager.InstallerCore.
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
                                                "root (NOT RECOMMENDED)",
                                                "chris"
                                            },
                                        "ihi")).
                            AddStep("Password",
                                    new PasswordStep(
                                        "MySQL Password",
                                        "This is the Password used to authenticate with the MySQL server.")).
                            AddStep("DatebaseName",
                                    new StringStep(
                                        "MySQL Database Name",
                                        "This is the name of the database IHI should use.",
                                        new[]
                                            {
                                                "ihi",
                                                "ihidb",
                                                "hotel"
                                            },
                                        "ihi")).
                            AddStep("MinimumPoolSize",
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
                            AddStep("MaximumPoolSize",
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
                                            14480)));
                return true;
            }
            return false;
        }

        private void SaveConfigInstallation()
        {
            Install.Core installer = CoreManager.InstallerCore;


            _standardOut.PrintImportant("Updating configuration file... (Install)");

            XmlDocument doc = _config.GetInternalDocument();
            XmlNode rootElement = doc.SelectSingleNode("/config");

            XmlElement standardOutElement = doc.CreateElement("standardout");
            XmlElement mySQLElement = doc.CreateElement("mysql");
            XmlElement networkElement = doc.CreateElement("network");
            XmlElement webAdminElement = doc.CreateElement("webadmin");

            #region StandardOut

            #region Importance

            XmlElement valueElement = doc.CreateElement("importance");
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
            valueElement.InnerText = installer.GetInstallerOutputValue("Database", "DatebaseName").ToString();
            mySQLElement.AppendChild(valueElement);

            #endregion

            #region MinPoolSize

            valueElement = doc.CreateElement("minpoolsize");
            valueElement.InnerText = installer.GetInstallerOutputValue("Database", "MinimumPoolSize").ToString();
            mySQLElement.AppendChild(valueElement);

            #endregion

            #region MaxPoolSize

            valueElement = doc.CreateElement("maxpoolsize");
            valueElement.InnerText = installer.GetInstallerOutputValue("Database", "MaximumPoolSize").ToString();
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