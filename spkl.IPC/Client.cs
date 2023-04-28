using spkl.IPC.Messaging;
using System;

namespace spkl.IPC
{
    public class Client
    {
        public string FilePath { get; private set; }

        private MessageChannel Channel { get; set; }

        public IHostConnectionHandler Handler { get; private set; }

        private Client()
        {
        }

        public static void Attach(string filePath, IHostConnectionHandler handler)
        {
            MessageChannel channel = MessageChannel.ConnectTo(filePath);

            Client client = new() 
            {
                FilePath = filePath,
                Channel = channel,
                Handler = handler
            };

            client.SendClientProperties();
            client.RunReceiveLoop();
        }

        private void SendClientProperties()
        {
            this.Channel.Receiver.ReceiveReqArgs();
            this.Channel.Sender.SendArgs(Environment.GetCommandLineArgs());

            this.Channel.Receiver.ReceiveReqCurrentDir();
            this.Channel.Sender.SendCurrentDir(Environment.CurrentDirectory);
        }

        private void RunReceiveLoop()
        {
            bool receivedExit = false;
            MessageType messageType;
            while ((messageType = this.Channel.Receiver.ReceiveMessage()) != MessageType.ConnClosed)
            {
                if (messageType == MessageType.OutStr)
                {
                    string str = this.Channel.Receiver.ExpectString();
                    this.Handler.HandleOutString(str);
                }
                else if (messageType == MessageType.ErrStr)
                {
                    string str = this.Channel.Receiver.ExpectString();
                    this.Handler.HandleErrorString(str);
                }
                else if (messageType == MessageType.Exit)
                {
                    int exitCode = this.Channel.Receiver.ExpectInt();
                    this.Handler.HandleExit(exitCode);
                    receivedExit = true;
                }
                else
                {
                    throw new Exception($"Received unexpected message type {messageType}"); // TODO exception type
                }
            }

            if (!receivedExit)
            {
                throw new Exception("Did not receive exit message"); // TODO exception type
            }
        }
    }
}
