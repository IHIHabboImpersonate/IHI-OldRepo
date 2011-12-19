namespace IHI.Server
{
    public static class CoreManager
    {
        /// <summary>
        /// The instance of the server Core
        /// </summary>
        public static Core ServerCore
        {
            get;
            private set;
        }
        /// <summary>
        /// The instance of the installer Core.
        /// </summary>
        public static Install.Core InstallerCore
        {
            get;
            private set;
        }

        internal static void InitialiseServerCore()
        {
            ServerCore = new Core();
        }

        internal static void InitialiseInstallerCore()
        {
            InstallerCore = new Install.Core();
        }

        internal static void DereferenceInstallerCore()
        {
            InstallerCore = null;
        }
    }
}