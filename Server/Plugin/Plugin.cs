
namespace IHI.Server.Plugin
{
    public abstract class Plugin
    {
        internal string fName;
        internal bool fIsRunning = false;

        /// <summary>
        /// Called when the plugin is started.
        /// </summary>
        public abstract void Start();

        /// <summary>
        /// Called when the plugin is stopped.
        /// </summary>
        public abstract void Stop();

        ///// <summary>
        ///// Called when the plugin is 
        ///// </summary>
        ///// <returns>True on a successful install, false otherwise.</returns>
        //public abstract bool Install();

        ///// <summary>
        ///// Called when the plugin is 
        ///// </summary>
        ///// <returns>True on a successful uninstall, false otherwise.</returns>
        //public abstract bool Uninstall();

        public string GetName()
        {
            return this.fName;
        }
        public bool IsRunning()
        {
            return this.fIsRunning;
        }
    }
}
