using System;
using System.IO;
using System.Text;

namespace spkl.IPC.Internal
{
    internal class DelegateTextWriter : TextWriter
    {
        public override Encoding Encoding => Encoding.UTF8;

        public Action<string> InternalWrite { get; }

        public DelegateTextWriter(Action<string> writeString)
        {
            this.InternalWrite = writeString;
        }

        public override void Write(char value)
        {
            this.InternalWrite(new string(value, 1));
        }

        public override void Write(char[] buffer, int index, int count)
        {
            this.InternalWrite(new string(buffer, index, count));
        }

        public override void Write(string value)
        {
            if (value != null)
            {
                // No need to handle SocketException here, because it is already wrapped
                // as ConnectionException in MessageSender.SendOutStr/SendErrStr.
                this.InternalWrite(value);
            }
        }
    }
}
