using LabApi.Events.Arguments.PlayerEvents;
using PlayerRoles;
using PlayerRoles.RoleAssign;
using SecretAPI.Extensions;
using SecretAPI.Features;

namespace SillySCP.Handlers
{
    public class ScpSwapHandler : IRegister
    {
        public void TryRegister()
        {
            LabApi.Events.Handlers.PlayerEvents.Spawned += OnPlayerSpawned;
        }
        
        public void TryUnregister()
        {
            LabApi.Events.Handlers.PlayerEvents.Spawned -= OnPlayerSpawned;
        }

        private void OnPlayerSpawned(PlayerSpawnedEventArgs ev)
        {
            if (ev.Player.Role != RoleTypeId.Scp079 || ScpSpawner.EnqueuedScps.Count != 0) 
                return;

            List<LabApi.Features.Wrappers.Player> scps = LabApi.Features.Wrappers.Player.List.Where(p => p.IsSCP).ToList();
            if (scps.Count > 2) 
                return;

            List<RoleTypeId> spawnableScps = ScpSpawner.SpawnableScps
                .Where(r => r.RoleTypeId != RoleTypeId.Scp079 && !scps.Any(p => p.Role == r.RoleTypeId))
                .Select(r => r.RoleTypeId)
                .ToList();

            ev.Player.Role = spawnableScps.GetRandomValue();
        }
    }
}