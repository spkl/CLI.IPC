﻿using spkl.IPC.Messaging;
using System;
using System.IO;
using System.Net.Sockets;

namespace spkl.IPC;

public class Host
{
    public string FilePath { get; }

    public IClientConnectionHandler Handler { get; }

    private MessageChannelHost? MessageChannelHost { get; set; }

    private Host(string filePath, IClientConnectionHandler handler)
    {
        this.FilePath = filePath;
        this.Handler = handler;
    }

    public static Host Start(string filePath, IClientConnectionHandler handler)
    {
        File.Delete(filePath);

        Host host = new(filePath, handler);
        host.AcceptConnections();
        return host;
    }

    private void AcceptConnections()
    {
        this.MessageChannelHost = new MessageChannelHost(this.FilePath, this.Handler.TaskFactory, this.HandleNewMessageChannel, this.HandleListenerException);
        this.MessageChannelHost.AcceptConnections();
    }

    private void HandleNewMessageChannel(MessageChannel channel)
    {
        ClientProperties properties;
        try
        {
            properties = this.ReceiveClientProperties(channel);
        }
        catch (SocketException e)
        {
            this.Handler.HandleListenerError(new ListenerError(e, false));
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

    private void HandleListenerException(Exception exception)
    {
        this.Handler.HandleListenerError(new ListenerError(exception, true));
    }

    private ClientProperties ReceiveClientProperties(MessageChannel channel)
    {
        ClientProperties properties = new ClientProperties();

        channel.Sender.SendReqArgs();
        properties.Arguments = channel.Receiver.ReceiveArgs();

        channel.Sender.SendReqCurrentDir();
        properties.CurrentDirectory = channel.Receiver.ReceiveCurrentDir();

        return properties;
    }

    public void Shutdown()
    {
        this.MessageChannelHost?.Shutdown();
        File.Delete(this.FilePath);
    }
}
