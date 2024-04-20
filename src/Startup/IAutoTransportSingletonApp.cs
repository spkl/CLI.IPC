// Copyright (c) Sebastian Fischer. All Rights Reserved.
// Licensed under the MIT License.

namespace spkl.CLI.IPC.Startup;

/// <inheritdoc cref="AutoTransportSingletonApp"/>
public interface IAutoTransportSingletonApp : ISingletonApp
{
    /// <summary>
    /// Gets or sets the transport to use to create a host or connect to it.
    /// Usage for hosts: Pass the value of this property to <see cref="Host.Start(ITransport, IClientConnectionHandler)"/>.
    /// Usage for clients: After calling <see cref="ISingletonApp.RequestInstance"/>, pass the value of this property to <see cref="Client.Attach(ITransport, IHostConnectionHandler)"/>;
    /// </summary>
    ITransport Transport { get; set; }
}
