using spkl.CLI.IPC.Internal;

namespace spkl.CLI.IPC.Test.Internal;
internal class DisposableTest : TestBase
{
    [Test]
    public void DisposeCallsCallback()
    {
        // arrange
        int callCount = 0;
        Disposable disposable = new(() => callCount++);

        // act
        disposable.Dispose();

        // assert
        callCount.Should().Be(1);
    }

    [Test]
    public void DisposeCallsCallbackOnlyOnce()
    {
        // arrange
        int callCount = 0;
        Disposable disposable = new(() => callCount++);

        // act
        disposable.Dispose();
        disposable.Dispose();
        disposable.Dispose();

        // assert
        callCount.Should().Be(1);
    }
}
