// Copyright (c) Sebastian Fischer. All Rights Reserved.
// Licensed under the MIT License.

using spkl.IPC;

namespace Client;

internal class Program
{
    static void Main(string[] args)
    {
        ITransport transport;
#if NET6_0_OR_GREATER
        transport = new UdsTransport(@"C:\Users\Sebastian\Documents\Projects\StreamTest\playground\Server\bin\Debug\net6.0\socket");
#else
        transport = new TcpLoopbackTransport(65058);
#endif

        spkl.IPC.Client.Attach(
            transport,
            new DefaultHostConnectionHandler());
    }
}