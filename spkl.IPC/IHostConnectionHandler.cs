namespace spkl.IPC
{
    public interface IHostConnectionHandler
    {
        string[] Arguments { get; }

        string CurrentDirectory { get; }

        void HandleOutString(string text);

        void HandleErrorString(string text);

        void HandleExit(int exitCode);
    }
}
