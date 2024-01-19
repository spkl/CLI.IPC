using spkl.IPC.Internal;
using System.Text;

namespace spkl.IPC.Test.Internal;
internal class DelegateTextWriterTest : TestBase
{
    [Test]
    public void EncodingIsUtf8()
    {
        // arrange
        DelegateTextWriter writer = new(s => { });

        // act
        Encoding result = writer.Encoding;

        // assert
        Assert.That(result, Is.EqualTo(Encoding.UTF8));
    }
}
