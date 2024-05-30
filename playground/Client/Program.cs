// Copyright (c) Sebastian Fischer. All Rights Reserved.
// Licensed under the MIT License.

using spkl.CLI.IPC;
using spkl.CLI.IPC.Startup;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace Client;

internal class Program
{
    static void Main(string[] args)
    {
        AutoTransportSingletonApp app = new AutoTransportSingletonApp(
            new StartupBehavior(
                @"C:\Users\Sebastian\Documents\Projects\spkl.CLI.IPC\playground\singleton",
                TimeSpan.FromMilliseconds(250),
                TimeSpan.FromSeconds(5),
                () =>
                {
                    string clientDir = Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location)!;
                    string serverDir = clientDir.Replace(Path.Combine("playground", "Client", "bin"), Path.Combine("playground", "Server", "bin"));
                    string serverExe = Path.Combine(serverDir, "Server.exe");

                    ProcessStartInfo psi = new ProcessStartInfo(serverExe);
                    psi.UseShellExecute = true;
                    Process.Start(psi);
                }));

        app.RequestInstance();
        spkl.CLI.IPC.Client.Attach(
            app.Transport,
            new DefaultHostConnectionHandler());
    }
}
