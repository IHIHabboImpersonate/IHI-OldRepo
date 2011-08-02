using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using IHI.Server.Habbos;
using IHI.Server.Configuration;
using IHI.Server.Plugins;
using IHI.Server.Users.Permissions;
using IHI.Server.Networking;
using IHI.Server.WebAdmin;

using System.IO;
using System.Xml;

using NHibernate;
using NHCfg = NHibernate.Cfg;

using MySql.Data.MySqlClient;

namespace IHI.Server
{
    public static class CoreManager
    {
        internal static Core fCore;
        /// <summary>
        /// Returns the instance of Core.
        /// </summary>
        /// <returns></returns>
        public static Core GetCore()
        {
            return fCore;
        }
    }
    public class Core : IShutdown
    {
        #region Fields
        private HabboDistributor fHabboDistributor;
        private StandardOut fStandardOut;
        private ISessionFactory fNHibernateSessionFactory;
        private XmlConfig fConfig;
        private IonTcpConnectionManager fConnectionManager;
        private Encoding fTextEncoding;
        private PermissionManager fPermissionManager;
        private PluginManager fPluginManager;
        private WebAdminManager fWebAdminManager;
        private HabboFigureFactory fHabboFigureFactory;
        #endregion

        public Core()
        {
            CoreManager.fCore = this;
        }

        #region API
        public HabboDistributor GetUserDistributor()
        {
            return this.fHabboDistributor;
        }
        public StandardOut GetStandardOut()
        {
            return this.fStandardOut;
        }
        public ISession GetDatabaseSession()
        {
            return this.fNHibernateSessionFactory.OpenSession();
        }
        public XmlConfig GetConfig()
        {
            return this.fConfig;
        }
        public IonTcpConnectionManager GetConnectionManager()
        {
            return this.fConnectionManager;
        }
        public Encoding GetTextEncoding()
        {
            return this.fTextEncoding;
        }
        public PermissionManager GetPermissionManager()
        {
            return this.fPermissionManager;
        }
        public WebAdminManager GetWebAdminManager()
        {
            return this.fWebAdminManager;
        }
        public PluginManager GetPluginManager()
        {
            return this.fPluginManager;
        }
        public HabboFigureFactory GetHabboFigureFactory()
        {
            return this.fHabboFigureFactory;
        }
        #endregion

