using spkl.CLI.IPC.Startup;
using System;
using System.IO;

namespace spkl.CLI.IPC.Test.Startup;

internal class SingletonAppTest : SingletonAppTestBase
{
    protected SingletonApp singletonApp = null!;

    [SetUp]
    public void SetUp()
    {
        this.singletonApp = new SingletonApp(this.startupBehavior);
    }

    [TearDown]
    public void TearDown()
    {
        this.singletonApp.Dispose();
    }

    protected override void CheckForLockedFiles()
    {
        File.Open(this.negotiationFile + ".start_lock", FileMode.Create).Dispose();
        File.Open(this.negotiationFile + ".run_lock", FileMode.Create).Dispose();
    }

    [Test]
    public void RequestInstanceReturnsWithoutActionIfApplicationRunning()
    {
        // arrange
        this.disposables.Add(File.Open(this.negotiationFile + ".run_lock", FileMode.Create));

        // act
        this.singletonApp.RequestInstance();

        // assert
        this.startupBehavior.DidNotReceive().StartInstance();
    }

    [Test]
    public void RequestInstanceReturnsWithoutActionIfApplicationStartingThenRunning()
    {
        // arrange
        this.disposables.Add(File.Open(this.negotiationFile + ".start_lock", FileMode.Create));
        this.singletonApp.BeforeRequestingInstance += (_, _) => this.disposables.Add(File.Open(this.negotiationFile + ".run_lock", FileMode.Create));

        // act
        this.singletonApp.RequestInstance();

        // assert
        this.startupBehavior.DidNotReceive().StartInstance();
    }

    [Test]
    public void RequestInstanceCallsStartInstanceIfNoApplicationRunningOrStarting()
    {
        // arrange
        this.startupBehavior
            .When(o => o.StartInstance())
            .Do(_ => this.disposables.Add(File.Open(this.negotiationFile + ".run_lock", FileMode.Create)));

        // act
        this.singletonApp.RequestInstance();

        // assert
        this.startupBehavior.Received().StartInstance();
    }

    [Test]
    public void RequestInstanceThrowsExceptionIfNoApplicationIsStartedBeforeTimeout()
    {
        // act & assert
        Invoking(() => this.singletonApp.RequestInstance()).Should().Throw<SingletonAppException>();
    }

    [Test]
    public void ReportInstanceRunningLocksFile()
    {
        // act
        this.singletonApp.ReportInstanceRunning();

        // assert
        Action openRunLockShared = () => this.disposables.Add(File.Open(this.negotiationFile + ".run_lock", FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite | FileShare.Delete));
        openRunLockShared.Should().Throw<IOException>();
    }

    [Test]
    public void ReportInstanceRunningThrowsExceptionIfFileLockCannotBeObtained()
    {
        // arrange
        this.disposables.Add(File.Open(this.negotiationFile + ".run_lock", FileMode.Create));

        // act & assert
        Invoking(() => this.singletonApp.ReportInstanceRunning()).Should().Throw<SingletonAppException>();
    }

    [Test]
    public void ReportInstanceShuttingDownThrowsExceptionIfReportInstanceRunningWasNotCalled()
    {
        // act & assert
        Invoking(() => this.singletonApp.ReportInstanceShuttingDown()).Should().Throw<InvalidOperationException>();
    }

    [Test]
    public void ReportInstanceShuttingDownUnlocksFile()
    {
        // arrange
        this.singletonApp.ReportInstanceRunning();

        // act
        this.singletonApp.ReportInstanceShuttingDown();

        // assert
        Invoking(() => this.disposables.Add(File.Open(this.negotiationFile + ".run_lock", FileMode.Create))).Should().NotThrow();
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

    [Test]
    public void SuspendStartupTwiceThrowsException()
    {
        // arrange
        this.disposables.Add(this.singletonApp.SuspendStartup());

        // act & assert
        Invoking(() => this.disposables.Add(this.singletonApp.SuspendStartup())).Should().Throw<SingletonAppException>();
    }

    [Test]
    public void SuspendStartupDisposeUnlocksFile()
    {
        // arrange
        IDisposable disposable = this.singletonApp.SuspendStartup();

        // act
        disposable.Dispose();

        // assert
        Invoking(() => this.disposables.Add(File.Open(this.negotiationFile + ".start_lock", FileMode.Create))).Should().NotThrow();
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
}
