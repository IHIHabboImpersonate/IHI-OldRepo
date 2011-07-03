/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//// http://www.paraesthesia.com/archive/2008/07/16/simplest-embedded-web-server-ever-with-httplistener.aspx ////
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////



using System;
using System.Globalization;
using System.Net;
using System.Threading;

namespace IHI.Server.WebAdmin
{
    public class WebAdminServer : IDisposable
    {
        public event EventHandler<HttpRequestEventArgs> IncomingRequest = null;

        public enum State
        {
            Stopped,
            Stopping,
            Starting,
            Started
        }

        private Thread _connectionManagerThread = null;
        private bool _disposed = false;
        private HttpListener _listener = null;
        private long _runState = (long)State.Stopped;

        public State RunState
        {
            get
            {
                return (State)Interlocked.Read(ref _runState);
            }
        }

        public virtual Guid UniqueId { get; private set; }

        public virtual Uri Url { get; private set; }

        public WebAdminServer(ushort PortNumber)
        {
            if (!HttpListener.IsSupported)
            {
                throw new NotSupportedException("The HttpListener class is not supported on this operating system.");
            }
            this.UniqueId = Guid.NewGuid();
            this._listener = new HttpListener();
            this._listener.Prefixes.Add("http://127.0.0.1:" + PortNumber + "/");
        }

        ~WebAdminServer()
        {
            this.Dispose(false);
        }

        private void ConnectionManagerThreadStart()
        {
            Interlocked.Exchange(ref this._runState, (long)State.Starting);
            try
            {
                if (!this._listener.IsListening)
                {
                    this._listener.Start();
                }
                if (this._listener.IsListening)
                {
                    Interlocked.Exchange(ref this._runState, (long)State.Started);
                }

                try
                {
                    while (RunState == State.Started)
                    {
                        HttpListenerContext context = this._listener.GetContext();
                        this.RaiseIncomingRequest(context);
                    }
                }
                catch (HttpListenerException)
                {
                    // This will occur when the listener gets shut down.
                    // Just swallow it and move on.
                }
            }
            finally
            {
                Interlocked.Exchange(ref this._runState, (long)State.Stopped);
            }
        }

        public virtual void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (this._disposed)
            {
                return;
            }
            if (disposing)
            {
                if (this.RunState != State.Stopped)
                {
                    this.Stop();
                }
                if (this._connectionManagerThread != null)
                {
                    this._connectionManagerThread.Abort();
                    this._connectionManagerThread = null;
                }
            }
            this._disposed = true;
        }

        private void RaiseIncomingRequest(HttpListenerContext context)
        {
            HttpRequestEventArgs e = new HttpRequestEventArgs(context);
            try
            {
                if (this.IncomingRequest != null)
                {
                    this.IncomingRequest.BeginInvoke(this, e, null, null);
                }
            }
            catch
            {
                // Swallow the exception and/or log it, but you probably don't want to exit
                // just because an incoming request handler failed.
            }
        }

        public virtual void Start()
        {
            if (this._connectionManagerThread == null || this._connectionManagerThread.ThreadState == ThreadState.Stopped)
            {
                this._connectionManagerThread = new Thread(new ThreadStart(this.ConnectionManagerThreadStart));
                this._connectionManagerThread.Name = String.Format(CultureInfo.InvariantCulture, "ConnectionManager_{0}", this.UniqueId);
            }
            else if (this._connectionManagerThread.ThreadState == ThreadState.Running)
            {
                throw new ThreadStateException("The request handling process is already running.");
            }

            if (this._connectionManagerThread.ThreadState != ThreadState.Unstarted)
            {
                throw new ThreadStateException("The request handling process was not properly initialized so it could not be started.");
            }
            this._connectionManagerThread.Start();

            long waitTime = DateTime.Now.Ticks + TimeSpan.TicksPerSecond * 10;
            while (this.RunState != State.Started)
            {
                Thread.Sleep(100);
                if (DateTime.Now.Ticks > waitTime)
                {
                    throw new TimeoutException("Unable to start the request handling process.");
                }
            }
        }

        public virtual void Stop()
        {
            // Setting the runstate to something other than "started" and
            // stopping the listener should abort the AddIncomingRequestToQueue
            // method and allow the ConnectionManagerThreadStart sequence to
            // end, which sets the RunState to Stopped.
            Interlocked.Exchange(ref this._runState, (long)State.Stopping);
            if (this._listener.IsListening)
            {
                this._listener.Stop();
            }
            long waitTime = DateTime.Now.Ticks + TimeSpan.TicksPerSecond * 10;
            while (this.RunState != State.Stopped)
            {
                Thread.Sleep(100);
                if (DateTime.Now.Ticks > waitTime)
                {
                    throw new TimeoutException("Unable to stop the web server process.");
                }
            }

            this._connectionManagerThread = null;
        }
    }

    public class HttpRequestEventArgs : EventArgs
    {
        public HttpListenerContext RequestContext { get; private set; }

        public HttpRequestEventArgs(HttpListenerContext requestContext)
        {
            this.RequestContext = requestContext;
        }
    }
}
