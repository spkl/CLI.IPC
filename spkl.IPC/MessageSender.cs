using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace spkl.IPC
{
    public class MessageSender
    {
        public Socket Socket { get; }

        internal MessageSender(Socket socket)
        {
            this.Socket = socket;
        }
    }
}
