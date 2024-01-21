// Copyright (c) Sebastian Fischer. All Rights Reserved.
// Licensed under the MIT License.

namespace spkl.CLI.IPC.Messaging;

public enum MessageType : byte
{
    /// <summary>
    /// Indicates that the connection has been closed.
    /// This is not a message code that is actually sent,
    /// it is only used to indicate a closed connection.
    /// </summary>
    ConnClosed = 0,

    /// <summary>
    /// Request to send command line arguments.
    /// Layout: Empty.
    /// </summary>
    ReqArgs = 0x30,
    /// <summary>
    /// Command line arguments.
    /// Layout: Number of arguments (int); [Length of Argument in bytes (int); Argument (string)] for every argument.
    /// </summary>
    Args = 0x31,

    /// <summary>
    /// Request to send current directory.
    /// Layout: Empty.
    /// </summary>
    ReqCurrentDir = 0x32,
    /// <summary>
    /// Current directory.
    /// Layout: Length of Directory in bytes (int); Directory (string).
    /// </summary>
    CurrentDir = 0x33,

    /// <summary>
    /// Message to write to the standard output of the client.
    /// Layout: Length of Message in bytes (int); Message (string).
    /// </summary>
    OutStr = 0x40,
    /// <summary>
    /// Message to write to the error output of the client.
    /// Layout: Length of Message in bytes (int); Message (string).
    /// </summary>
    ErrStr = 0x41,

    /// <summary>
    /// Request to close the connection and end the process.
    /// Layout: Exit code (int).
    /// </summary>
    Exit = 0x50,
}