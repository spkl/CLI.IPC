using System.Diagnostics.CodeAnalysis;

namespace spkl.IPC
{
    public interface IHostConnectionHandler
    {
        [NotNull]
        string[] Arguments { get; }

        [NotNull]
        string CurrentDirectory { get; }

        void HandleOutString(string text);

        void HandleErrorString(string text);

        void HandleExit(int exitCode);
    }
}
