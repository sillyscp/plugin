using CustomPlayerEffects;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Features.Wrappers;
using RueI.Displays;
using RueI.Elements;
using RueI.Extensions;
using SecretAPI.Features.UserSettings;
using SillySCP.API.Modules;
using UnityEngine;

namespace SillySCP.API.Features
{
    public class StruggleSetting : CustomKeybindSetting
    {
        internal static string Hint => "Press {0} to break free.";

        internal static int SettingId => 836;

        public StruggleSetting()
            : base(SettingId, "Struggle", KeyCode.E, hint: "The key bind to press when being strangled by 3114 to potentially break free.")
        {
            LabApi.Events.Handlers.PlayerEvents.UpdatingEffect += OnUpdatingEffect;
        }

        protected override CustomSetting CreateDuplicate() => new StruggleSetting();

        public override CustomHeader Header { get; } = SSSSModule.Header;

        private readonly Dictionary<Player, (float percentage, IElemReference<SetElement> elemReference)> _stranglePercentage = new ();

        protected override void HandleSettingUpdate(Player player)
        {
            if (!player.HasEffect<Strangled>()) return;
            (float percentage, IElemReference<SetElement> elemReference) val = _stranglePercentage[player];
            val.percentage += 9.5f;
            if(val.percentage >= 100) 
                player.DisableEffect<Strangled>();
            UpdateHint(player, val.percentage, val.elemReference);
            _stranglePercentage[player] = val;
        }

        private void OnUpdatingEffect(PlayerEffectUpdatingEventArgs ev)
        {
            if(ev.Effect is Strangled) AddOrRemove(ev.Player);
        }

        private void AddOrRemove(Player player)
        {
            if (player.HasEffect<Strangled>())
            {
                IElemReference<SetElement> elemReference = DisplayCore.GetReference<SetElement>();
                _stranglePercentage.Add(player, (0, elemReference));
                UpdateHint(player, 0, elemReference);
            }
            else
            {
                (float percentage, IElemReference<SetElement> elemReference) val = _stranglePercentage[player];
                DisplayCore.Get(player.ReferenceHub).RemoveReference(val.elemReference);
                _stranglePercentage.Remove(player);
            }
        }

        private static void UpdateHint(Player player, float percentage, IElemReference<SetElement> elemReference)
        {
            DisplayCore.Get(player.ReferenceHub).SetElementOrNew(elemReference, $"{Hint}\n{percentage}%", 300);
        }
    }
}