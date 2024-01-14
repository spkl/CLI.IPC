using System;

namespace spkl.IPC.Test;
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
    public void HostWasShutDown(bool value)
    {
        // arrange
        ListenerError error = new(new Exception(), value);

        // act
        bool result = error.HostWasShutDown;

        // assert
        Assert.That(result, Is.EqualTo(value));
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
