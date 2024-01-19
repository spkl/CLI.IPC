namespace spkl.IPC.Test.ExecutionTests;

#if !NET6_0_OR_GREATER
[Platform(Exclude = "Linux")]
#endif
internal abstract class ExecutionTest : TestBase
{
}
