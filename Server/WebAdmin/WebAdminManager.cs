using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

namespace IHI.Server.WebAdmin
{
    public delegate void HttpPathHandler(HttpListenerContext RequestContext);
    
    public class WebAdminManager
    {
        private Dictionary<string, HttpPathHandler> fPaths;
        private AutoResetEvent fStopWaiter;
        private ushort fPortNumber;
        
        internal WebAdminManager(ushort PortNumber)
        {
            this.fPaths = new Dictionary<string, HttpPathHandler>();
            this.fPortNumber = PortNumber;
            this.fStopWaiter = new AutoResetEvent(false);   

            new Thread(new ThreadStart(Run)).Start();
        }

        /// <summary>
        /// Ensures the web server is running.
        /// </summary>
        private void Run()
        {
            using (WebAdminServer listener = new WebAdminServer(this.fPortNumber))
            {
                listener.IncomingRequest += HandlePath;
                listener.Start();

                this.fStopWaiter.WaitOne();
            }
        }
         
        private void HandlePath(object sender, HttpRequestEventArgs e)
        {
            string Path = e.RequestContext.Request.Url.AbsolutePath;
            lock (this.fPaths)
            {
                if (IsPathHandled(Path))
                {
                    Core.GetStandardOut().PrintDebug("WebAdmin Request [200]: " + Path);
                    GetPathHandler(Path)(e.RequestContext);
                    return;
                }
            }
            Core.GetStandardOut().PrintDebug("WebAdmin Request [404]: " + Path);

            HttpListenerResponse Response = e.RequestContext.Response;
            byte[] buffer = Encoding.UTF8.GetBytes("Not Handled!");
            Response.StatusCode = (int)HttpStatusCode.NotFound;
            Response.StatusDescription = "Not Found";
            Response.ContentLength64 = buffer.Length;
            Response.ContentEncoding = Encoding.UTF8;
            Response.AddHeader("plugin-name", "");
            Response.OutputStream.Write(buffer, 0, buffer.Length);
            Response.OutputStream.Close();
            Response.Close();
        }

        /// <summary>
        /// Returns true if a path already has a handler, false otherwise.
        /// </summary>
        /// <param name="Path">The path to check.</param>
        public bool IsPathHandled(string Path)
        {
            lock(this.fPaths)
                return this.fPaths.ContainsKey(Path);
        }

        /// <summary>
        /// Registers a path handler to a path.
        /// </summary>
        /// <param name="Path">The path to register to.</param>
        /// <param name="Handler">The handler for the path.</param>
        /// <returns>True on success, false on failure (handler already taken).</returns>
        public bool AddPathHandler(string Path, HttpPathHandler Handler)
        {
            lock (this.fPaths)
            {
                if (IsPathHandled(Path))
                    return false;

                this.fPaths.Add(Path, Handler);
                Core.GetStandardOut().PrintDebug("WebAdmin handler added: " + Path);
                return true;
            }
        }

        /// <summary>
        /// Unregisters the registered path handler of a path.
        /// </summary>
        /// <param name="Path">The path to register to.</param>
        /// <returns>True on success, false on failure (handler not registered).</returns>
        public bool RemovePathHandler(string Path)
        {
            lock (this.fPaths)
            {
                if (!IsPathHandled(Path))
                    return false;

                this.fPaths.Remove(Path);
                Core.GetStandardOut().PrintDebug("WebAdmin handler removed: " + Path);
                return true;
            }
        }

        /// <summary>
        /// Get the registered path handler of a path.
        /// </summary>
        /// <param name="Path">The path to get the handler of.</param>
        /// <returns>The HttpPathHandler if it is register, null otherwise.</returns>
        public HttpPathHandler GetPathHandler(string Path)
        {
            lock (this.fPaths)
            {
                if (!IsPathHandled(Path))
                    return null;

                return this.fPaths[Path];
            }
        }

        /// <summary>
        /// Stops the web server.
        /// </summary>
        internal void Stop()
        {   
            this.fStopWaiter.Set();
        }


        public void SendResponse(HttpListenerResponse Response, string PluginName, string Content)
        {
            Core.GetStandardOut().PrintDebug("WebAdmin Response [" + PluginName + "]: " + Content);
            byte[] buffer = Encoding.UTF8.GetBytes(Content);
            Response.StatusCode = (int)HttpStatusCode.OK;
            Response.StatusDescription = "OK";
            Response.ContentType = "text/html; charset=UTF-8";
            Response.ContentLength64 = buffer.Length;
            Response.ContentEncoding = Encoding.UTF8;
            Response.AddHeader("plugin-name", PluginName);
            Response.OutputStream.Write(buffer, 0, buffer.Length);
            Response.OutputStream.Close();
            Response.Close();
        }
    } 
}
