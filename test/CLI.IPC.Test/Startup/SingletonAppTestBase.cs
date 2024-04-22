using spkl.CLI.IPC.Startup;
using System;
using System.Collections.Generic;
using System.IO;

namespace spkl.CLI.IPC.Test.Startup;

internal abstract class SingletonAppTestBase : TestBase
{
    protected readonly List<IDisposable> disposables = new List<IDisposable>();

    protected string negotiationFile = string.Empty;

    protected IStartupBehavior startupBehavior = null!;

    [SetUp]
    public void SingletonAppTestBaseSetUp()
    {
        this.negotiationFile = Path.Combine(TestContext.CurrentContext.WorkDirectory, "SingletonAppTest");

        this.startupBehavior = Substitute.For<IStartupBehavior>();
        this.startupBehavior.PollingPeriod.Returns(TimeSpan.FromMilliseconds(1));
        this.startupBehavior.TimeoutThreshold.Returns(TimeSpan.FromSeconds(2));
        this.startupBehavior.NegotiationFileBasePath.Returns(this.negotiationFile);
    }

    [TearDown]
    public void SingletonAppTestBaseTearDown()
    {
        foreach (IDisposable disposable in this.disposables)
        {
            disposable.Dispose();
        }

        this.disposables.Clear();

        this.CheckForLockedFiles();
    }

    protected abstract void CheckForLockedFiles();
}
