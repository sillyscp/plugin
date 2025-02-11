using Exiled.API.Enums;
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
        }

        public void Unregister()
        {
            Exiled.Events.Handlers.Player.Spawned -= OnPlayerSpawned;
        }

        private void OnPlayerSpawned(SpawnedEventArgs ev)
        {
            if (ev.Reason == SpawnReason.ForceClass || ev.Player.Role.Type != RoleTypeId.Scp079 || ScpSpawner.EnqueuedScps.Count != 0) 
                return;

            List<Exiled.API.Features.Player> scps = Exiled.API.Features.Player.List.Where(p => p.IsScp).ToList();
            if (scps.Count > 2) 
                return;

            List<RoleTypeId> spawnableScps = ScpSpawner.SpawnableScps
                .Where(r => r.RoleTypeId != RoleTypeId.Scp079 && !scps.Any(p => p.Role.Type == r.RoleTypeId))
                .Select(r => r.RoleTypeId)
                .ToList();

            ev.Player.Role.Set(spawnableScps.GetRandomValue());
        }
    }
}