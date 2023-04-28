using spkl.IPC;

namespace Client
{
    internal class Program
    {
        static void Main(string[] args)
        {
            spkl.IPC.Client.Attach(
                @"C:\Users\Sebastian\Documents\Projects\StreamTest\Server\bin\Debug\net6.0\socket",
                new DefaultHostConnectionHandler());
        }
    }
}