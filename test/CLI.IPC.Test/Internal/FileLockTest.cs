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
        bool isLocked = this.fileLock.IsLocked();

        // assert
        isLocked.Should().BeFalse();
    }

    [Test]
    public void IsNotHoldingLockInitially()
    {
        // act
        bool isHoldingLock = this.fileLock.IsHoldingLock();

        // assert
        isHoldingLock.Should().BeFalse();
    }

    [Test]
    public void LockLocksTheFile()
    {
        // act
        this.fileLock.Lock();

        // assert
        File.Exists(this.fileLock.Path).Should().BeTrue();
        Invoking(() => FileStreams.OpenForSharedReading(this.fileLock.Path)).Should().Throw<Exception>();
        this.fileLock.IsHoldingLock().Should().BeTrue();
        this.fileLock.IsLocked().Should().BeTrue();
    }

    [Test]
    public void TryLockLocksTheFileIfItIsNotLocked()
    {
        // act
        bool tryLock = this.fileLock.TryLock();

        // assert
        tryLock.Should().BeTrue();
        Invoking(() => FileStreams.OpenForSharedReading(this.fileLock.Path)).Should().Throw<Exception>();
        this.fileLock.IsHoldingLock().Should().BeTrue();
        this.fileLock.IsLocked().Should().BeTrue();
    }

    [Test]
    public void TryLockReturnsFalseIfTheFileIsAlreadyLocked()
    {
        // arrange
        using FileLocker _ = FileLocker.Lock(this.fileLock.Path);

        // act
        bool tryLock = this.fileLock.TryLock();

        // assert
        tryLock.Should().BeFalse();
        this.fileLock.IsHoldingLock().Should().BeFalse();
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
        bool tryLock = this.fileLock.TryLock(TimeSpan.FromSeconds(3));

        // assert
        tryLock.Should().BeTrue();
        Invoking(() => FileStreams.OpenForSharedReading(this.fileLock.Path)).Should().Throw<Exception>();
        this.fileLock.IsHoldingLock().Should().BeTrue();
    }

    [Test]
    public void TryLockWithTimeoutReturnsFalseIfTheLockCannotBeObtained()
    {
        // arrange
        using FileLocker _ = FileLocker.Lock(this.fileLock.Path);

        // act
        bool tryLock = this.fileLock.TryLock(TimeSpan.FromSeconds(1));

        // assert
        tryLock.Should().BeFalse();
        this.fileLock.IsHoldingLock().Should().BeFalse();
    }

    [Test]
    public void IsLockedReturnsTrueIfFileIsLocked()
    {
        // arrange
        using FileLocker _ = FileLocker.Lock(this.fileLock.Path);

        // act
        bool isLocked = this.fileLock.IsLocked();

        // assert
        isLocked.Should().BeTrue();
        this.fileLock.IsHoldingLock().Should().BeFalse();
    }

    [Test]
    public void IsLockedReturnsFalseAfterFileIsUnlocked()
    {
        // arrange
        this.fileLock.Lock();
        this.fileLock.Unlock();

        // act
        bool isLocked = this.fileLock.IsLocked();

        // assert
        isLocked.Should().BeFalse();
        this.fileLock.IsHoldingLock().Should().BeFalse();
    }

    [Test]
    public void IsLockedReturnsFalseIsFileExistsButIsNotLocked()
    {
        // arrange
        File.WriteAllText(this.fileLock.Path, string.Empty);

        // act
        bool isLocked = this.fileLock.IsLocked();

        // assert
        isLocked.Should().BeFalse();
        this.fileLock.IsHoldingLock().Should().BeFalse();
    }

    [Test]
    public void UnlockUnlocksAndDeletesTheFile()
    {
        // arrange
        this.fileLock.Lock();

        // act
        this.fileLock.Unlock();

        // assert
        File.Exists(this.fileLock.Path).Should().BeFalse();
        Invoking(() => FileStreams.OpenForExclusiveWriting(this.fileLock.Path).Dispose()).Should().NotThrow();
        this.fileLock.IsHoldingLock().Should().BeFalse();
    }
}
