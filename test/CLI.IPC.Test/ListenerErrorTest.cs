using System;

namespace spkl.CLI.IPC.Test;
internal class ListenerErrorTest : TestBase
{
    [Test]
    public void Exception()
    {
        // arrange
        Exception exception = new("Foo");
        ListenerError error = new(exception, default);

        // act
        Exception result = error.Exception;

        // assert
        result.Should().BeSameAs(exception);
    }

    [Theory]
    public void ErrorPoint(ListenerErrorPoint value)
    {
        // arrange
        ListenerError error = new(new Exception(), value);

        // act
        ListenerErrorPoint result = error.ErrorPoint;

        // assert
        result.Should().Be(value);
    }

    [Test]
    [TestCase(ListenerErrorPoint.ConnectionAccept, true)]
    [TestCase(ListenerErrorPoint.ReceiveClientProperties, false)]
    [TestCase(ListenerErrorPoint.ClientConnectionHandler, false)]
    public void IsHostInterrupted(ListenerErrorPoint errorPoint, bool expected)
    {
        // arrange
        ListenerError error = new(new Exception(), errorPoint);

        // act
        bool result = error.IsHostInterrupted;

        // assert
        result.Should().Be(expected);
    }

    [Test]
    public void ToStringReturnsExceptionToString()
    {
        // arrange
        Exception exception = new("Foo");
        ListenerError error = new(exception, default);

        // act
        string result = error.ToString();

        // assert
        result.Should().Be(exception.ToString());
    }
}
