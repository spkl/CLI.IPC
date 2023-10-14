using System;

namespace spkl.IPC
{
    public class ClientProperties
    {
        public string[] Arguments { get; set; }

        public string CurrentDirectory { get; set; }

        public ClientProperties()
        {
            this.Arguments = Array.Empty<string>();
            this.CurrentDirectory = string.Empty;
        }
    }
}
