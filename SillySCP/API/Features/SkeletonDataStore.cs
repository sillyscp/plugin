using System.Diagnostics;
using CustomPlayerEffects;
using JetBrains.Annotations;
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
            _role = role;
            role.SubroutineModule.TryGetSubroutine(out Scp3114Strangle strangle);
            if (!strangle) return;
            _strangle = strangle;
        }
        
        private Player Skeleton { get; }

        private readonly Scp3114Role _role;

        [CanBeNull]
        private Scp3114Role Role
        {
            get
            {
                if (_role) return _role;
                return Skeleton is not { RoleBase: Scp3114Role role } ? null : role;
            }
        }

        private Scp3114Strangle _strangle;

        [CanBeNull]
        private Scp3114Strangle Strangle
        {
            get
            {
                if(_strangle) return _strangle;
                if (!Role?.SubroutineModule) return null;
                Role.SubroutineModule.TryGetSubroutine(out _strangle);
                return _strangle;
            }
        }

        public readonly AbilityCooldown Cooldown;

        public void StopStrangle()
        {
            if (!StrangleTarget || !Strangle) return;
            Player player = Player.Get(StrangleTarget);
            Strangle.SyncTarget = null;
            Strangle._rpcType = Scp3114Strangle.RpcType.AttackInterrupted;
            Strangle.ServerSendRpc(true);
            player.DisableEffect<Strangled>();
        }

        public bool CanStartStrangle => Cooldown.IsReady;

        public bool IsStrangling => StrangleTarget;

        private ReferenceHub StrangleTarget
        {
            get
            {
                try
                {
                    if(!Strangle || Strangle.SyncTarget == null || !Strangle.SyncTarget.Value.Target) return null;
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

        private static IEnumerable<Player> ValidSkeletons => Player.ReadyList.Where(IsValidSkeleton);

        private static bool IsValidSkeleton(Player player) =>
            player.RoleBase is Scp3114Role role && role.SubroutineModule.TryGetSubroutine(out Scp3114Strangle _);
    }
}