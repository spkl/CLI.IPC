using spkl.CLI.IPC.Internal;
using System.Collections.Generic;
using System.Text;

namespace spkl.CLI.IPC.Test.Internal;

internal class DelegateTextWriterTest : TestBase
{
    private readonly List<string> writtenStrings = new();

    private DelegateTextWriter writer = null!;

    [SetUp]
    public void SetUp()
    {
        this.writtenStrings.Clear();
        this.writer = new DelegateTextWriter(this.writtenStrings.Add);
    }

    [TearDown]
    public void TearDown()
    {
        this.writer.Dispose();
    }

    [Test]
    public void EncodingIsUtf8()
    {
        // act
        Encoding result = this.writer.Encoding;

        // assert
        Assert.That(result, Is.EqualTo(Encoding.UTF8));
    }

    [Test]
    public void WriteChar()
    {
        // act
        this.writer.Write('x');

        // assert
        Assert.That(this.writtenStrings, Is.EqualTo(new string[] { "x" }));
    }

    [Test]
    public void WriteCharArrayIntInt()
    {
        // act
        this.writer.Write("foobar".ToCharArray(), 1, 4);

        // assert
        Assert.That(this.writtenStrings, Is.EqualTo(new string[] { "ooba" }));
    }

    [Test]
    public void WriteString()
    {
        // act
        this.writer.Write("foobar");

        // assert
        Assert.That(this.writtenStrings, Is.EqualTo(new string[] { "foobar" }));
    }

    [Test]
    public void WriteStringNull()
    {
        // act
        this.writer.Write((string?)null);

        // assert
        Assert.That(this.writtenStrings, Is.Empty);
    }
}
