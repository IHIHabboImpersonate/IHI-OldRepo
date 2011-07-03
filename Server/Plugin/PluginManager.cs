using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace IHI.Server.Plugin
{
    public class PluginManager
    {
        private Dictionary<string, Plugin> fPlugins = new Dictionary<string, Plugin>();

        /// <summary>
        /// Load and start a plugin with a relative path to the plugin directory.
        /// </summary>
        /// <param name="Filename">The filename of the plugin MINUS THE .dll!!</param>
        public Plugin GetPlugin(string Name)
        {
            if(fPlugins.ContainsKey(Name))
                return this.fPlugins[Name];
            return null;
        }
        /// <summary>
        /// Load and start a plugin with a relative path to the plugin directory.
        /// </summary>
        /// <param name="Filename">The filename of the plugin MINUS THE .dll!!</param>
        public PluginManager StartPlugin(Plugin Plugin)
        {
            if (!Plugin.IsRunning())
            {
                Plugin.Start();
                Plugin.fIsRunning = true;
                Core.GetStandardOut().PrintNotice("Plugin " + Plugin.GetName() + " has been started.");
            }
            return this;
        }
        /// <summary>
        /// Load a plugin at a given path.
        /// </summary>
        /// <param name="Path">The file path of the plugin.</param>
        public Plugin LoadPluginAtPath(string PluginPath)
        {
            Assembly PluginAssembly = Assembly.LoadFile(PluginPath);
            Type PType = typeof(Plugin);
            Plugin PluginObject = null;

            foreach (Type T in PluginAssembly.GetTypes())
            {
                if (T.IsSubclassOf(PType))
                {
                    PluginObject = Activator.CreateInstance(T) as Plugin;
                    break;
                }
            }

            if (PluginObject == null)
            {
                Core.GetStandardOut().PrintWarning("Plugin " + Path.GetFileNameWithoutExtension(PluginPath) + " failed to load!").PrintDebug(PluginPath);
                return null;
            }

            PluginObject.fName = Path.GetFileNameWithoutExtension(PluginPath);

            lock (this.fPlugins)
            {
                this.fPlugins.Add(PluginObject.fName, PluginObject);
            }

            Core.GetStandardOut().PrintNotice("Plugin '" + Path.GetFileNameWithoutExtension(PluginPath) + "' loaded.");
            return PluginObject;
        }

        /// <summary>
        /// Stop and unload a plugin.
        /// </summary>
        /// <param name="Plugin">The plugin object to unload.</param>
        public PluginManager StopPlugin(Plugin Plugin)
        {
            if (Plugin.IsRunning())
            {
                Plugin.Stop();
                Plugin.fIsRunning = false;
                Core.GetStandardOut().PrintNotice("Plugin " + Plugin.GetName() + " has been stopped.");
            }
            return this;
        }

        /// <summary>
        /// Stops all running plugins.
        /// </summary>
        public PluginManager StopAllPlugins()
        {
            lock (this.fPlugins)
            {
                foreach (Plugin Plugin in this.fPlugins.Values)
                {
                    StopPlugin(Plugin);
                }
            }
            return this;
        }

        /// <summary>
        /// Returns a string array containing the paths of all DLL files in the plugin directory.
        /// </summary>
        public string[] GetAllPluginPaths()
        {
            return Directory.GetFiles(Path.Combine(Directory.GetCurrentDirectory(), "plugins"), "*.dll", SearchOption.TopDirectoryOnly);
            
            //FileInfo[] Files = new DirectoryInfo("./plugins/").GetFiles("*.dll", SearchOption.TopDirectoryOnly);

            //string[] Paths = new string[Files.Length];

            //for (int i = 0; i < Files.Length; i++)
            //{
            //    Paths[i] = Files[i].FullName;
            //}
            //return Paths;
        }

        public Plugin[] GetLoadedPlugins()
        {
            Plugin[] ReturnArray = new Plugin[this.fPlugins.Values.Count];
            this.fPlugins.Values.CopyTo(ReturnArray, 0);

            return ReturnArray;
        }
    }
}
