using Exiled.API.Extensions;
using Exiled.Events.EventArgs.Player;
using PlayerRoles;
using PlayerRoles.RoleAssign;
using SillySCP.API.Interfaces;

namespace SillySCP.Handlers
{
    public class ScpSwapHandler : IRegisterable
    {
        public void Init()
        {
            Exiled.Events.Handlers.Player.Spawned += OnPlayerSpawned;
            Exiled.Events.Handlers.Player.ChangingRole += OnChangingRole;
        }

        public void Unregister()
        {
            Exiled.Events.Handlers.Player.Spawned -= OnPlayerSpawned;
            Exiled.Events.Handlers.Player.ChangingRole -= OnChangingRole;
        }

        private void OnPlayerSpawned(SpawnedEventArgs ev)
        {
            if (Exiled.API.Features.Player.List.Count(p => p.IsScp) is 1 or 2 &&
                ev.Player.Role.Type == RoleTypeId.Scp079)
            {
                ev.Player.Role.Set(ev.OldRole.Team == Team.SCPs ? ev.OldRole.Type : ScpSpawner.NextScp);
                ev.Player.Broadcast(new("SCP-079 cannot at 1/2 scps."));
            }
        }

        private void OnChangingRole(ChangingRoleEventArgs ev)
        {
            if (ev.Player.IsScp && RoleExtensions.GetTeam(ev.NewRole) == Team.SCPs)
                DiscordBot.Instance.ScpSwapChannel.SendMessageAsync(
                    $"Player `{ev.Player.Nickname}` has swapped from `{ev.Player.Role.Name}` to `{ev.NewRole.GetFullName()}`");
        }
    }
}