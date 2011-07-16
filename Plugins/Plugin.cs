
namespace IHI.Server.Plugins
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
