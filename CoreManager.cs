namespace IHI.Server
{
    public static class CoreManager
    {
        private static Core fServerCore;
        private static Install.Core fInstallerCore;

        /// <summary>
        /// Returns the instance of the server Core.
        /// </summary>
        public static Core GetServerCore()
        {
            return fServerCore;
        }

        /// <summary>
        /// Returns the instance of the installer Core.
        /// </summary>
        public static Install.Core GetInstallerCore()
        {
            return fInstallerCore;
        }

        internal static void InitializeServerCore()
        {
            fServerCore = new Core();
        }

        internal static void InitializeInstallerCore()
        {
            fInstallerCore = new Install.Core();
        }

        internal static void DereferenceInstallerCore()
        {
            fInstallerCore = null;
        }
    }
}