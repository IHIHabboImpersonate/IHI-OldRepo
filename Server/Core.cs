using System;
using System.Collections.Generic;
using System.Text;
using IHI.Server.Configuration;
using IHI.Server.Plugin;
using IHI.Server.Users.Permissions;
using IHI.Server.Networking;
using IHI.Server.WebAdmin;

using NHibernate;
using NHCfg = NHibernate.Cfg;

using MySql.Data.MySqlClient;

namespace IHI.Server
{
    public static class Core
    {
        #region Fields
        private static HabboDistributor fHabboDistributor;
        private static StandardOut fStandardOut;
        private static ISessionFactory fNHibernateSessionFactory;
        private static XmlConfig fConfig;
        private static IonTcpConnectionManager fConnectionManager;
        private static Encoding fTextEncoding;
        private static PermissionManager fPermissionManager;
        private static PluginManager fPluginManager;
        private static WebAdminManager fWebAdminManager;
        #endregion

        #region API
        public static HabboDistributor GetUserDistributor()
        {
            return fHabboDistributor;
        }
        public static StandardOut GetStandardOut()
        {
            return fStandardOut;
        }
        public static ISession GetDatabaseSession()
        {
            return fNHibernateSessionFactory.OpenSession();
        }
        public static XmlConfig GetConfig()
        {
            return fConfig;
        }
        public static IonTcpConnectionManager GetConnectionManager()
        {
            return fConnectionManager;
        }
        public static Encoding GetTextEncoding()
        {
            return fTextEncoding;
        }
        public static PermissionManager GetPermissionManager()
        {
            return fPermissionManager;
        }
        public static WebAdminManager GetWebAdminManager()
        {
            return fWebAdminManager;
        }
        public static PluginManager GetPluginManager()
        {
            return fPluginManager;
        }
        #endregion

