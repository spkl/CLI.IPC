namespace spkl.IPC
{
    public interface IClientConnectionHandler
    {
        /// <summary>
        /// Handles an incoming client connection.
        /// This method must implement its own exception handling, otherwise exceptions might be left unhandled.
        /// </summary>
        /// <remarks>
        /// The connection is NOT automatically closed when this method returns: Use <see cref="ClientConnection.Exit(int)"/> to close the connection.
        /// If this method throws an exception, the connection IS automatically closed.
        /// </remarks>
        /// <param name="connection">Object to interact with the connected client.</param>
        void HandleCall(ClientConnection connection);

        /// <summary>
        /// Handles an error that occurs while listening for client connections.
        /// </summary>
        void HandleListenerError(ListenerError error);
    }
}
