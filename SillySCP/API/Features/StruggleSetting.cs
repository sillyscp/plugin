using CustomPlayerEffects;
using Exiled.API.Enums;
using Exiled.API.Features;
using InventorySystem.Items.Firearms.Modules;
using JetBrains.Annotations;
using LabApi.Events.Arguments.PlayerEvents;
using PlayerRoles;
using PlayerRoles.PlayableScps.Scp3114;
using PlayerRoles.Subroutines;
using RueI.Displays;
using RueI.Elements;
using RueI.Extensions;
using SecretAPI.Features.UserSettings;
using SillySCP.API.Modules;
using UnityEngine;
using Display = RueI.Displays.Display;
using Player = LabApi.Features.Wrappers.Player;
using Scp3114Role = Exiled.API.Features.Roles.Scp3114Role;

namespace SillySCP.API.Features
{
    public class StruggleSetting : CustomKeybindSetting
    {
        internal static string Hint => "Press {0} to break free.";

        internal static int SettingId => 836;

        internal static readonly DynamicElement Element = new (HintContent, 300);

        public StruggleSetting()
            : base(SettingId, "Struggle", KeyCode.E, hint: "The key bind to press when being strangled by 3114 to potentially break free.")
        {
            _cooldown = new();
            Percentage = 0;
        }

        protected override CustomSetting CreateDuplicate() => new StruggleSetting();

        public override CustomHeader Header { get; } = SSSSModule.Header;

        internal float Percentage;

        [CanBeNull] internal Display Display;

        private readonly AbilityCooldown _cooldown;
        
        protected override void HandleSettingUpdate(Player player)
        {
            if (!player.HasEffect<Strangled>() || !_cooldown.IsReady) return;
            _cooldown.Trigger(0.1f);
            Percentage += 4f;
            if(Percentage >= 100)
            {
                Exiled.API.Features.Player skeleton = Exiled.API.Features.Player.Get(RoleTypeId.Scp3114).FirstOrDefault(skele =>
                {
                    Scp3114Role role = (Scp3114Role)skele.Role;
                    role.SubroutineModule.TryGetSubroutine(out Scp3114Strangle strang);
                    return strang.SyncTarget.HasValue && strang.SyncTarget.Value.Target == player.ReferenceHub;
                });
                if(skeleton == null)
                {
                    player.DisableEffect<Strangled>();
                    return;
                }
                Scp3114Role role = (Scp3114Role)skeleton.Role;
                role.SubroutineModule.TryGetSubroutine(out Scp3114Strangle strangle);
                strangle.SyncTarget = null;
                strangle._rpcType = Scp3114Strangle.RpcType.AttackInterrupted;
                strangle.ServerSendRpc(true);
                player.DisableEffect<Strangled>();
                Reset();
                skeleton.EnableEffect(EffectType.Disabled, 2, 5);
                skeleton.PlayShieldBreakSound();
                return;
            }
            Display ??= new(player.ReferenceHub);
            Display.Update();
        }

        private void Reset()
        {
            if (Display == null) return;
            Percentage = 0;
            Display.Elements.Clear();
            Display.Update();
        }

        private static string HintContent(DisplayCore core)
        {
            StruggleSetting setting = GetPlayerSetting<StruggleSetting>(SettingId, Player.Get(core.Hub));
            return setting == null ? "" : $"{Hint}\n{setting.Percentage}%";
        }

        private static Player GetFirstStrangled()
        {
            return Player.List.FirstOrDefault(player =>
            {
                if (player.HasEffect<Strangled>()) return false;
                StruggleSetting setting = GetPlayerSetting<StruggleSetting>(SettingId, player);
                if (setting == null) return false;
                return setting.Display?.Elements.Count == 1;
            });
        }

        internal static void RemoveFromFirst()
        {
            Player player = GetFirstStrangled();
            if (player == null) return;
            StruggleSetting setting = GetPlayerSetting<StruggleSetting>(SettingId, player)!;
            setting.Reset();
        }
    }
}