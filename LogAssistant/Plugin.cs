using DiscordLab.Bot;
using LabApi.Events.CustomHandlers;
using LabApi.Features;
using LabApi.Loader.Features.Plugins;
using Logger = LabApi.Features.Console.Logger;

namespace LogAssistant;

public class Plugin : Plugin<Config>
{
    public static Plugin Instance = null!;

    public override string Name { get; } = "SillySCP Discord Assistant";
    public override string Description { get; } = "Helps with stuff on the SillySCP Discord.";
    public override string Author { get; } = "SillySCP Team";
    public override Version RequiredApiVersion { get; } = new(LabApiProperties.CompiledVersion);

    private Events? Events { get; set; }

    public override void Enable()
    {
        Instance = this;

        Client.SocketClient.ChannelCreated += DiscordEvents.OnChannelCreated;

        Events = new();
        CustomHandlersManager.RegisterEventsHandler(Events);
    }

    public override void Disable()
    {
        CustomHandlersManager.UnregisterEventsHandler(Events!);
        Events = null;

        Client.SocketClient.ChannelCreated -= DiscordEvents.OnChannelCreated;
    }
}