        #region InternalMethods
        internal bool Boot(string ConfigPath)
        {
            this.fTextEncoding = Encoding.UTF8;  // TODO: Move this to an external config.

            this.fStandardOut = new StandardOut();

            try
            {
                this.fStandardOut.PrintNotice("Text Encoding => Set to " + fTextEncoding.EncodingName);
                this.fStandardOut.PrintNotice("Standard Out => Ready");

                this.fConfig = new XmlConfig(ConfigPath);

                if (fConfig.WasCreated()) // Did the config file have to be created?
                {
                    // Yes, run the install process.

                    this.fStandardOut.PrintImportant("No config file found! File created!");
                    this.fStandardOut.PrintImportant("Starting installer...");

                    this.fStandardOut.PrintNotice("Standard Out => Disabled (Install)");


                    Dictionary<string, object> InstallerReturn = Install.Core.Run();
                    MonoAware.System.Console.Clear();

                    this.fStandardOut.PrintNotice("Standard Out => Enabled (Install)");
                    this.fStandardOut.PrintImportant("Updating configuration file...");

                    XmlDocument Doc = fConfig.GetInternalDocument();
                    XmlNode RootElement = Doc.GetElementsByTagName("config")[0] as XmlNode;

                    XmlElement StandardOutElement = Doc.CreateElement("standardout");
                    XmlElement MySQLElement = Doc.CreateElement("mysql");
                    XmlElement NetworkElement = Doc.CreateElement("network");
                    XmlElement WebAdminElement = Doc.CreateElement("webadmin");

                    XmlElement ValueElement;

                    #region StandardOut
                    #region Importance
                    ValueElement = Doc.CreateElement("importance");
                    ValueElement.InnerText = InstallerReturn["standardout.importance"].ToString();
                    StandardOutElement.AppendChild(ValueElement);
                    #endregion
                    #endregion
                    #region MySQL
                    #region Host
                    ValueElement = Doc.CreateElement("host");
                    ValueElement.InnerText = InstallerReturn["database.host"].ToString();
                    MySQLElement.AppendChild(ValueElement);
                    #endregion
                    #region Port
                    ValueElement = Doc.CreateElement("port");
                    ValueElement.InnerText = InstallerReturn["database.port"].ToString();
                    MySQLElement.AppendChild(ValueElement);
                    #endregion
                    #region User
                    ValueElement = Doc.CreateElement("user");
                    ValueElement.InnerText = InstallerReturn["database.username"].ToString();
                    MySQLElement.AppendChild(ValueElement);
                    #endregion
                    #region Password
                    ValueElement = Doc.CreateElement("password");
                    ValueElement.InnerText = InstallerReturn["database.password"].ToString();
                    MySQLElement.AppendChild(ValueElement);
                    #endregion
                    #region Database
                    ValueElement = Doc.CreateElement("database");
                    ValueElement.InnerText = InstallerReturn["database.database"].ToString();
                    MySQLElement.AppendChild(ValueElement);
                    #endregion
                    #region MinPoolSize
                    ValueElement = Doc.CreateElement("minpoolsize");
                    ValueElement.InnerText = InstallerReturn["database.minpool"].ToString();
                    MySQLElement.AppendChild(ValueElement);
                    #endregion
                    #region MaxPoolSize
                    ValueElement = Doc.CreateElement("maxpoolsize");
                    ValueElement.InnerText = InstallerReturn["database.maxpool"].ToString();
                    MySQLElement.AppendChild(ValueElement);
                    #endregion
                    #endregion
                    #region Network
                    #region Host
                    ValueElement = Doc.CreateElement("host");
                    ValueElement.InnerText = InstallerReturn["network.game.host"].ToString();
                    NetworkElement.AppendChild(ValueElement);
                    #endregion
                    #region Port
                    ValueElement = Doc.CreateElement("port");
                    ValueElement.InnerText = InstallerReturn["network.game.port"].ToString();
                    NetworkElement.AppendChild(ValueElement);
                    #endregion
                    #region MaxConnections
                    ValueElement = Doc.CreateElement("maxconnections");
                    ValueElement.InnerText = InstallerReturn["network.game.maxconnections"].ToString();
                    NetworkElement.AppendChild(ValueElement);
                    #endregion
                    #endregion
                    #region WebAdmin
                    #region Port
                    ValueElement = Doc.CreateElement("port");
                    ValueElement.InnerText = InstallerReturn["network.webadmin.port"].ToString();
                    WebAdminElement.AppendChild(ValueElement);
                    #endregion
                    #endregion

                    RootElement.AppendChild(StandardOutElement);
                    RootElement.AppendChild(MySQLElement);
                    RootElement.AppendChild(NetworkElement);
                    RootElement.AppendChild(WebAdminElement);
                    this.fConfig.Save();

                    this.fStandardOut.PrintImportant("Configuration file saved!");

                    this.fStandardOut.PrintImportant("Resuming IHI Boot (Installer)");
                    this.fStandardOut.PrintImportant("Press any key...");
                    Console.ReadKey(true);
                }

                this.fStandardOut.PrintNotice("Config File => Loaded");

                this.fStandardOut.SetImportance((StandardOutImportance)fConfig.ValueAsByte("/config/standardout/importance", (byte)StandardOutImportance.Debug));


                this.fStandardOut.PrintNotice("MySQL => Preparing database connection settings...");

                try
                {
                    MySqlConnectionStringBuilder CS = new MySqlConnectionStringBuilder();
                    CS.Server = fConfig.ValueAsString("/config/mysql/host");
                    CS.Port = fConfig.ValueAsUint("/config/mysql/port", 3306);
                    CS.UserID = fConfig.ValueAsString("/config/mysql/user");
                    CS.Password = fConfig.ValueAsString("/config/mysql/password");
                    CS.Database = fConfig.ValueAsString("/config/mysql/database");
                    CS.Pooling = true;
                    CS.MinimumPoolSize = fConfig.ValueAsUint("/config/mysql/minpoolsize", 1);
                    CS.MaximumPoolSize = fConfig.ValueAsUint("/config/mysql/maxpoolsize", 25);

                    PrepareSessionFactory(CS.ConnectionString);

                    this.fStandardOut.PrintNotice("MySQL => Testing connection...");

                    using (ISession DB = GetDatabaseSession())
                    {
                        if (!DB.IsConnected)
                            throw new Exception("Unknown cause");
                    }
                }
                catch (Exception ex)
                {
                    this.fStandardOut.PrintError("MySQL => Connection failed!");
                    this.fStandardOut.PrintException(ex);
                    return false;
                }
                this.fStandardOut.PrintNotice("MySQL => Connected!");



                this.fStandardOut.PrintNotice("Habbo Distributor => Constructing...");
                this.fHabboDistributor = new HabboDistributor();
                this.fStandardOut.PrintNotice("Habbo Distributor => Ready");

                this.fStandardOut.PrintNotice("Habbo Figure Factory => Constructing...");
                this.fHabboFigureFactory = new HabboFigureFactory();
                this.fStandardOut.PrintNotice("Habbo Figure Factory => Ready");

                // TODO: Download Requirements

                this.fStandardOut.PrintNotice("Permission Manager => Constructing...");
                this.fPermissionManager = new PermissionManager();
                this.fStandardOut.PrintNotice("Permission Manager => Ready");

                // TODO: Cache Navigator
                // TODO: Cache Furni
                // TODO: Write Dynamic Client Files
                // TODO: Authenticate with IHINet

                this.fStandardOut.PrintNotice("Connection Manager => Starting...");
                this.fConnectionManager = new IonTcpConnectionManager(fConfig.ValueAsString("/config/network/host"), fConfig.ValueAsInt("/config/network/port", 14478), fConfig.ValueAsInt("/config/network/maxconnections", 2));
                this.fConnectionManager.GetListener().Start();
                this.fStandardOut.PrintNotice("Connection Manager => Ready!");

                this.fStandardOut.PrintNotice("Web Admin => Starting...");
                this.fWebAdminManager = new WebAdminManager(fConfig.ValueAsUshort("/config/webadmin/port", 14480));
                this.fStandardOut.PrintNotice("Web Admin => Ready!");

                this.fStandardOut.PrintNotice("Plugin Loader => Starting...");
                this.fPluginManager = new PluginManager();

                foreach (string Path in this.fPluginManager.GetAllPluginPaths())
                {
                    Plugin P = this.fPluginManager.LoadPluginAtPath(Path);

                    if (P == null)
                        continue;

                    if (P.GetName() == "PluginManager")
                        this.fPluginManager.StartPlugin(P); // TODO: Remove this - debugging only
                }

                this.fStandardOut.PrintNotice("Plugin Loader => Started!");

                this.fStandardOut.PrintImportant("IHI is now functional!");

                return true;
            }
            catch (Exception e)
            {
                this.fStandardOut.PrintException(e);
                return false;
            }
        }

        internal void PrepareSessionFactory(string ConnectionString)
        {
            IDictionary<string, string> Properties = new Dictionary<string, string>();

            Properties.Add("connection.driver_class", "NHibernate.Driver.MySqlDataDriver");
            Properties.Add("connection.connection_string", ConnectionString);
            Properties.Add("proxyfactory.factory_class", "NHibernate.ByteCode.Castle.ProxyFactoryFactory,NHibernate.ByteCode.Castle");
            Properties.Add("dialect", "NHibernate.Dialect.MySQL5Dialect");

            NHCfg.Configuration Configuration = new NHCfg.Configuration();
            Configuration.SetProperties(Properties);

            foreach(FileInfo FI in new DirectoryInfo(Path.Combine(Environment.CurrentDirectory, "database")).GetFiles("*.dll"))
            {
                Configuration.AddAssembly(Assembly.LoadFile(FI.FullName));
            }

            fNHibernateSessionFactory = Configuration.BuildSessionFactory();
        }

        public void Shutdown()
        {
            // TODO: Safe Shutdown
        }
        #endregion
    }
}
