namespace spkl.IPC
{
    public enum MessageType : byte
    {
        ConnectionClosed = 0,

        /// <summary>
        /// Layout: Length of Message in bytes (int) | Message (string)
        /// </summary>
        OutString = 0x30,
    }
}