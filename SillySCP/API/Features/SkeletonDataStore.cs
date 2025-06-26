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
            if (player.RoleBase is not Scp3114Role role) return;
            Role = role;
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
            if (!StrangleTarget) return;
            Player player = Player.Get(StrangleTarget);
            Strangle.SyncTarget = null;
            Strangle._rpcType = Scp3114Strangle.RpcType.AttackInterrupted;
            Strangle.ServerSendRpc(true);
            player.DisableEffect<Strangled>();
        }

        public bool CanStartStrangle => Cooldown.IsReady;

        public bool IsStrangling => StrangleTarget;

        public ReferenceHub StrangleTarget
        {
            get
            {
                try
                {
                    if(Strangle.SyncTarget == null || !Strangle.SyncTarget.Value.Target) return null;
                    return Strangle.SyncTarget.Value.Target;
                }
                catch (NullReferenceException)
                {
                    return null;
                }
            }
        }

        public static SkeletonDataStore GetFromStrangle(Scp3114Strangle strangle) => 
            ValidSkeletons
                .FirstOrDefault(p => p.GetDataStore<SkeletonDataStore>().Strangle == strangle)?
                .GetDataStore<SkeletonDataStore>();

        public static IEnumerable<Player> ValidSkeletons => Player.ReadyList.Where(IsValidSkeleton);

        public static bool IsValidSkeleton(Player player) =>
            player.RoleBase is Scp3114Role role && role.SubroutineModule.TryGetSubroutine(out Scp3114Strangle _);
    }
}