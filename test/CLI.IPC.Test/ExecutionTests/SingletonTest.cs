using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

namespace spkl.CLI.IPC.Test.ExecutionTests;

#if !NET6_0_OR_GREATER
[Platform(Exclude = "Linux")]
#endif
internal class SingletonTest : TestBase
{
    /// <summary>
    /// Approach:
    /// - staticTime: Host lifetime is 5 seconds, so if we continuously create new clients for 8 seconds, there should be exactly two host processes.
    /// - untilUnused: Host lifetime is "last used time" plus 5 seconds, so there should be only one host process.
    /// </summary>
    [Test]
    [TestCase("staticTime", 2)]
    [TestCase("untilUnused", 1)]
    public void TestSingleton(string timeoutBehavior, int expectedProcesses)
    {
        // arrange
        string singletonExe = Path.GetFullPath(Path.Combine(TestContext.CurrentContext.TestDirectory.Replace("IPC.Test", "IPC.Test.Singleton"), "spkl.CLI.IPC.Test.Singleton.exe"));
#if NET6_0_OR_GREATER
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            singletonExe = singletonExe.Substring(0, singletonExe.Length - ".exe".Length);
        }
#endif

        List<Thread> threads = new List<Thread>();
        ConcurrentQueue<int> exitCodes = new ConcurrentQueue<int>();
        ConcurrentQueue<string> stdOutputs = new ConcurrentQueue<string>();
        ConcurrentQueue<string> errOutputs = new ConcurrentQueue<string>();
        TimeSpan testDuration = TimeSpan.FromSeconds(8);
        DateTime testStart = DateTime.Now;
        int startedProcesses = 0;

        // act
        while ((DateTime.Now - testStart) < testDuration)
        {
            Thread t = new Thread(() =>
            {
                ProcessStartInfo psi = new ProcessStartInfo(singletonExe, timeoutBehavior);
                psi.UseShellExecute = false;
                psi.RedirectStandardError = true;
                psi.RedirectStandardOutput = true;

                Process p = new();
                p.StartInfo = psi;
                p.Start();
                Interlocked.Increment(ref startedProcesses);

                stdOutputs.Enqueue(p.StandardOutput.ReadToEnd());
                errOutputs.Enqueue(p.StandardError.ReadToEnd());
                p.WaitForExit();
                exitCodes.Enqueue(p.ExitCode);
            });
            t.Start();
            threads.Add(t);

            Thread.Sleep(100);
        }

        foreach (Thread t in threads)
        {
            t.Join();
        }

        Thread.Sleep(TimeSpan.FromSeconds(3)); // Grace period for host shutdown.

        // assert
        TestContext.Out.WriteLine($"{startedProcesses} processes were started.");
        using (new AssertionScope())
        {
            exitCodes.Should().HaveCount(startedProcesses);
            stdOutputs.Should().HaveCount(startedProcesses);
            errOutputs.Should().HaveCount(startedProcesses);
            exitCodes.Should().AllSatisfy(exitCode => exitCode.Should().Be(0));
            stdOutputs.Distinct().Should().HaveCount(expectedProcesses).And.AllSatisfy(output => output.Should().StartWith("PID "));
            errOutputs.Should().AllSatisfy(errOutput => errOutput.Should().BeEmpty());
            Process.GetProcessesByName("spkl.CLI.IPC.Test.Singleton").Should().BeEmpty("there should be no leftover process");
        }
    }

    [TearDown]
    public void TearDown()
    {
        foreach (Process process in Process.GetProcessesByName("spkl.CLI.IPC.Test.Singleton"))
        {
            process.Kill();
        }
    }
}
