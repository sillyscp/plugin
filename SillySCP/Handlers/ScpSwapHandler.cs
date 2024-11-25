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
            List<Exiled.API.Features.Player> scps = Exiled.API.Features.Player.List.Where(p => p.IsScp).ToList();
            if (scps.Count(p => p.IsScp) is 1 or 2 &&
                ev.Player.Role.Type == RoleTypeId.Scp079)
            {
                scps.Remove(ev.Player);
                PlayerRoleBase[] spawnableScps = ScpSpawner.SpawnableScps.Where(s => s.RoleTypeId != RoleTypeId.Scp079 && s.RoleTypeId != scps.FirstOrDefault()?.Role.Type).ToArray();
                ev.Player.Role.Set(ev.OldRole.Team == Team.SCPs ? ev.OldRole.Type : spawnableScps[UnityEngine.Random.Range(0, spawnableScps.Length-1)].RoleTypeId);
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