namespace spkl.IPC
{
    public interface IClientConnectionHandler
    {
        void HandleCall(ClientConnection connection);
    }
}
