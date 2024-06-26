﻿using spkl.CLI.IPC.Startup;
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
        Assert.That(() => this.singletonApp.RequestInstance(), Throws.InstanceOf<SingletonAppException>());
    }

    [Test]
    public void ReportInstanceRunningLocksFile()
    {
        // act
        this.singletonApp.ReportInstanceRunning();

        // assert
        Assert.That(
            () => this.disposables.Add(File.Open(this.negotiationFile + ".run_lock", FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite | FileShare.Delete)),
            Throws.InstanceOf<IOException>());
    }

    [Test]
    public void ReportInstanceRunningThrowsExceptionIfFileLockCannotBeObtained()
    {
        // arrange
        this.disposables.Add(File.Open(this.negotiationFile + ".run_lock", FileMode.Create));

        // act & assert
        Assert.That(() => this.singletonApp.ReportInstanceRunning(), Throws.InstanceOf<SingletonAppException>());
    }

    [Test]
    public void ReportInstanceShuttingDownThrowsExceptionIfReportInstanceRunningWasNotCalled()
    {
        // act & assert
        Assert.That(() => this.singletonApp.ReportInstanceShuttingDown(), Throws.InstanceOf<InvalidOperationException>());
    }

    [Test]
    public void ReportInstanceShuttingDownUnlocksFile()
    {
        // arrange
        this.singletonApp.ReportInstanceRunning();

        // act
        this.singletonApp.ReportInstanceShuttingDown();

        // assert
        Assert.That(() => this.disposables.Add(File.Open(this.negotiationFile + ".run_lock", FileMode.Create)), Throws.Nothing);
    }

    [Test]
    public void SuspendStartupLocksFile()
    {
        // act
        this.disposables.Add(
            this.singletonApp.SuspendStartup()
        );

        // assert
        Assert.That(
            () => this.disposables.Add(File.Open(this.negotiationFile + ".start_lock", FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite | FileShare.Delete)),
            Throws.InstanceOf<IOException>());
    }

    [Test]
    public void SuspendStartupTwiceThrowsException()
    {
        // arrange
        this.disposables.Add(this.singletonApp.SuspendStartup());

        // act & assert
        Assert.That(() => this.disposables.Add(this.singletonApp.SuspendStartup()), Throws.InstanceOf<SingletonAppException>());
    }

    [Test]
    public void SuspendStartupDisposeUnlocksFile()
    {
        // arrange
        IDisposable disposable = this.singletonApp.SuspendStartup();

        // act
        disposable.Dispose();

        // assert
        Assert.That(() => this.disposables.Add(File.Open(this.negotiationFile + ".start_lock", FileMode.Create)), Throws.Nothing);
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
        bool result = this.singletonApp.IsInstanceRunning();

        // assert
        Assert.That(result, Is.EqualTo(fileLocked));
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
        bool result = this.singletonApp.IsInstanceStarting();

        // assert
        Assert.That(result, Is.EqualTo(fileLocked));
    }
}
