﻿// Copyright (c) Sebastian Fischer. All Rights Reserved.
// Licensed under the MIT License.

using System;

namespace spkl.IPC;

public class ConnectionException : Exception
{
    public ConnectionException(string message) : base(message)
    {
    }

    public ConnectionException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
