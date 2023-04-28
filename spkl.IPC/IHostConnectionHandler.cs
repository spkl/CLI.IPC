namespace spkl.IPC
{
    public interface IHostConnectionHandler
    {
        void HandleOutString(string text);

        void HandleErrorString(string text);

        void HandleExit(int exitCode);
    }
}
