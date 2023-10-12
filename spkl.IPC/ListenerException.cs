using System;

namespace spkl.IPC
{
    public class ListenerException
    {
        /// <summary>
        /// The exception that occurred.
        /// </summary>
        public Exception Exception { get; }

        /// <summary>
        /// If this is true, the <see cref="Host"/> is no longer accepting new connections and should be shut down.
        /// </summary>
        public bool HostWasShutDown { get; }

        public ListenerException(Exception exception, bool hostWasShutDown)
        {
            this.Exception = exception;
            this.HostWasShutDown = hostWasShutDown;
        }
    }
}
