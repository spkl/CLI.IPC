using System;
using System.Diagnostics;
using System.IO;

namespace spkl.CLI.IPC.Test;

internal class DefaultHostConnectionHandlerTest : TestBase
{
    private DefaultHostConnectionHandler handler;

    [SetUp]
    public void SetUp()
    {
        this.handler = new DefaultHostConnectionHandler();
    }

    [Test]
    public void Arguments()
    {
        // act
        string[] result = this.handler.Arguments;

        // assert
        Assert.That(result, Is.EqualTo(Environment.GetCommandLineArgs()));
    }

    [Test]
    public void CurrentDirectory()
    {
        // act
        string result = this.handler.CurrentDirectory;

        // assert
        Assert.That(result, Is.EqualTo(Environment.CurrentDirectory));
    }

    [Test]
    public void ProcessID()
    {
        // act
        int result = this.handler.ProcessID;

        // assert
        using Process p = Process.GetCurrentProcess();
        Assert.That(result, Is.EqualTo(p.Id));
    }

    [Test]
    public void HandleOutString()
    {
        // arrange
        TextWriter defaultWriter = Console.Out;
        try
        {
            using StringWriter stringWriter = new();
            Console.SetOut(stringWriter);

            // act
            this.handler.HandleOutString("MyOutString");

            // arrange
            Assert.That(stringWriter.ToString(), Is.EqualTo("MyOutString"));

        }
        finally
        {
            Console.SetOut(defaultWriter);
        }
    }

    [Test]
    public void HandleErrorString()
    {
        // arrange
        TextWriter defaultWriter = Console.Error;
        try
        {
            using StringWriter stringWriter = new();
            Console.SetError(stringWriter);

            // act
            this.handler.HandleErrorString("MyErrorString");

            // arrange
            Assert.That(stringWriter.ToString(), Is.EqualTo("MyErrorString"));

        }
        finally
        {
            Console.SetError(defaultWriter);
        }
    }
}
