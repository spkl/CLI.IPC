using spkl.CLI.IPC.Internal;
using spkl.CLI.IPC.Startup;
using System;
using System.IO;

namespace spkl.CLI.IPC.Test.Startup;

internal class AutoTransportSingletonAppTest : SingletonAppTestBase
{
    protected AutoTransportSingletonApp singletonApp = null!;

    [SetUp]
    public void SetUp()
    {
        this.singletonApp = new AutoTransportSingletonApp(this.startupBehavior);
    }

    [TearDown]
    public void TearDown()
    {
        this.singletonApp.Dispose();
    }

    [Test]
    public void RequestInstanceReturnsWithDeserializedTransportIfApplicationRunning()
    {
        // arrange
        this.disposables.Add(File.Open(this.negotiationFile + ".run_lock", FileMode.Create));
        this.disposables.Add(File.Open(this.negotiationFile + ".transport_lock", FileMode.Create));
        this.disposables.Add(File.Open(this.negotiationFile + ".transport_ready", FileMode.Create));
        using (FileStream type = File.Open(this.negotiationFile + ".transport_type", FileMode.Create))
        using (FileStream data = File.Open(this.negotiationFile + ".transport_data", FileMode.Create))
        {
            Serializer.Write(new TcpLoopbackTransport(1234), type, data);
        }

        // act
        this.singletonApp.RequestInstance();
        ITransport transport = this.singletonApp.Transport;

        // assert
        transport.Should().BeOfType<TcpLoopbackTransport>();
        transport.As<TcpLoopbackTransport>().Port.Should().Be(1234);
    }

    [Test]
    public void RequestInstanceThrowsExceptionIfTransportDataIsNotReady()
    {
        // arrange
        this.disposables.Add(File.Open(this.negotiationFile + ".run_lock", FileMode.Create));

        // act & assert
        Invoking(() => this.singletonApp.RequestInstance()).Should().Throw<SingletonAppException>();
    }

    [Test]
    public void RequestInstanceThrowsExceptionIfTransportDataIsReadyButMissing()
    {
        // arrange
        this.disposables.Add(File.Open(this.negotiationFile + ".run_lock", FileMode.Create));
        this.disposables.Add(File.Open(this.negotiationFile + ".transport_lock", FileMode.Create));
        this.disposables.Add(File.Open(this.negotiationFile + ".transport_ready", FileMode.Create));

        // act & assert
        Invoking(() => this.singletonApp.RequestInstance()).Should().Throw<SingletonAppException>();
    }

    [Test]
    public void ReportInstanceRunningLocksFiles()
    {
        // act
        this.singletonApp.ReportInstanceRunning();

        // assert
        Action openTransportLockShared = () => this.disposables.Add(File.Open(this.negotiationFile + ".transport_lock", FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite | FileShare.Delete));
        openTransportLockShared.Should().Throw<IOException>();
        Action openTransportReadyShared = () => this.disposables.Add(File.Open(this.negotiationFile + ".transport_ready", FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite | FileShare.Delete));
        openTransportReadyShared.Should().Throw<IOException>();
    }

    [Test]
    public void ReportInstanceRunningSerializesTransport()
    {
        // arrange
        this.singletonApp.Transport = new TcpLoopbackTransport(2345);

        // act
        this.singletonApp.ReportInstanceRunning();

        // assert
        using (FileStream type = File.Open(this.negotiationFile + ".transport_type", FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
        using (FileStream data = File.Open(this.negotiationFile + ".transport_data", FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
        {
            ITransport transport = Serializer.Read<ITransport>(type, data);

            transport.Should().BeOfType<TcpLoopbackTransport>();
            transport.As<TcpLoopbackTransport>().Port.Should().Be(2345);
        }
    }

    [Test]
    public void ReportInstanceRunningThrowsExceptionIfFileIsLocked()
    {
        // arrange
        this.disposables.Add(File.Open(this.negotiationFile + ".transport_lock", FileMode.Create));

        // act & assert
        Invoking(() => this.singletonApp.ReportInstanceRunning()).Should().Throw<SingletonAppException>();
    }

    [Test]
    public void ReportInstanceShuttingDownUnlocksFiles()
    {
        // arrange
        this.singletonApp.ReportInstanceRunning();

        // act
        this.singletonApp.ReportInstanceShuttingDown();

        // assert
        Invoking(() => this.disposables.Add(File.Open(this.negotiationFile + ".transport_type", FileMode.Create))).Should().NotThrow();
        Invoking(() => this.disposables.Add(File.Open(this.negotiationFile + ".transport_data", FileMode.Create))).Should().NotThrow();
        Invoking(() => this.disposables.Add(File.Open(this.negotiationFile + ".transport_ready", FileMode.Create))).Should().NotThrow();
        Invoking(() => this.disposables.Add(File.Open(this.negotiationFile + ".transport_lock", FileMode.Create))).Should().NotThrow();
    }

    [Test]
    public void SuspendStartupLocksFile()
    {
        // act
        this.disposables.Add(
            this.singletonApp.SuspendStartup()
        );

        // assert
        Action openStartLockShared = () => this.disposables.Add(File.Open(this.negotiationFile + ".start_lock", FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite | FileShare.Delete));
        openStartLockShared.Should().Throw<IOException>();
    }

    [Theory]
    public void IsInstanceRunningReflectsFileLockState(bool fileLocked)
    {
        // arrange
        if (fileLocked)
        {
            this.disposables.Add(File.Open(this.negotiationFile + ".run_lock", FileMode.Create));
        }

        // act
        bool isInstanceRunning = this.singletonApp.IsInstanceRunning();

        // assert
        isInstanceRunning.Should().Be(fileLocked);
    }

    [Theory]
    public void IsInstanceStartingReflectsFileLockState(bool fileLocked)
    {
        // arrange
        if (fileLocked)
        {
            this.disposables.Add(File.Open(this.negotiationFile + ".start_lock", FileMode.Create));
        }

        // act
        bool isInstanceStarting = this.singletonApp.IsInstanceStarting();

        // assert
        isInstanceStarting.Should().Be(fileLocked);
    }

#if NET6_0_OR_GREATER
    [Test]
    public void TransportIsUdsIfPathShortEnough()
    {
        // act
        ITransport transport = this.singletonApp.Transport;

        // assert
        transport.Should().BeOfType<UdsTransport>();
    }

    [Test]
    public void TransportIsTcpIfPathTooLong()
    {
        // arrange
        this.singletonApp.Dispose();
        this.startupBehavior.NegotiationFileBasePath.Returns("this/path/is/way/too/long/because/unix/domain/socket/paths/are/limited/to/around/100/characters/depending/on/the/platform");
        this.singletonApp = new AutoTransportSingletonApp(this.startupBehavior);

        // act
        ITransport transport = this.singletonApp.Transport;

        // assert
        transport.Should().BeOfType<TcpLoopbackTransport>();
    }
#else
    [Test]
    public void TransportIsTcp()
    {
        // act
        ITransport transport = this.singletonApp.Transport;

        // assert
        transport.Should().BeOfType<TcpLoopbackTransport>();
    }
#endif

    protected override void CheckForLockedFiles()
    {
        File.Open(this.negotiationFile + ".transport_type", FileMode.Create).Dispose();
        File.Open(this.negotiationFile + ".transport_data", FileMode.Create).Dispose();
        File.Open(this.negotiationFile + ".transport_ready", FileMode.Create).Dispose();
        File.Open(this.negotiationFile + ".transport_lock", FileMode.Create).Dispose();
    }
}
