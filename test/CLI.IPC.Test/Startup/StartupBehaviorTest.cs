using spkl.CLI.IPC.Startup;
using System;

namespace spkl.CLI.IPC.Test.Startup;
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
        startupBehavior.NegotiationFileBasePath.Should().Be(arg1);
        startupBehavior.PollingPeriod.Should().Be(arg2);
        startupBehavior.TimeoutThreshold.Should().Be(arg3);
        calls.Should().Be(1);
    }
}