        #region InternalMethods
        internal static bool Boot(string ConfigPath)
        {
            fTextEncoding = Encoding.UTF8;  // TODO: Move this to an external config.

            fStandardOut = new StandardOut();

            try
            {
                fStandardOut.PrintNotice("Text Encoding => Set to " + fTextEncoding.EncodingName);
                fStandardOut.PrintNotice("Standard Out => Ready");

                fConfig = new XmlConfig(ConfigPath);

                if (fConfig.WasCreated()) // Did the config file have to be created?
                {
                    // Yes, run the install process.

                    fStandardOut.PrintImportant("No config file found! File created!");
                    fStandardOut.PrintImportant("Starting installer...");

                    fStandardOut.PrintNotice("Standard Out => Disabled (Install)");


                    Dictionary<string, object> InstallerReturn = Install.Core.Run();
                    MonoAware.System.Console.Clear();

                    fStandardOut.PrintNotice("Standard Out => Enabled (Install)");
                    fStandardOut.PrintImportant("Updating configuration file...");
                    
                    System.Xml.XmlDocument inDoc = fConfig.GetInternalDocument();

                    System.Xml.XmlNode inDocRootNode = inDoc.GetElementsByTagName("config")[0];
                    System.Xml.XmlElement inDocMySQLElement = inDoc.CreateElement("mysql");
                    System.Xml.XmlElement inDocTempElement;

                    inDocTempElement = inDoc.CreateElement("host");
                    inDocTempElement.InnerText = InstallerReturn["database.host"].ToString();
                    inDocMySQLElement.AppendChild(inDocTempElement);

                    inDocTempElement = inDoc.CreateElement("port");
                    inDocTempElement.InnerText = InstallerReturn["database.port"].ToString();
                    inDocMySQLElement.AppendChild(inDocTempElement);

                    inDocTempElement = inDoc.CreateElement("user");
                    inDocTempElement.InnerText = InstallerReturn["database.username"].ToString();
                    inDocMySQLElement.AppendChild(inDocTempElement);

                    inDocTempElement = inDoc.CreateElement("password");
                    inDocTempElement.InnerText = InstallerReturn["database.password"].ToString();
                    inDocMySQLElement.AppendChild(inDocTempElement);

                    inDocTempElement = inDoc.CreateElement("database");
                    
                    inDocTempElement.InnerText = InstallerReturn["database.database"].ToString();
                    inDocMySQLElement.AppendChild(inDocTempElement);

                    inDocTempElement = inDoc.CreateElement("minpoolsize");
                    inDocTempElement.InnerText = InstallerReturn["database.minpool"].ToString();
                    inDocMySQLElement.AppendChild(inDocTempElement);

                    inDocTempElement = inDoc.CreateElement("maxpoolsize");
                    inDocTempElement.InnerText = InstallerReturn["database.maxpool"].ToString();
                    inDocMySQLElement.AppendChild(inDocTempElement);

                    inDocRootNode.AppendChild(inDocMySQLElement);

                    fConfig.Save();

                    fStandardOut.PrintImportant("Configuration file saved!");

                    fStandardOut.PrintImportant("Resuming IHI Boot (Installer)");
                    fStandardOut.PrintImportant("Press any key...");
                    Console.ReadKey(true);
                }

                fStandardOut.PrintNotice("Config File => Loaded");

                fStandardOut.SetImportance((StandardOutImportance)fConfig.ValueAsByte("/config/standardout/importance", (byte)StandardOutImportance.Debug));


                fStandardOut.PrintNotice("MySQL => Preparing database connection settings...");

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

                    fStandardOut.PrintNotice("MySQL => Testing connection...");

                    using (ISession DB = GetDatabaseSession())
                    {
                        if (!DB.IsConnected)
                            throw new Exception("Unknown cause");
                    }
                }
                catch (Exception ex)
                {
                    fStandardOut.PrintError("MySQL => Connection failed!");
                    fStandardOut.PrintException(ex);
                    return false;
                }
                fStandardOut.PrintNotice("MySQL => Connected!");



                fStandardOut.PrintNotice("Habbo Distributor => Constructing...");
                fHabboDistributor = new HabboDistributor();
                fStandardOut.PrintNotice("Habbo Distributor => Ready");

                // TODO: Download Requirements

                fStandardOut.PrintNotice("Permission Manager => Constructing...");
                fPermissionManager = new PermissionManager();
                fStandardOut.PrintNotice("Permission Manager => Ready");

                // TODO: Cache Navigator
                // TODO: Cache Furni
                // TODO: Write Dynamic Client Files
                // TODO: Authenticate with IHINet

                fStandardOut.PrintNotice("Connection Manager => Starting...");
                fConnectionManager = new IonTcpConnectionManager(fConfig.ValueAsString("/config/network/host"), fConfig.ValueAsInt("/config/network/port", 14478), fConfig.ValueAsInt("/config/network/maxconnections", 2));
                fConnectionManager.GetListener().Start();
                fStandardOut.PrintNotice("Connection Manager => Ready!");

                fStandardOut.PrintNotice("Web Admin => Starting...");
                fWebAdminManager = new WebAdminManager(fConfig.ValueAsUshort("/config/webadmin/port", 14480));
                fStandardOut.PrintNotice("Web Admin => Ready!");

                fStandardOut.PrintNotice("Plugin Loader => Starting...");
                fPluginManager = new PluginManager();
                
                foreach (string Path in fPluginManager.GetAllPluginPaths())
                {
                    Plugin.Plugin P = fPluginManager.LoadPluginAtPath(Path);

                    if (P == null)
                        continue;

                    if(P.GetName() == "WEB_PluginManager")
                        fPluginManager.StartPlugin(P); // TODO: Remove this - debugging only
                }
                                
                fStandardOut.PrintNotice("Plugin Loader => Started!");

                fStandardOut.PrintImportant("IHI is now functional!");

                return true;
            }
            catch (Exception e)
            {
                fStandardOut.PrintException(e);
                return false;
            }
        }

        internal static void PrepareSessionFactory(string ConnectionString)
        {
            IDictionary<string, string> Properties = new Dictionary<string, string>();

            Properties.Add("connection.driver_class", "NHibernate.Driver.MySqlDataDriver");
            Properties.Add("connection.connection_string", ConnectionString);
            Properties.Add("proxyfactory.factory_class", "NHibernate.ByteCode.Castle.ProxyFactoryFactory,NHibernate.ByteCode.Castle");
            Properties.Add("dialect", "NHibernate.Dialect.MySQL5Dialect");

            NHCfg.Configuration Configuration = new NHCfg.Configuration();
            Configuration.SetProperties(Properties);
            Configuration.AddAssembly("IHI.Database.Main");

            fNHibernateSessionFactory = Configuration.BuildSessionFactory();
        }

        internal static void Destroy()
        {
            // TODO: Safe Shutdown
        }
        #endregion
    }
}
