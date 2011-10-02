namespace IHI.Server.Plugins
{
    public abstract class Plugin
    {
        internal string fName;

        /// <summary>
        /// Called when the plugin is started.
        /// </summary>
        public abstract void Start();


        public string GetName()
        {
            return fName;
        }
    }
}