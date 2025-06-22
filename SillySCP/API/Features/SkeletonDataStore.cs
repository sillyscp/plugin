using System.Diagnostics;
using CustomPlayerEffects;
using LabApi.Features.Stores;
using LabApi.Features.Wrappers;
using PlayerRoles;
using PlayerRoles.PlayableScps.Scp3114;
using PlayerRoles.Subroutines;
using Utils.Networking;

namespace SillySCP.API.Features
{
    public class SkeletonDataStore : CustomDataStore
    {
        public SkeletonDataStore(Player player) : base(player)
        {
            Skeleton = player;
            Cooldown = new();
            Role = (Scp3114Role)player.RoleBase;
            Role.SubroutineModule.TryGetSubroutine(out Scp3114Strangle strangle);
            if (!strangle) return;
            Strangle = strangle;
        }
        
        public Player Skeleton { get; set; }
        
        public Scp3114Role Role { get; set; }
        
        public Scp3114Strangle Strangle { get; set; }

        public AbilityCooldown Cooldown;

        public void StopStrangle()
        {
            if (!Strangle.SyncTarget.HasValue) return;
            Player player = Player.Get(Strangle.SyncTarget.Value.Target);
            if (player == null) return;
            Strangle.SyncTarget = null;
            Strangle._rpcType = Scp3114Strangle.RpcType.AttackInterrupted;
            Strangle.ServerSendRpc(true);
            player.DisableEffect<Strangled>();
        }

        public bool CanStartStrangle => Cooldown.IsReady;

        public static SkeletonDataStore GetFromStrangle(Scp3114Strangle strangle) => 
            Player.ReadyList
                .Where(p => p.Role == RoleTypeId.Scp3114)
                .FirstOrDefault(p => p.GetDataStore<SkeletonDataStore>().Strangle == strangle)?
                .GetDataStore<SkeletonDataStore>();
    }
}