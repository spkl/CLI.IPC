﻿// Copyright (c) Sebastian Fischer. All Rights Reserved.
// Licensed under the MIT License.

using spkl.CLI.IPC.Services;
using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace spkl.CLI.IPC.Messaging;

/// <summary>
/// Implements client connection handling on a host.
/// </summary>
public class MessageChannelHost
{
#if !NET6_0_OR_GREATER
    /// <summary>
    /// Controls how large the socket backlog for connection requests is.
    /// </summary>
    public static int SocketBacklog = 5;
#endif

    internal Socket Socket { get; }

    private readonly TaskFactory taskFactory;

    private readonly Action<MessageChannel> handleNewConnection;

    private readonly Action<Exception, ListenerErrorPoint> handleListenerException;

    private Thread? listenerThread;

    internal MessageChannelHost(ITransport transport, TaskFactory taskFactory, Action<MessageChannel> handleNewConnection, Action<Exception, ListenerErrorPoint> handleListenerException)
    {
        this.Socket = transport.Socket;
        this.Socket.Bind(transport.EndPoint);
        transport.AfterHostBind(this.Socket.LocalEndPoint!);

        this.taskFactory = taskFactory;
        this.handleNewConnection = handleNewConnection;
        this.handleListenerException = handleListenerException;
    }

    internal void AcceptConnections()
    {
#if NET6_0_OR_GREATER
        this.Socket.Listen();
#else
        this.Socket.Listen(MessageChannelHost.SocketBacklog);
#endif

        using EventWaitHandle waitHandle = new EventWaitHandle(false, EventResetMode.ManualReset);

        this.listenerThread = new Thread(this.Accept);
        this.listenerThread.Name = $"{nameof(MessageChannelHost)} listener for {this.Socket.LocalEndPoint}";
        this.listenerThread.Start(waitHandle);

        waitHandle.WaitOne();
    }

    internal void Shutdown()
    {
        this.Socket.Close();
    }

    private void Accept(object? waitHandle)
    {
        ((EventWaitHandle)waitHandle!).Set();
        try
        {
            while (true)
            {
                Socket incoming = this.Socket.Accept();
                MessageChannel messageChannel = ServiceProvider.MessageChannelFactory.CreateForIncoming(incoming);
                this.taskFactory.StartNew(() =>
                {
                    try
                    {
                        this.handleNewConnection(messageChannel);
                    }
                    catch (Exception e)
                    {
                        this.handleListenerException(e, ListenerErrorPoint.ClientConnectionHandler);
                    }
                });
            }
        }
        catch (SocketException e) when (e.SocketErrorCode == SocketError.Interrupted)
        {
            // This host was shut down - allow thread to end.
        }
        catch (Exception e)
        {
            this.handleListenerException(e, ListenerErrorPoint.ConnectionAccept);
        }
        finally
        {
            this.Socket.Close();
        }
    }
}
