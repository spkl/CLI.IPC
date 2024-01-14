namespace spkl.IPC.Services;
internal static class ServiceProvider
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    static ServiceProvider()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    {
        ServiceProvider.InitializeDefaults();
    }

    public static void InitializeDefaults()
    {
        ServiceProvider.MessageChannelFactory = new MessageChannelFactory();
    }

    public static IMessageChannelFactory MessageChannelFactory { get; set; }
}
