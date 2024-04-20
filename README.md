# CLI.IPC

A .NET library that helps you implement a client/server architecture for command line interface applications. Using _spkl.CLI.IPC_, you can let your CLI application delegate its workload to a separate server process.

__How does it work?__
When a user starts your client application, it sends its working directory, command line arguments and process ID to the server application. While processing the request, the server application can send console outputs (standard and error) back to the client. At the end, the server sends an exit code and closes the connection.

_spkl.CLI.IPC_ was designed for clients and servers that run on the same machine, but because it is based on sockets, network communication between different machines can also be used.

## Example Usage

### Server

Start a server by calling `Host.Start` and supplying the following information:
* How will the connection be established (`ITransport`)? Any streaming socket is supported; support for TCP loopback and Unix Domain Sockets* is built-in.
* What will happen when a client connects (`IClientConnectionHandler`)?

\* Unix Domain Sockets are not supported for .NET Standard 2.0 or before Windows 10.

In this example, lets implement a fictional database dump command:
```csharp
static void Main(string[] args)
{
    Host host = Host.Start(new UdsTransport("path/to/socket_file"), new MyClientConnectionHandler());
    // [ wait until it is time to shut down the server ]
    host.Shutdown();
    host.WaitUntilAllClientsDisconnected();
}

private class MyClientConnectionHandler : IClientConnectionHandler
{
    public void HandleCall(ClientConnection connection)
    {
        if (connection.Properties.Arguments is [_, "dump-db", string fileName])
        {
            connection.Out.WriteLine("Dumping database to file...");
            Db.DumpTo(Path.Combine(connection.Properties.CurrentDirectory, fileName));
            connection.Exit(0);
            return;
        }

        connection.Error.WriteLine("Unknown command.");
        connection.Exit(1);
    }

    // [...]
}
```

### Client

Implementing a client can be as easy as this one-liner:
```csharp
Client.Attach(
    new UdsTransport("path/to/socket_file"),
    new DefaultHostConnectionHandler());
```

Just like for the server, you need to specify connection and behavior information (`ITransport`, `IHostConnectionHandler2`).

For most scenarios, the built-in `DefaultHostConnectionHandler` can be used. It writes the received outputs to the console and exits the application with the received exit code when the server closes the connection.

## Other Features

### SingletonApplication: Starting a Server On Demand

A client application obviously can't connect to a server when the server is not running. If the server should be started on demand, i.e. when a client wants to connect, the `SingletonApplication` class can help.

With `SingletonApplication`, you can ensure that only one instance of the server is started, even when multiple clients ask for it simultaneously.

Usage as a client application: Call `RequestInstance` to ensure that an application instance is running before connecting to it.
```csharp
IStartupBehavior startupBehavior = [...];
new SingletonApplication(startupBehavior).RequestInstance();

Client.Attach([...]);
```

Usage as a server application: Call `ReportInstanceRunning` when ready for incoming connections. Call `ShutdownInstance` before no longer accepting connections.
```csharp
IStartupBehavior startupBehavior = [...];
SingletonApplication singletonApplication = new(startupBehavior);

Host host = Host.Start([...]);
singletonApplication.ReportInstanceRunning();
// [...]
singletonApplication.ShutdownInstance();
host.Shutdown();
```

Using the `IStartupBehavior` interface, you can customize the following aspects:
* How a server instance is started. Typically, by starting a new process.
* Which time period is used for polling whether a server is starting or running.
* After what timeout the `RequestInstance` or `ReportInstanceRunning` methods will fail.
* Which file path is used to determine server state (because `SingletonApplication` uses file-based locking). This can be used to provide a server process per machine, per user, or arbitrarily.
