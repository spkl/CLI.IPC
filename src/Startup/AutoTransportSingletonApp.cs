// Copyright (c) Sebastian Fischer. All Rights Reserved.
// Licensed under the MIT License.

using spkl.CLI.IPC.Internal;
using System;
using System.IO;

namespace spkl.CLI.IPC.Startup;

/// <summary>
/// Ensures that an application is only started once while automatically managing the <see cref="ITransport"/> used between Host and Client.
/// Usage as a client application: Call <see cref="RequestInstance"/> to ensure that an application instance is running before connecting to it using the provided <see cref="Transport"/>.
/// Usage as the hosting application: Start the host using the provided <see cref="Transport"/>, then call <see cref="ReportInstanceRunning"/> when ready for incoming connections. Call <see cref="ShutdownInstance"/> before exit
/// </summary>
/// <remarks>
/// When running as a host on .NET 6 (or higher), a Unix Domain Socket transport is used by default,
/// but only if the resulting socket path (dependent on <see cref="IStartupBehavior.NegotiationFileBasePath"/>) is not too long,
/// and the OS supports Unix Domain Sockets.
/// In all other cases, a TCP loopback transport on an automatically determined free port is used.
/// To override the default behavior, manually set the <see cref="Transport"/> property before starting the host.
/// </remarks>
public sealed class AutoTransportSingletonApp : IDisposable, IAutoTransportSingletonApp
{
    private readonly IStartupBehavior behavior;

    private readonly SingletonApp innerSingleton;

    private readonly FileLock transportLock;

    private readonly FileLock transportReadyLock;

    private string TransportTypePath => this.behavior.NegotiationFileBasePath + ".transport_type";

    private string TransportDataPath => this.behavior.NegotiationFileBasePath + ".transport_data";

    private FileStream? transportTypeStream;

    private FileStream? transportDataStream;

    /// <inheritdoc/>
    public ITransport Transport { get; set; }

    /// <summary>
    /// </summary>
    public AutoTransportSingletonApp(IStartupBehavior behavior)
    {
        this.behavior = behavior;
        this.innerSingleton = new SingletonApp(behavior);

        this.transportLock = new FileLock(this.behavior.NegotiationFileBasePath + ".transport_lock");
        this.transportReadyLock = new FileLock(this.behavior.NegotiationFileBasePath + ".transport_ready");

        this.Transport = this.CreateTransportForHost();
    }

    private ITransport CreateTransportForHost()
    {
#if NET6_0_OR_GREATER
        UdsTransport udsTransport = new UdsTransport(this.behavior.NegotiationFileBasePath + ".sock");
        try
        {
            _ = udsTransport.EndPoint;
            return udsTransport;
        }
        catch (Exception e) when (e is ArgumentOutOfRangeException or PlatformNotSupportedException)
        {
            return new TcpLoopbackTransport(0);
        }
#else
        return new TcpLoopbackTransport(0);
#endif
    }

    /// <inheritdoc/>
    public void ReportInstanceRunning()
    {
        this.innerSingleton.ReportInstanceRunning();

        try
        {
            this.transportLock.Lock();

            this.transportTypeStream = FileStreams.OpenForExclusiveWriting(this.TransportTypePath);
            this.transportDataStream = FileStreams.OpenForExclusiveWriting(this.TransportDataPath);
            Serializer.Write(this.Transport, this.transportTypeStream, this.transportDataStream);

            this.transportReadyLock.Lock();
        }
        catch (IOException e)
        {
            HandleException(e);
        }
        catch (UnauthorizedAccessException e)
        {
            HandleException(e);
        }

        void HandleException(Exception e)
        {
            this.CloseAllFiles();
            throw new SingletonAppException($"Could not write transport information: {e.Message}");
        }
    }

    /// <inheritdoc/>
    public void RequestInstance()
    {
        this.innerSingleton.RequestInstance();

        bool ready = false;
        TimeSpan timeout = TimeSpan.FromSeconds(5);
        Try.UntilTimedOut(timeout, () =>
        {
            ready = this.transportLock.IsLocked() && this.transportReadyLock.IsLocked();
            return ready;
        });

        if (!ready)
        {
            throw new SingletonAppException($"Timed out: Transport information did not become available within {timeout}.");
        }

        try
        {
            this.transportTypeStream = FileStreams.OpenForSharedReading(this.TransportTypePath);
            this.transportDataStream = FileStreams.OpenForSharedReading(this.TransportDataPath);
            this.Transport = Serializer.Read<ITransport>(this.transportTypeStream, this.transportDataStream);
        }
        catch (Exception e)
        {
            throw new SingletonAppException($"Could not obtain valid transport information: {e.Message}");
        }
        finally
        {
            this.CloseAllFiles();
        }
    }

    /// <inheritdoc/>
    public void ShutdownInstance()
    {
        this.innerSingleton.ShutdownInstance();
        this.CloseAllFiles();
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        this.CloseAllFiles();
        this.innerSingleton.Dispose();
    }

    private void CloseAllFiles()
    {
        this.transportLock.Unlock();
        Try.Dispose(ref this.transportTypeStream);
        Try.Dispose(ref this.transportDataStream);
        this.transportReadyLock.Unlock();
    }
}
