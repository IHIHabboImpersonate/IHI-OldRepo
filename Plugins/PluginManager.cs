using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace IHI.Server.Plugins
{
    public class PluginManager
    {
        private readonly Dictionary<string, Plugin> _plugins = new Dictionary<string, Plugin>();

        /// <summary>
        /// Load and start a plugin with a relative path to the plugin directory.
        /// </summary>
        /// <param name="name">The filename of the plugin MINUS THE .dll!!</param>
        public Plugin GetPlugin(string name)
        {
            if (_plugins.ContainsKey(name))
                return _plugins[name];
            return null;
        }

        /// <summary>
        /// Start a plugin.
        /// </summary>
        /// <param name="plugin">The plugin object you wish to start.</param>
        internal PluginManager StartPlugin(Plugin plugin)
        {
            plugin.Start();
            CoreManager.GetServerCore().GetStandardOut().PrintNotice("Plugin " + plugin.GetName() + " has been started.");
            return this;
        }

        /// <summary>
        /// Load a plugin at a given path.
        /// </summary>
        /// <param name="path">The file path of the plugin.</param>
        internal Plugin LoadPluginAtPath(string path)
        {
            var pluginAssembly = Assembly.LoadFile(path);
            var pluginType = typeof (Plugin);
            var pluginObject = (from T in pluginAssembly.GetTypes()
                                   where T.IsSubclassOf(pluginType)
                                   select Activator.CreateInstance(T) as Plugin).FirstOrDefault();

            if (pluginObject == null)
            {
                CoreManager.GetServerCore().GetStandardOut().PrintWarning(Path.GetFileNameWithoutExtension(path) +
                                                                          " is in the plugin directory but is not a plugin.")
                    .PrintDebug(path);
                return null;
            }

            pluginObject.fName = Path.GetFileNameWithoutExtension(path);
            _plugins.Add(pluginObject.fName, pluginObject);

            return pluginObject;
        }

        /// <summary>
        /// Returns a string array containing the paths of all DLL files in the plugins directory.
        /// </summary>
        internal static IEnumerable<string> GetAllPotentialPluginPaths()
        {
            return Directory.GetFiles(Path.Combine(Directory.GetCurrentDirectory(), "plugins"), "*.dll",
                                      SearchOption.TopDirectoryOnly);
        }

        /// <summary>
        /// Returns a Plugin array containing all the loaded plugins.
        /// </summary>
        public IEnumerable<Plugin> GetLoadedPlugins()
        {
            var returnArray = new Plugin[_plugins.Values.Count];
            _plugins.Values.CopyTo(returnArray, 0);

            return returnArray;
        }
    }
}