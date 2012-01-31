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
using System;
using System.Globalization;
using System.Net;
using System.Threading;

namespace IHI.Server.WebAdmin
{
    public sealed class WebAdminServer : IDisposable
    {
        #region State enum

        public enum State
        {
            Stopped,
            Stopping,
            Starting,
            Started
        }

        #endregion

        private readonly HttpListener _listener;

        private Thread _connectionManagerThread;
        private bool _disposed;
        private long _runState = (long) State.Stopped;

        public WebAdminServer(ushort port)
        {
            if (!HttpListener.IsSupported)
            {
                throw new NotSupportedException("The HttpListener class is not supported on this operating system.");
            }
            UniqueId = Guid.NewGuid();
            _listener = new HttpListener();
            _listener.Prefixes.Add("http://127.0.0.1:" + port + "/");
            _listener.Prefixes.Add("http://localhost:" + port + "/");
        }

        private State RunState
        {
            get { return (State) Interlocked.Read(ref _runState); }
        }

        private Guid UniqueId { get; set; }

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        public event EventHandler<HttpRequestEventArgs> IncomingRequest = null;

        ~WebAdminServer()
        {
            Dispose(false);
        }

        private void ConnectionManagerThreadStart()
        {
            Interlocked.Exchange(ref _runState, (long) State.Starting);
            try
            {
                if (!_listener.IsListening)
                {
                    _listener.Start();
                }
                if (_listener.IsListening)
                {
                    Interlocked.Exchange(ref _runState, (long) State.Started);
                }

                try
                {
                    while (RunState == State.Started)
                    {
                        HttpListenerContext context = _listener.GetContext();
                        RaiseIncomingRequest(context);
                    }
                }
                catch (HttpListenerException)
                {
                    // This will occur when the listener gets shut down.
                    // Just swallow it and move on.
                }
            }
            catch (HttpListenerException e)
            {
                if (e.Message == "The process cannot access the file because it is being used by another process")
                    throw new Exception("The WebAdminServer was unable to start. Is the port already in use?", e);
            }
            finally
            {
                Interlocked.Exchange(ref _runState, (long) State.Stopped);
            }
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }
            if (disposing)
            {
                if (RunState != State.Stopped)
                {
                    Stop();
                }
                if (_connectionManagerThread != null)
                {
                    _connectionManagerThread.Abort();
                    _connectionManagerThread = null;
                }
            }
            _disposed = true;
        }

        private void RaiseIncomingRequest(HttpListenerContext context)
        {
            HttpRequestEventArgs e = new HttpRequestEventArgs(context);
            try
            {
                if (IncomingRequest != null)
                {
                    IncomingRequest.BeginInvoke(this, e, null, null);
                }
            }
            catch
            {
                return;
            }
        }

        public void Start()
        {
            if (_connectionManagerThread == null || _connectionManagerThread.ThreadState == ThreadState.Stopped)
            {
                _connectionManagerThread = new Thread(ConnectionManagerThreadStart)
                                               {
                                                   Name =
                                                       String.Format(CultureInfo.InvariantCulture,
                                                                     "ConnectionManager_{0}",
                                                                     UniqueId)
                                               };
            }
            else if (_connectionManagerThread.ThreadState == ThreadState.Running)
            {
                throw new ThreadStateException("The request handling process is already running.");
            }

            if (_connectionManagerThread.ThreadState != ThreadState.Unstarted)
            {
                throw new ThreadStateException(
                    "The request handling process was not properly initialized so it could not be started.");
            }
            _connectionManagerThread.Start();

            long waitTime = DateTime.Now.Ticks + TimeSpan.TicksPerSecond*10;
            while (RunState != State.Started)
            {
                Thread.Sleep(100);
                if (DateTime.Now.Ticks > waitTime)
                {
                    throw new TimeoutException("Unable to start the request handling process.");
                }
            }
        }

        private void Stop()
        {
            // Setting the runstate to something other than "started" and
            // stopping the listener should abort the AddIncomingRequestToQueue
            // method and allow the ConnectionManagerThreadStart sequence to
            // end, which sets the RunState to Stopped.
            Interlocked.Exchange(ref _runState, (long) State.Stopping);
            if (_listener.IsListening)
            {
                _listener.Stop();
            }
            long waitTime = DateTime.Now.Ticks + TimeSpan.TicksPerSecond*10;
            while (RunState != State.Stopped)
            {
                Thread.Sleep(100);
                if (DateTime.Now.Ticks > waitTime)
                {
                    throw new TimeoutException("Unable to stop the web server process.");
                }
            }

            _connectionManagerThread = null;
        }
    }

    public class HttpRequestEventArgs : EventArgs
    {
        public HttpRequestEventArgs(HttpListenerContext requestContext)
        {
            RequestContext = requestContext;
        }

        public HttpListenerContext RequestContext { get; private set; }
    }
}