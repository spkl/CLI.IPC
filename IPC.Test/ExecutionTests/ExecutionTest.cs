using System.Diagnostics;
using System.IO;
using System;

namespace spkl.IPC.Test.ExecutionTests;

public abstract class ExecutionTest
{
    protected static string SocketPath => Path.Combine(TestContext.CurrentContext.TestDirectory, "TestSocket");

    protected Process Server { get; set; } = new();

    protected Process Client { get; set; } = new();

    [TearDown]
    public void TearDown()
    {
        try
        {
            if (this.Server.HasExited)
            {
                this.Server.Kill();
            }
        }
        catch (InvalidOperationException)
        {
        }

        try
        {
            if (!this.Client.HasExited)
            {
                this.Client.Kill();
            }
        }
        catch (InvalidOperationException)
        {
        }

        this.Server.Dispose();
        this.Client.Dispose();
    }

    protected Process StartServer<T>() where T : IClientConnectionHandler
    {
        string dynamicServerExe = Path.GetFullPath(Path.Combine(TestContext.CurrentContext.TestDirectory.Replace("IPC.Test", "IPC.Test.DynamicServer"), "spkl.IPC.Test.DynamicServer.exe"));
        Assume.That(dynamicServerExe, Does.Exist);

        ProcessStartInfo serverStart = new(dynamicServerExe);
        serverStart.ArgumentList.Add(SocketPath);
        serverStart.ArgumentList.Add(typeof(T).Assembly.Location);
        serverStart.ArgumentList.Add(typeof(T).FullName!);
        serverStart.RedirectStandardError = true;
        serverStart.RedirectStandardInput = true;
        serverStart.RedirectStandardOutput = true;
        this.Server = new() { StartInfo = serverStart };
        this.Server.Start();

        return Server;
    }

    protected Process StartClient<T>() where T : IHostConnectionHandler
    {
        string dynamicClientExe = Path.GetFullPath(Path.Combine(TestContext.CurrentContext.TestDirectory.Replace("IPC.Test", "IPC.Test.DynamicClient"), "spkl.IPC.Test.DynamicClient.exe"));
        Assume.That(dynamicClientExe, Does.Exist);

        ProcessStartInfo clientStart = new(dynamicClientExe);
        clientStart.ArgumentList.Add(SocketPath);
        clientStart.ArgumentList.Add(typeof(T).Assembly.Location);
        clientStart.ArgumentList.Add(typeof(T).FullName!);
        clientStart.RedirectStandardError = true;
        clientStart.RedirectStandardInput = true;
        clientStart.RedirectStandardOutput = true;
        Client = new() { StartInfo = clientStart };
        this.Client.Start();

        return Client;
    }

    protected void WaitForClientExit()
    {
        this.Client.WaitForExit(5_000);

        Assume.That(this.Server.HasExited,
            Is.False,
            () => $"Server has exited" +
            $"{Environment.NewLine}-- Out:{this.Server.StandardOutput.ReadToEnd()}" +
            $"{Environment.NewLine}-- Error:{this.Server.StandardError.ReadToEnd()}");
        Assert.That(this.Client.HasExited, Is.True, "Client has exited");
    }

    protected void RunServerAndClient<TClientConnectionHandler, THostConnectionHandler>()
        where TClientConnectionHandler : IClientConnectionHandler
        where THostConnectionHandler : IHostConnectionHandler
    {
        StartServer<TClientConnectionHandler>();
        StartClient<THostConnectionHandler>();
        WaitForClientExit();
    }
}
