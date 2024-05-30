using spkl.CLI.IPC.Internal;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace spkl.CLI.IPC.Test.Internal;

#if !NET6_0_OR_GREATER
[Platform(Exclude = "Linux")]
#endif
internal class FileLockTest : TestBase
{
    private FileLock fileLock = null!;

    [SetUp]
    public void SetUp()
    {
        this.fileLock = new FileLock(Path.Combine(TestContext.CurrentContext.WorkDirectory, "FileLockTest"));
    }

    [TearDown]
    public void TearDown()
    {
        this.fileLock.Unlock();
    }

    [Test]
    public void IsNotLockedInitially()
    {
        // act
        bool result = this.fileLock.IsLocked();

        // assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void IsNotHoldingLockInitially()
    {
        // act
        bool result = this.fileLock.IsHoldingLock();

        // assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void LockLocksTheFile()
    {
        // act
        this.fileLock.Lock();

        // assert
        Assert.That(this.fileLock.Path, Does.Exist);
        Assert.That(() => FileStreams.OpenForSharedReading(this.fileLock.Path), Throws.Exception);
        Assert.That(this.fileLock.IsHoldingLock(), Is.True);
        Assert.That(this.fileLock.IsLocked(), Is.True);
    }

    [Test]
    public void TryLockLocksTheFileIfItIsNotLocked()
    {
        // act
        bool result = this.fileLock.TryLock();

        // assert
        Assert.That(result, Is.True);
        Assert.That(() => FileStreams.OpenForSharedReading(this.fileLock.Path), Throws.Exception);
        Assert.That(this.fileLock.IsHoldingLock(), Is.True);
        Assert.That(this.fileLock.IsLocked(), Is.True);
    }

    [Test]
    public void TryLockReturnsFalseIfTheFileIsAlreadyLocked()
    {
        // arrange
        using FileLocker _ = FileLocker.Lock(this.fileLock.Path);

        // act
        bool result = this.fileLock.TryLock();

        // assert
        Assert.That(result, Is.False);
        Assert.That(this.fileLock.IsHoldingLock(), Is.False);
    }

    [Test]
    public void TryLockWithTimeoutLocksTheFileAsSoonAsItIsNotLocked()
    {
        // arrange
        using FileLocker locker = FileLocker.Lock(this.fileLock.Path);
        Task.Run(() =>
        {
            Thread.Sleep(TimeSpan.FromSeconds(1));
            locker.Dispose();
        });

        // act
        bool result = this.fileLock.TryLock(TimeSpan.FromSeconds(3));

        // assert
        Assert.That(result, Is.True);
        Assert.That(() => FileStreams.OpenForSharedReading(this.fileLock.Path), Throws.Exception);
        Assert.That(this.fileLock.IsHoldingLock(), Is.True);
    }

    [Test]
    public void TryLockWithTimeoutReturnsFalseIfTheLockCannotBeObtained()
    {
        // arrange
        using FileLocker _ = FileLocker.Lock(this.fileLock.Path);

        // act
        bool result = this.fileLock.TryLock(TimeSpan.FromSeconds(1));

        // assert
        Assert.That(result, Is.False);
        Assert.That(this.fileLock.IsHoldingLock(), Is.False);
    }

    [Test]
    public void IsLockedReturnsTrueIfFileIsLocked()
    {
        // arrange
        using FileLocker _ = FileLocker.Lock(this.fileLock.Path);

        // act
        bool result = this.fileLock.IsLocked();

        // assert
        Assert.That(result, Is.True);
        Assert.That(this.fileLock.IsHoldingLock(), Is.False);
    }

    [Test]
    public void IsLockedReturnsFalseAfterFileIsUnlocked()
    {
        // arrange
        this.fileLock.Lock();
        this.fileLock.Unlock();

        // act
        bool result = this.fileLock.IsLocked();

        // assert
        Assert.That(result, Is.False);
        Assert.That(this.fileLock.IsHoldingLock(), Is.False);
    }

    [Test]
    public void IsLockedReturnsFalseIsFileExistsButIsNotLocked()
    {
        // arrange
        File.WriteAllText(this.fileLock.Path, string.Empty);

        // act
        bool result = this.fileLock.IsLocked();

        // assert
        Assert.That(result, Is.False);
        Assert.That(this.fileLock.IsHoldingLock(), Is.False);
    }

    [Test]
    public void UnlockUnlocksAndDeletesTheFile()
    {
        // arrange
        this.fileLock.Lock();

        // act
        this.fileLock.Unlock();

        // assert
        Assert.That(this.fileLock.Path, Does.Not.Exist);
        Assert.That(() => FileStreams.OpenForExclusiveWriting(this.fileLock.Path).Dispose(), Throws.Nothing);
        Assert.That(this.fileLock.IsHoldingLock(), Is.False);
    }
}
