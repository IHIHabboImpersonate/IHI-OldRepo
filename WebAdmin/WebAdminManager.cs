using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;

namespace IHI.Server.WebAdmin
{
    public delegate void HttpPathHandler(HttpListenerContext requestContext);

    public class WebAdminManager
    {
        private readonly Dictionary<string, HttpPathHandler> _paths;
        private readonly ushort _port;
        private readonly AutoResetEvent _stopWaiter;

        internal WebAdminManager(ushort port)
        {
            _paths = new Dictionary<string, HttpPathHandler>();
            _port = port;
            _stopWaiter = new AutoResetEvent(false);

            new Thread(Run).Start();
        }

        /// <summary>
        /// Ensures the web server is running.
        /// </summary>
        private void Run()
        {
            using (var listener = new WebAdminServer(_port))
            {
                listener.IncomingRequest += HandlePath;
                listener.Start();

                _stopWaiter.WaitOne();
            }
        }

        private void HandlePath(object sender, HttpRequestEventArgs e)
        {
            var path = e.RequestContext.Request.Url.AbsolutePath;
            lock (_paths)
            {
                if (IsPathHandled(path))
                {
                    CoreManager.GetServerCore().GetStandardOut().PrintDebug("WebAdmin Request [200]: " + path);
                    GetPathHandler(path)(e.RequestContext);
                    return;
                }
            }
            CoreManager.GetServerCore().GetStandardOut().PrintDebug("WebAdmin Request [404]: " + path);

            var response = e.RequestContext.Response;
            var buffer = Encoding.UTF8.GetBytes("Not Handled!");
            response.StatusCode = (int) HttpStatusCode.NotFound;
            response.StatusDescription = "Not Found";
            response.ContentLength64 = buffer.Length;
            response.ContentEncoding = Encoding.UTF8;
            response.AddHeader("plugin-name", "");
            response.OutputStream.Write(buffer, 0, buffer.Length);
            response.OutputStream.Close();
            response.Close();
        }

        /// <summary>
        /// Returns true if a path already has a handler, false otherwise.
        /// </summary>
        /// <param name="path">The path to check.</param>
        public bool IsPathHandled(string path)
        {
            lock (_paths)
                return _paths.ContainsKey(path);
        }

        /// <summary>
        /// Registers a path handler to a path.
        /// </summary>
        /// <param name="path">The path to register to.</param>
        /// <param name="handler">The handler for the path.</param>
        /// <returns>True on success, false on failure (handler already taken).</returns>
        public bool AddPathHandler(string path, HttpPathHandler handler)
        {
            lock (_paths)
            {
                if (IsPathHandled(path))
                    return false;

                _paths.Add(path, handler);
                CoreManager.GetServerCore().GetStandardOut().PrintDebug("WebAdmin handler added: " + path);
                return true;
            }
        }

        /// <summary>
        /// Unregisters the registered path handler of a path.
        /// </summary>
        /// <param name="path">The path to register to.</param>
        /// <returns>True on success, false on failure (handler not registered).</returns>
        public bool RemovePathHandler(string path)
        {
            lock (_paths)
            {
                if (!IsPathHandled(path))
                    return false;

                _paths.Remove(path);
                CoreManager.GetServerCore().GetStandardOut().PrintDebug("WebAdmin handler removed: " + path);
                return true;
            }
        }

        /// <summary>
        /// Get the registered path handler of a path.
        /// </summary>
        /// <param name="path">The path to get the handler of.</param>
        /// <returns>The HttpPathHandler if it is register, null otherwise.</returns>
        public HttpPathHandler GetPathHandler(string path)
        {
            lock (_paths)
            {
                if (!IsPathHandled(path))
                    return null;

                return _paths[path];
            }
        }

        /// <summary>
        /// Stops the web server.
        /// </summary>
        internal void Stop()
        {
            _stopWaiter.Set();
        }


        public static void SendResponse(HttpListenerResponse response, string pluginName, string content)
        {
            CoreManager.GetServerCore().GetStandardOut().PrintDebug("WebAdmin Response [" + pluginName + "]: " + content);
            var buffer = Encoding.UTF8.GetBytes(content);
            response.StatusCode = (int) HttpStatusCode.OK;
            response.StatusDescription = "OK";
            response.ContentType = "text/html; charset=UTF-8";
            response.ContentLength64 = buffer.Length;
            response.ContentEncoding = Encoding.UTF8;
            response.AddHeader("plugin-name", pluginName);
            response.OutputStream.Write(buffer, 0, buffer.Length);
            response.OutputStream.Close();
            response.Close();
        }
    }
}