using spkl.IPC.Startup;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace spkl.IPC.Test.Startup;

internal class SingletonApplicationTest : TestBase
{
    private readonly List<IDisposable> disposables = new List<IDisposable>();

    private string negotiationFile = string.Empty;

    private IStartupBehavior? startupBehavior;

    private SingletonApplication? singletonApplication;

    [SetUp]
    public void SetUp()
    {
        this.negotiationFile = Path.Combine(TestContext.CurrentContext.WorkDirectory, "SingletonApplicationTest");

        this.startupBehavior = Substitute.For<IStartupBehavior>();
        this.startupBehavior.PollingPeriod.Returns(TimeSpan.FromMilliseconds(1));
        this.startupBehavior.TimeoutThreshold.Returns(TimeSpan.FromSeconds(2));
        this.startupBehavior.NegotiationFileBasePath.Returns(this.negotiationFile);

        this.singletonApplication = new SingletonApplication(this.startupBehavior);
    }

    [TearDown]
    public void TearDown()
    {
        this.singletonApplication?.Dispose();

        foreach (IDisposable disposable in this.disposables)
        {
            disposable.Dispose();
        }

        this.disposables.Clear();
    }

    [Test]
    public void RequestInstanceReturnsWithoutActionIfApplicationRunning()
    {
        // arrange
        this.disposables.Add(File.Open(this.negotiationFile + ".lock1", FileMode.Create));

        // act
        this.singletonApplication!.RequestInstance();

        // assert
        this.startupBehavior!.DidNotReceive().StartInstance();
    }

    [Test]
    public void RequestInstanceReturnsWithoutActionIfApplicationStartingThenRunning()
    {
        // arrange
        this.disposables.Add(File.Open(this.negotiationFile + ".lock0", FileMode.Create));
        new Thread(
            () =>
            {
                Thread.Sleep(100);
                this.disposables.Add(File.Open(this.negotiationFile + ".lock1", FileMode.Create));
            }).Start();

        // act
        this.singletonApplication!.RequestInstance();

        // assert
        this.startupBehavior!.DidNotReceive().StartInstance();
    }

    [Test]
    public void RequestInstanceCallsStartInstanceIfNoApplicationRunningOrStarting()
    {
        // arrange
        this.startupBehavior!
            .When(o => o.StartInstance())
            .Do(_ => this.disposables.Add(File.Open(this.negotiationFile + ".lock1", FileMode.Create)));

        // act
        this.singletonApplication!.RequestInstance();

        // assert
        this.startupBehavior!.Received().StartInstance();
    }

    [Test]
    public void RequestInstanceThrowsExceptionIfNoApplicationIsStartedBeforeTimeout()
    {
        // act & assert
        Assert.That(() => this.singletonApplication!.RequestInstance(), Throws.InstanceOf<SingletonApplicationException>());
    }

    [Test]
    public void ReportInstanceRunningLocksFile()
    {
        // act
        this.singletonApplication!.ReportInstanceRunning();

        // assert
        Assert.That(
            () => this.disposables.Add(File.Open(this.negotiationFile + ".lock1", FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite | FileShare.Delete)),
            Throws.InstanceOf<IOException>());
    }

    [Test]
    public void ReportInstanceRunningThrowsExceptionIfFileLockCannotBeObtained()
    {
        // arrange
        this.disposables.Add(File.Open(this.negotiationFile + ".lock1", FileMode.Create));

        // act & assert
        Assert.That(() => this.singletonApplication!.ReportInstanceRunning(), Throws.InstanceOf<SingletonApplicationException>());
    }

    [Test]
    public void ShutdownInstanceThrowsExceptionIfReportInstanceRunningWasNotCalled()
    {
        // act & assert
        Assert.That(() => this.singletonApplication!.ShutdownInstance(), Throws.InstanceOf<InvalidOperationException>());
    }

    [Test]
    public void ShutdownInstanceUnlocksFile()
    {
        // arrange
        this.singletonApplication!.ReportInstanceRunning();

        // act
        this.singletonApplication!.ShutdownInstance();

        // assert
        Assert.That(() => this.disposables.Add(File.Open(this.negotiationFile + ".lock1", FileMode.Create)), Throws.Nothing);
    }
}
