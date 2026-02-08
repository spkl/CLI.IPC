using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace spkl.CLI.IPC.Test.Internal;

internal class FileLocker : IDisposable
{
    public static FileLocker Lock(string path)
    {
        string fileLockerExe = Path.GetFullPath(Path.Combine(TestContext.CurrentContext.TestDirectory.Replace("IPC.Test", "IPC.Test.FileLocker"), "spkl.CLI.IPC.Test.FileLocker.exe"));
#if NET6_0_OR_GREATER
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            fileLockerExe = fileLockerExe.Substring(0, fileLockerExe.Length - ".exe".Length);
        }
#endif

        ProcessStartInfo psi = new(fileLockerExe, $"\"{path}\"");
        psi.UseShellExecute = false;
        psi.RedirectStandardOutput = true;

        Process process = new() { StartInfo = psi };
        process.Start();

        string? output = process.StandardOutput.ReadLine();
        Assert.That(output, Is.EqualTo("locked"));

        return new FileLocker(process);
    }

    public Process Process { get; set; }

    private bool isDisposed;

    public FileLocker(Process process)
    {
        this.Process = process;
    }

    public void Dispose()
    {
        if (this.isDisposed)
        {
            return;
        }

        this.isDisposed = true;
        this.Process.Kill();
        this.Process.WaitForExit();
        this.Process.Dispose();
    }
}
