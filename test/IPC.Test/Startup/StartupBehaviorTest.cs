using spkl.IPC.Startup;
using System;

namespace spkl.IPC.Test.Startup;
internal class StartupBehaviorTest : TestBase
{
    [Test]
    public void Ctor()
    {
        // arrange
        string arg1 = "Foo";
        TimeSpan arg2 = TimeSpan.FromSeconds(1);
        TimeSpan arg3 = TimeSpan.FromSeconds(2);
        int calls = 0;
        Action arg4 = () => { calls++; };

        // act
        StartupBehavior startupBehavior = new StartupBehavior(arg1, arg2, arg3, arg4);
        startupBehavior.StartInstance();

        // assert
        Assert.That(startupBehavior.NegotiationFileBasePath, Is.EqualTo(arg1));
        Assert.That(startupBehavior.PollingPeriod, Is.EqualTo(arg2));
        Assert.That(startupBehavior.TimeoutThreshold, Is.EqualTo(arg3));
        Assert.That(calls, Is.EqualTo(1));
    }
}
