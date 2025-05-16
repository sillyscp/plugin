using CustomPlayerEffects;
using Exiled.API.Features;
using InventorySystem.Items.Firearms.Modules;
using LabApi.Events.Arguments.PlayerEvents;
using PlayerRoles;
using PlayerRoles.PlayableScps.Scp3114;
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

        private static readonly DynamicElement Element = new (HintContent, 300);

        public StruggleSetting()
            : base(SettingId, "Struggle", KeyCode.E, hint: "The key bind to press when being strangled by 3114 to potentially break free.")
        {}

        protected override CustomSetting CreateDuplicate() => new StruggleSetting();

        public override CustomHeader Header { get; } = SSSSModule.Header;

        private static readonly Dictionary<Player, (float percentage, Display display)> StranglePercentage = new ();

        protected override void HandleSettingUpdate(Player player)
        {
            if (!player.HasEffect<Strangled>()) return;
            (float percentage, Display display) val = StranglePercentage[player];
            val.percentage += 2f;
            if(val.percentage >= 100)
            {
                Scp3114Role role = (Scp3114Role)Exiled.API.Features.Player.Get(RoleTypeId.Scp3114).First().Role;
                role.SubroutineModule.TryGetSubroutine(out Scp3114Strangle strangle);
                strangle.SyncTarget = null;
                strangle._rpcType = Scp3114Strangle.RpcType.AttackInterrupted;
                strangle.ServerSendRpc(true);
                Remove(player);
                return;
            }
            val.display.Update();
            StranglePercentage[player] = val;
        }

        internal static void Add(Player player)
        {
            if (StranglePercentage.ContainsKey(player)) return;
            Display display = new(player.ReferenceHub);
            StranglePercentage.Add(player, (0, display));
            display.Elements.Add(Element);
            display.Update();
        }

        internal static void Remove(Player player)
        {
            if (!StranglePercentage.TryGetValue(player, out var displayInfo)) return;
            displayInfo.display.Delete();
            StranglePercentage.Remove(player);
        }

        private static string HintContent(DisplayCore core)
        {
            Player player = Player.Get(core.Hub);
            (float percentage, Display display) val = StranglePercentage[player];
            return $"{Hint}\n{val.percentage}%";
        }
    }
}