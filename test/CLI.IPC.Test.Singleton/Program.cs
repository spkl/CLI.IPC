using spkl.CLI.IPC.Startup;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace spkl.CLI.IPC.Test.Singleton;

internal class Program
{
    private static string AssemblyDir => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;

    private static readonly TimeSpan HostAliveTime = TimeSpan.FromSeconds(2);

    private static readonly TimeSpan HostShutdownGracePeriod = TimeSpan.FromSeconds(0.25);

    private static readonly TimeSpan PollingPeriod = TimeSpan.FromSeconds(0.25);

    private static readonly TimeSpan TimeoutThreshold = TimeSpan.FromSeconds(10);

    private static readonly TimeSpan HostStartupDelay = TimeSpan.FromSeconds(0.5);

    private static readonly TimeSpan ClientConnectionDuration = TimeSpan.FromSeconds(0.5);

    private const string ARG_HOST = "host";
    private const string ARG_STATIC_TIME = "staticTime";
    private const string ARG_UNTIL_UNUSED = "untilUnused";

    static void Main(string[] args)
    {
        if (args.Length == 0 || args[0] is not (ARG_HOST or ARG_STATIC_TIME or ARG_UNTIL_UNUSED))
        {
            throw new Exception($"Specify either '{ARG_HOST}', '{ARG_STATIC_TIME}' or '{ARG_UNTIL_UNUSED}' as first argument.");
        }

        string timeoutBehavior = args[0];
        if (args[0] == ARG_HOST)
        {
            if (args.Length < 2 || args[1] is not (ARG_STATIC_TIME or ARG_UNTIL_UNUSED))
            {
                throw new Exception($"Specify either '{ARG_STATIC_TIME}' or '{ARG_UNTIL_UNUSED}' as second argument.");
            }

            timeoutBehavior = args[1];
        }

        StartupBehavior b = new(timeoutBehavior);
        AutoTransportSingletonApp s = new(b);

        if (args[0] == ARG_HOST)
        {
            Thread.Sleep(Program.HostStartupDelay);

            Host h = Host.Start(s.Transport, new ClientConnectionHandler());
            s.ReportInstanceRunning();

            if (timeoutBehavior == ARG_STATIC_TIME)
            {
                Thread.Sleep(Program.HostAliveTime);
            }
            else
            {
                h.WaitUntilUnusedFor(Program.HostAliveTime);
            }

            s.ReportInstanceShuttingDown();
            Thread.Sleep(Program.HostShutdownGracePeriod);
            h.Shutdown();
            h.WaitUntilAllClientsDisconnected();
        }
        else
        {
            s.RequestInstance();
            Client.Attach(s.Transport, new DefaultHostConnectionHandler());
        }
    }

    private class StartupBehavior : IStartupBehavior
    {
        public string NegotiationFileBasePath => Path.Combine(Program.AssemblyDir, "s");

        public TimeSpan PollingPeriod { get; } = Program.PollingPeriod;

        public TimeSpan TimeoutThreshold { get; } = Program.TimeoutThreshold;

        private readonly string timeoutBehavior;

        public StartupBehavior(string timeoutBehavior)
        {
            this.timeoutBehavior = timeoutBehavior;
        }

        public void StartInstance()
        {
            string executablePath = "spkl.CLI.IPC.Test.Singleton.exe";
#if NET6_0_OR_GREATER
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                executablePath = executablePath.Substring(0, executablePath.Length - ".exe".Length);
            }
#endif

            Process p = new Process();
            p.StartInfo = new ProcessStartInfo(Path.Combine(Program.AssemblyDir, executablePath), $"{ARG_HOST} {this.timeoutBehavior}");
            p.StartInfo.UseShellExecute = false;
            p.Start();
        }
    }

    private class ClientConnectionHandler : IClientConnectionHandler
    {
        public TaskFactory TaskFactory => Task.Factory;

        public void HandleCall(IClientConnection connection)
        {
            int processId;
#if NET6_0_OR_GREATER
            processId = Environment.ProcessId;
#else
            using Process p = Process.GetCurrentProcess();
            processId = p.Id;
#endif

            connection.Out.Write("PID " + processId);

            Thread.Sleep(Program.ClientConnectionDuration);

            connection.Exit(0);
        }

        public void HandleListenerError(IListenerError error)
        {
            ExceptionDispatchInfo.Capture(error.Exception).Throw();
        }
    }
}
