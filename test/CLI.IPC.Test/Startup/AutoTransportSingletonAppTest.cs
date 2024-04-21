using spkl.CLI.IPC.Startup;
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

#if NET6_0_OR_GREATER
    [Test]
    public void TransportIsUdsIfPathShortEnough()
    {
        // act
        ITransport transport = this.singletonApp.Transport;

        // assert
        Assert.That(transport, Is.InstanceOf<UdsTransport>());
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
        Assert.That(transport, Is.InstanceOf<TcpLoopbackTransport>());
    }
#else
    [Test]
    public void TransportIsTcp()
    {
        // act
        ITransport transport = this.singletonApp.Transport;

        // assert
        Assert.That(transport, Is.InstanceOf<TcpLoopbackTransport>());
    }
#endif

    protected override void CheckForLockedFiles()
    {
        File.Open(this.negotiationFile + ".transport_type", FileMode.Create).Dispose();
        File.Open(this.negotiationFile + ".transport_data", FileMode.Create).Dispose();
        File.Open(this.negotiationFile + ".transport_ready", FileMode.Create).Dispose();
    }
}
