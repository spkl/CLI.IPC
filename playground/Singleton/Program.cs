using spkl.IPC;
using spkl.IPC.Startup;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.ExceptionServices;

namespace Singleton;

internal class Program
{
    private const string UdsPath = @"C:\Users\Sebastian\Source\Repos\spkl.IPC\playground\Singleton\bin\Debug\net6.0\singleton.sock";

    static void Main(string[] args)
    {
        StartupBehavior b = new();
        spkl.IPC.Startup.Singleton s = new(b);

        if (args.Length == 1 && args[0] == "host")
        {
            Host h = Host.Start(new UdsTransport(UdsPath), new ClientConnectionHandler());
            s.ReportInstanceRunning();
            Thread.Sleep(TimeSpan.FromSeconds(10));
            s.ShutdownInstance();
            h.Shutdown();
        }
        else
        {
            s.RequestInstance();
            Client.Attach(new UdsTransport(UdsPath), new DefaultHostConnectionHandler());
        }
    }

    private class StartupBehavior : IStartupBehavior
    {
        public string NegotiationFileBasePath => @"C:\Users\Sebastian\Source\Repos\spkl.IPC\playground\Singleton\bin\Debug\net6.0\singleton";

        public TimeSpan PollingPeriod { get; } = TimeSpan.FromMilliseconds(250);

        public TimeSpan TimeoutThreshold { get; } = TimeSpan.FromSeconds(10);

        public void StartInstance()
        {
            Process.Start(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Singleton.exe"), "host");
        }
    }

    private class ClientConnectionHandler : IClientConnectionHandler
    {
        public void HandleCall(ClientConnection connection)
        {
            connection.Out.WriteLine("Hello from " + Process.GetCurrentProcess().Id);
            connection.Exit(0);
        }

        public void HandleListenerError(ListenerError error)
        {
            ExceptionDispatchInfo.Capture(error.Exception).Throw();
        }
    }
}
