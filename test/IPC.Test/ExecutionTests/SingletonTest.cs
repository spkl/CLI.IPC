using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

namespace spkl.IPC.Test.ExecutionTests;
internal class SingletonTest : TestBase
{
    /// <summary>
    /// Approach: Host lifetime is 5 seconds, so if we continuously create new clients for 7 seconds, there should be exactly two host processes.
    /// </summary>
    [Test]
    public void TestSingleton()
    {
        // arrange
        string singletonExe = Path.GetFullPath(Path.Combine(TestContext.CurrentContext.TestDirectory.Replace("IPC.Test", "IPC.Test.Singleton"), "spkl.IPC.Test.Singleton.exe"));
#if NET6_0_OR_GREATER
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            singletonExe = singletonExe.Substring(0, singletonExe.Length - ".exe".Length);
        }
#endif

        List<Thread> threads = new List<Thread>();
        ConcurrentBag<int> exitCodes = new ConcurrentBag<int>();
        ConcurrentBag<string> stdOutputs = new ConcurrentBag<string>();
        ConcurrentBag<string> errOutputs = new ConcurrentBag<string>();
        TimeSpan testDuration = TimeSpan.FromSeconds(7);
        DateTime testStart = DateTime.Now;
        int startedProcesses = 0;

        // act
        while ((DateTime.Now - testStart) < testDuration)
        {
            Thread t = new Thread(() =>
            {
                ProcessStartInfo psi = new ProcessStartInfo(singletonExe);
                psi.UseShellExecute = false;
                psi.RedirectStandardError = true;
                psi.RedirectStandardOutput = true;

                Process p = new();
                p.StartInfo = psi;
                p.Start();
                Interlocked.Increment(ref startedProcesses);

                stdOutputs.Add(p.StandardOutput.ReadToEnd());
                errOutputs.Add(p.StandardError.ReadToEnd());
                p.WaitForExit();
                exitCodes.Add(p.ExitCode);
            });
            t.Start();
            threads.Add(t);
                        
            Thread.Sleep(50);
        }

        foreach (Thread t in threads) 
        {
            t.Join();
        }

        // assert
        TestContext.Out.WriteLine($"{startedProcesses} processes were started.");
        Assert.Multiple(() =>
        {
            Assert.That(exitCodes.Count, Is.EqualTo(startedProcesses), "Number of exit codes");
            Assert.That(stdOutputs.Count, Is.EqualTo(startedProcesses), "Number of std outputs");
            Assert.That(errOutputs.Count, Is.EqualTo(startedProcesses), "Number of err outputs");
            Assert.That(exitCodes, Has.All.EqualTo(0), "Exit codes");
            Assert.That(stdOutputs.Distinct().ToArray(), Has.Exactly(2).Items.And.All.StartsWith("PID "), "Std outputs");
            Assert.That(errOutputs, Has.All.Empty, "Err outputs");
        });
    }
}
