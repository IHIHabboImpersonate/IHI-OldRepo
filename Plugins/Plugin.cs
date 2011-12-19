namespace IHI.Server.Plugins
{
    public abstract class Plugin
    {
        public string Name
        {
            get;
            internal set;
        }

        /// <summary>
        /// Called when the plugin is started.
        /// </summary>
        public abstract void Start();
    }
}