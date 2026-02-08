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
        Assert.That(result, Is.EqualTo(exception));
    }

    [Theory]
    public void ErrorPoint(ListenerErrorPoint value)
    {
        // arrange
        ListenerError error = new(new Exception(), value);

        // act
        ListenerErrorPoint result = error.ErrorPoint;

        // assert
        Assert.That(result, Is.EqualTo(value));
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
        Assert.That(result, Is.EqualTo(expected));
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
        Assert.That(result, Is.EqualTo(exception.ToString()));
    }
}
