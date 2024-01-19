using spkl.IPC.Startup;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace spkl.IPC.Test.Singleton;

internal class Program
{
    private static string AssemblyDir => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;

    private const int HostAliveTime_Seconds = 5;

    private const int PollingPeriod_Milliseconds = 250;

    private const int TimeoutThreshold_Seconds = 10;

    private const int HostStartupDelay_Seconds = 1;

    private const int ClientConnectionDuration_Seconds = 1;

    static void Main(string[] args)
    {
        ITransport transport;
#if NET6_0_OR_GREATER
        transport = new UdsTransport(Path.Combine(Program.AssemblyDir, "sock"));
#else
        transport = new TcpLoopbackTransport(65049);
#endif

        StartupBehavior b = new();
        SingletonApplication s = new(b);

        if (args.Length == 1 && args[0] == "host")
        {
            Thread.Sleep(TimeSpan.FromSeconds(HostStartupDelay_Seconds));

            Host h = Host.Start(transport, new ClientConnectionHandler());
            s.ReportInstanceRunning();

            Thread.Sleep(TimeSpan.FromSeconds(Program.HostAliveTime_Seconds));

            s.ShutdownInstance();
            h.Shutdown();
            h.WaitUntilAllClientsDisconnected();
        }
        else
        {
            s.RequestInstance();

            int tries = 3;
            while (tries > 0)
            {
                tries--;
                try
                {
                    Client.Attach(transport, new DefaultHostConnectionHandler());
                    break;
                }
                catch (ConnectionException)
                {
                    Thread.Sleep(100);
                    if (tries == 0)
                    {
                        throw;
                    }
                }
            }
        }
    }

    private class StartupBehavior : IStartupBehavior
    {
        public string NegotiationFileBasePath => Path.Combine(Program.AssemblyDir, "singleton");

        public TimeSpan PollingPeriod { get; } = TimeSpan.FromMilliseconds(Program.PollingPeriod_Milliseconds);

        public TimeSpan TimeoutThreshold { get; } = TimeSpan.FromSeconds(Program.TimeoutThreshold_Seconds);

        public void StartInstance()
        {
            string executablePath = "spkl.IPC.Test.Singleton.exe";
#if NET6_0_OR_GREATER
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                executablePath = executablePath.Substring(0, executablePath.Length - ".exe".Length);
            }
#endif

            Process p = new Process();
            p.StartInfo = new ProcessStartInfo(Path.Combine(Program.AssemblyDir, executablePath), "host");
            p.StartInfo.UseShellExecute = false;
            p.Start();
        }
    }

    private class ClientConnectionHandler : IClientConnectionHandler
    {
        public TaskFactory TaskFactory => Task.Factory;

        public void HandleCall(ClientConnection connection)
        {
            int processId;
#if NET6_0_OR_GREATER
            processId = Environment.ProcessId;
#else
            using Process p = Process.GetCurrentProcess();
            processId = p.Id;
#endif

            connection.Out.WriteLine("PID " + processId);

            Thread.Sleep(TimeSpan.FromSeconds(Program.ClientConnectionDuration_Seconds));

            connection.Exit(0);
        }

        public void HandleListenerError(ListenerError error)
        {
            ExceptionDispatchInfo.Capture(error.Exception).Throw();
        }
    }
}
