#region GPLv3

// 
// Copyright (C) 2012  Chris Chenery
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
// 

#endregion

#region Usings

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

#endregion

namespace IHI.Server.Plugins
{
    public class PluginManager
    {
        private readonly Dictionary<string, Plugin> _plugins = new Dictionary<string, Plugin>();

        private HashSet<int> _compatibleReleases;

        /// <summary>
        /// Removed (To be rewritten) - Reason: Wrong documention is worse than none.
        /// </summary>
        /// <param name = "name">Removed (To be rewritten) - Reason: Wrong documention is worse than none.</param>
        public Plugin GetPlugin(string name)
        {
            if (_plugins.ContainsKey(name))
                return _plugins[name];
            return null;
        }

        /// <summary>
        ///   Start a plugin.
        /// </summary>
        /// <param name = "plugin">The plugin object you wish to start.</param>
        internal PluginManager StartPlugin(Plugin plugin)
        {
            plugin.Start();
            plugin.StartedResetEvent.Set();
            CoreManager.ServerCore.GetStandardOut().PrintNotice("Plugin " + plugin.Name + " has been started.");
            return this;
        }

        /// <summary>
        ///   Load a plugin at a given path.
        /// </summary>
        /// <param name = "path">The file path of the plugin.</param>
        internal Plugin LoadPluginAtPath(string path)
        {
            if (!new FileInfo(path).Exists)
            {
                CoreManager.ServerCore.GetStandardOut().PrintWarning("Plugin does not exist: " + path);
                return null;
            }

            Assembly pluginAssembly = Assembly.LoadFile(path);
            Type genericPluginType = typeof (Plugin);
            Type specificPluginType = pluginAssembly.GetTypes().FirstOrDefault(T => T.IsSubclassOf(genericPluginType));

            if (specificPluginType == null)
            {
                CoreManager.ServerCore.GetStandardOut().PrintWarning(Path.GetFileNameWithoutExtension(path) +
                                                                     " is in the plugin directory but is not a plugin.")
                    .PrintDebug(path);
                return null;
            }
            if (!IsPluginCompatible(specificPluginType))
                throw new IncompatiblePluginException();

            Plugin pluginInstance = Activator.CreateInstance(specificPluginType) as Plugin;

            if(pluginInstance.Name == null)
                pluginInstance.Name = Path.GetFileNameWithoutExtension(path);
            _plugins.Add(pluginInstance.Name, pluginInstance);

            return pluginInstance;
        }

        /// <summary>
        ///   Returns a string array containing the paths of all DLL files in the plugins directory.
        /// </summary>
        internal static IEnumerable<string> GetAllPotentialPluginPaths()
        {
            return Directory.GetFiles(Path.Combine(Directory.GetCurrentDirectory(), "plugins"), "*.dll",
                                      SearchOption.AllDirectories);
        }

        /// <summary>
        ///   Returns a Plugin array containing all the loaded plugins.
        /// </summary>
        public IEnumerable<Plugin> GetLoadedPlugins()
        {
            Plugin[] returnArray = new Plugin[_plugins.Values.Count];
            _plugins.Values.CopyTo(returnArray, 0);

            return returnArray;
        }

        internal bool IsPluginCompatible(Type pluginType)
        {
            object[] customAttributes = pluginType.GetCustomAttributes(typeof (CompatibilityLockAttribute), false);

            if (customAttributes.Length == 0)
                return true;

            int[] releases;
            if (_compatibleReleases == null)
            {
                releases = new int[customAttributes.Length];
                for (int i = 0; i < customAttributes.Length; i++)
                    releases[i] = (customAttributes[i] as CompatibilityLockAttribute).Release;

                _compatibleReleases = new HashSet<int>(releases);
                return true;
            }

            releases = new int[customAttributes.Length];
            for (int i = 0; i < customAttributes.Length; i++)
                releases[i] = (customAttributes[i] as CompatibilityLockAttribute).Release;

            _compatibleReleases.IntersectWith(releases);
            if (_compatibleReleases.Count == 0)
                return false;
            return true;
        }
    }
}