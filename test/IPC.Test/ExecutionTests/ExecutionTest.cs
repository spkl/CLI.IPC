using System.Diagnostics;
using System.IO;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Linq;

namespace spkl.IPC.Test.ExecutionTests;

internal abstract class ExecutionTest : TestBase
{
#if NET6_0_OR_GREATER
    protected static string TransportArgument => Path.Combine(TestContext.CurrentContext.TestDirectory, "TestSocket");
#else
    protected static string TransportArgument => "65056";
#endif

    protected Process Host { get; set; } = new();

    protected List<string> HostArguments { get; private set; } = new();

    protected Process Client { get; set; } = new();

    protected List<string> ClientArguments { get; private set; } = new();

    protected List<Process> Clients { get; set; } = new();

    [TearDown]
    public void TearDown()
    {
        try
        {
            if (!this.Host.HasExited)
            {
                this.Host.Kill();
            }
        }
        catch (InvalidOperationException)
        {
        }

        this.Host.Dispose();

        foreach (Process client in this.Clients)
        {
            try
            {
                if (!client.HasExited)
                {
                    client.Kill();
                }
            }
            catch (InvalidOperationException)
            {
            }

            client.Dispose();
        }
    }

    protected Process StartHost<T>() where T : IClientConnectionHandler
    {
        string dynamicHostExe = Path.GetFullPath(Path.Combine(TestContext.CurrentContext.TestDirectory.Replace("IPC.Test", "IPC.Test.DynamicHost"), "spkl.IPC.Test.DynamicHost.exe"));
        Assume.That(dynamicHostExe, Does.Exist);

        ProcessStartInfo hostStart = new(dynamicHostExe);
        this.HostArguments = new List<string>()
        {
            TransportArgument,
            typeof(T).Assembly.Location,
            typeof(T).FullName!,
        };
        hostStart.Arguments = string.Join(" ", this.HostArguments.Select(arg => $@"""{arg}"""));
        hostStart.RedirectStandardError = true;
        hostStart.RedirectStandardInput = true;
        hostStart.RedirectStandardOutput = true;
        hostStart.UseShellExecute = false;
        this.Host = new() { StartInfo = hostStart };
        this.Host.Start();
        Thread.Sleep(500);

        return this.Host;
    }

    protected Process StartClient<T>() where T : IHostConnectionHandler
    {
        string dynamicClientExe = Path.GetFullPath(Path.Combine(TestContext.CurrentContext.TestDirectory.Replace("IPC.Test", "IPC.Test.DynamicClient"), "spkl.IPC.Test.DynamicClient.exe"));
        Assume.That(dynamicClientExe, Does.Exist);

        ProcessStartInfo clientStart = new(dynamicClientExe);
        this.ClientArguments = new List<string>()
        {
            TransportArgument,
            typeof(T).Assembly.Location,
            typeof(T).FullName!,
        };
        clientStart.Arguments = string.Join(" ", this.ClientArguments.Select(arg => $@"""{arg}"""));
        clientStart.RedirectStandardError = true;
        clientStart.RedirectStandardInput = true;
        clientStart.RedirectStandardOutput = true;
        clientStart.UseShellExecute = false;
        this.Client = new() { StartInfo = clientStart };
        this.Client.Start();

        this.Clients.Add(this.Client);
        return this.Client;
    }

    protected void WaitForClientExit()
    {
        this.WaitForClientExit(this.Client);
    }

    protected void WaitForClientExit(Process client)
    {
        Assume.That(this.Host.HasExited,
            Is.False,
            () => $"Host has exited" +
            $"{Environment.NewLine}-- Out:{this.Host.StandardOutput.ReadToEnd()}" +
            $"{Environment.NewLine}-- Error:{this.Host.StandardError.ReadToEnd()}");
        Assert.That(client.WaitForExit(5_000), Is.True, "Client has exited");
    }

    protected void RunHostAndClient<TClientConnectionHandler, THostConnectionHandler>()
        where TClientConnectionHandler : IClientConnectionHandler
        where THostConnectionHandler : IHostConnectionHandler
    {
        this.StartHost<TClientConnectionHandler>();
        Process client = this.StartClient<THostConnectionHandler>();
        this.WaitForClientExit(client);
    }
}
