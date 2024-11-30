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
            List<RoleTypeId> scps = Exiled.API.Features.Player.List.Where(p => p.IsScp && p.Role.Type != RoleTypeId.Scp0492).Select(p => p.Role.Type).ToList();
            if (scps.Count is 1 or 2 &&
                ev.Player.Role.Type == RoleTypeId.Scp079)
            {
                scps.Remove(RoleTypeId.Scp079);
                PlayerRoleBase[] spawnableScps = ScpSpawner.SpawnableScps.Where(s => s.RoleTypeId != RoleTypeId.Scp079 && s.RoleTypeId != scps.FirstOrDefault()).ToArray();
                ev.Player.Role.Set(ev.OldRole.Team == Team.SCPs ? ev.OldRole.Type : spawnableScps.GetRandomValue().RoleTypeId);
                ev.Player.Broadcast(new("SCP-079 cannot at 1/2 scps."));
            }

            if (scps.Count is 1 or 2 && scps.Count(s => s == scps.FirstOrDefault()) == 2)
            {
                foreach (Exiled.API.Features.Player player in Exiled.API.Features.Player.List.Where(p => p.IsScp))
                {
                    player.Role.Set(ScpSpawner.SpawnableScps.Where(s => s.RoleTypeId != RoleTypeId.Scp079 && s.RoleTypeId != scps.FirstOrDefault()).GetRandomValue().RoleTypeId);
                }
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