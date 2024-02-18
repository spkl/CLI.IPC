// Copyright (c) Sebastian Fischer. All Rights Reserved.
// Licensed under the MIT License.

using spkl.CLI.IPC.Messaging;
using System;
using System.Net.Sockets;
using System.Threading;

namespace spkl.CLI.IPC;

/// <summary>
/// The host (or server).
/// </summary>
public class Host
{
    internal ITransport Transport { get; }

    internal IClientConnectionHandler Handler { get; }

    private MessageChannelHost? MessageChannelHost { get; set; }

    private int connectedClients;

    /// <summary>
    /// Gets the number of currently connected clients.
    /// </summary>
    public int ConnectedClients => Interlocked.CompareExchange(ref this.connectedClients, 0, 0);

    private CountdownEvent clientCountdown = new CountdownEvent(1);

    private Host(ITransport transport, IClientConnectionHandler handler)
    {
        this.Transport = transport;
        this.Handler = handler;
    }

    /// <summary>
    /// Starts a new host using the specified <paramref name="transport"/> and <paramref name="handler"/>.
    /// This method does not block - client connections are accepted on a separate thread.
    /// </summary>
    /// <exception cref="SocketException">
    /// Binding or listening on the socket failed.
    /// Socket-related exceptions that occur during connection handling are not exposed through this method, but through <see cref="IClientConnectionHandler.HandleListenerError(ListenerError)"/>.
    /// </exception>
    public static Host Start(ITransport transport, IClientConnectionHandler handler)
    {
        transport.BeforeHostStart();

        Host host = new(transport, handler);
        host.AcceptConnections();
        return host;
    }

    private void AcceptConnections()
    {
        this.MessageChannelHost = new MessageChannelHost(this.Transport, this.Handler.TaskFactory, this.HandleNewMessageChannel, this.HandleListenerException);
        this.MessageChannelHost.AcceptConnections();
    }

    private void HandleNewMessageChannel(MessageChannel channel)
    {
        Interlocked.Increment(ref this.connectedClients);
        this.clientCountdown.AddCount();

        try
        {
            ClientProperties properties;
            try
            {
                properties = this.ReceiveClientProperties(channel);
            }
            catch (SocketException e)
            {
                this.HandleListenerException(e, ListenerErrorPoint.ReceiveClientProperties);
                return;
            }

            try
            {
                this.Handler.HandleCall(new ClientConnection(properties, channel));
            }
            catch
            {
                channel.Close();
                throw;
            }
        }
        finally
        {
            Interlocked.Decrement(ref this.connectedClients);
            this.clientCountdown.Signal();
        }
        
    }

    private void HandleListenerException(Exception exception, ListenerErrorPoint errorPoint)
    {
        this.Handler.HandleListenerError(new ListenerError(exception, errorPoint));
    }

    private ClientProperties ReceiveClientProperties(MessageChannel channel)
    {
        ClientProperties properties = new();

        channel.Sender.SendReqArgs();
        properties.Arguments = channel.Receiver.ReceiveArgs();

        channel.Sender.SendReqCurrentDir();
        properties.CurrentDirectory = channel.Receiver.ReceiveCurrentDir();

        channel.Sender.SendReqProcessID();
        properties.ProcessID = channel.Receiver.ReceiveProcessID();

        return properties;
    }

    /// <summary>
    /// Stops listening for incoming connections.
    /// </summary>
    public void Shutdown()
    {
        if (this.MessageChannelHost != null)
        {
            this.MessageChannelHost?.Shutdown();
            this.Transport.AfterHostShutdown();

            this.MessageChannelHost = null;
        }
    }

    /// <summary>
    /// Returns only when no client is connected anymore.
    /// </summary>
    public void WaitUntilAllClientsDisconnected()
    {
        this.clientCountdown.Signal();
        this.clientCountdown.Wait();
        this.clientCountdown = new CountdownEvent(1);
    }
}
