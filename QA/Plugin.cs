using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;
using LabApi.Features.Console;
using RemoteAdmin.Communication;

namespace QA
{
    public class Plugin : LabApi.Loader.Features.Plugins.Plugin
    {
        public static UserGroup Group => ServerStatic.PermissionsHandler.GetGroup("tester");
        
        public override void Enable()
        {
            PlayerEvents.Joined += OnJoined;
            PlayerEvents.RequestingRaPlayerInfo += OnRequestingPlayerInfo;
            PlayerEvents.RequestingRaPlayersInfo += OnRequestingPlayersInfo;
        }

        public override void Disable()
        {
            PlayerEvents.Joined -= OnJoined;
            PlayerEvents.RequestingRaPlayerInfo -= OnRequestingPlayerInfo;
            PlayerEvents.RequestingRaPlayersInfo -= OnRequestingPlayersInfo;
        }

        public static void OnJoined(PlayerJoinedEventArgs ev)
        {
            ev.Player.UserGroup = Group;
        }

        public static void OnRequestingPlayerInfo(PlayerRequestingRaPlayerInfoEventArgs ev)
        {
            if (ev.Player.UserGroup == null)
                return;
            
            if (ev.Player.UserGroup.Name == Group.Name)
                ev.IsAllowed = false;
        }

        public static void OnRequestingPlayersInfo(PlayerRequestingRaPlayersInfoEventArgs ev)
        {
            if (ev.Player.UserGroup == null)
                return;
            
            if (ev.Player.UserGroup.Name == Group.Name)
                ev.IsAllowed = false;
        }

        public override string Name { get; } = "QA Testing Plugin";
        public override string Description { get; } = "QA Testing Plugin";
        public override string Author { get; } = "SillySCP Team";
        public override Version Version { get; } = new(0, 0, 1);
        public override Version RequiredApiVersion { get; } = new(1, 0, 0);
    }
}