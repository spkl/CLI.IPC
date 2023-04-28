namespace spkl.IPC
{
    public interface IClientHandler
    {
        void HandleCall(ClientConnection connection);
    }
}
