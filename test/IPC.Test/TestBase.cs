using spkl.IPC.Services;

namespace spkl.IPC.Test;

[TestFixture]
internal abstract class TestBase
{
    [SetUp]
    public void TestBaseSetUp()
    {
        ServiceProvider.InitializeDefaults();
    }

    private static int nextUnusedPort = 65070;

    protected static int GetUnusedPort()
    {
        return TestBase.nextUnusedPort++;
    }
}
