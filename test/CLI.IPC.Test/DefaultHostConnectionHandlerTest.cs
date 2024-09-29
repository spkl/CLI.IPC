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
        string[] arguments = this.handler.Arguments;

        // assert
        arguments.Should().Equal(Environment.GetCommandLineArgs());
    }

    [Test]
    public void CurrentDirectory()
    {
        // act
        string currentDirectory = this.handler.CurrentDirectory;

        // assert
        currentDirectory.Should().Be(Environment.CurrentDirectory);
    }

    [Test]
    public void ProcessID()
    {
        // act
        int processId = this.handler.ProcessID;

        // assert
        using Process p = Process.GetCurrentProcess();
        processId.Should().Be(p.Id);
    }

    [Test]
    public void HandleOutString()
    {
        // arrange
        TextWriter defaultWriter = Console.Out;
        try
        {
            using StringWriter consoleOut = new();
            Console.SetOut(consoleOut);

            // act
            this.handler.HandleOutString("MyOutString");

            // arrange
            consoleOut.ToString().Should().Be("MyOutString");
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
            using StringWriter consoleError = new();
            Console.SetError(consoleError);

            // act
            this.handler.HandleErrorString("MyErrorString");

            // arrange
            consoleError.ToString().Should().Be("MyErrorString");
        }
        finally
        {
            Console.SetError(defaultWriter);
        }
    }
}
