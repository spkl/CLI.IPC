using spkl.CLI.IPC.Internal;

namespace spkl.CLI.IPC.Test.Internal;
internal class DisposableTest : TestBase
{
    [Test]
    public void DisposeCallsCallback()
    {
        // arrange
        int i = 0;
        Disposable disposable = new(() => i++);

        // act
        disposable.Dispose();

        // assert
        Assert.That(i, Is.EqualTo(1));
    }

    [Test]
    public void DisposeCallsCallbackOnlyOnce()
    {
        // arrange
        int i = 0;
        Disposable disposable = new(() => i++);

        // act
        disposable.Dispose();
        disposable.Dispose();
        disposable.Dispose();

        // assert
        Assert.That(i, Is.EqualTo(1));
    }
}
