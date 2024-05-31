using spkl.CLI.IPC.Internal;
using System;
using System.IO;
using System.Threading;

namespace spkl.CLI.IPC.Test.FileLocker;

internal class Program
{
    static void Main(string[] args)
    {
        string filePath = args[0];
        //FileStream stream = FileStreams.OpenForLocking(filePath);
        Console.Out.WriteLine("locked");
        Console.Out.Flush();
        Thread.Sleep(Timeout.Infinite);
    }
}
