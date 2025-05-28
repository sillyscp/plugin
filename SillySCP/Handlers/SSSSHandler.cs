using Exiled.Events.EventArgs.Item;
using Exiled.Events.EventArgs.Player;
using SecretAPI.Features.UserSettings;
using SillySCP.API.Interfaces;
using SillySCP.API.Modules;
using SillySCP.API.Settings;
using UnityEngine;
using UserSettings.ServerSpecific;

namespace SillySCP.Handlers
{
    public class SSSSHandler : IRegisterable
    { 
        public void Init()
        {
            Exiled.Events.Handlers.Player.Verified += OnVerified;
            ServerSpecificSettingsSync.ServerOnSettingValueReceived += SettingRecieved;
            
            CustomSetting.Register(new ExclusiveColorSetting(), new StruggleSetting(), new JailbirdSetting());
            
            // jailbird handler

            Exiled.Events.Handlers.Item.Swinging += OnJailbirdSwing;
        }
        
        public void Unregister()
        {
            Exiled.Events.Handlers.Player.Verified -= OnVerified;
            
            Exiled.Events.Handlers.Item.Swinging -= OnJailbirdSwing;
        }

        private void OnJailbirdSwing(SwingingEventArgs ev)
        {
            AudioPlayer audioPlayer = AudioPlayer.CreateOrGet($"Jailbird {ev.Player.Nickname}", 
                condition: hub =>
                    {
                        LabApi.Features.Wrappers.Player player = LabApi.Features.Wrappers.Player.Get(hub);
                        if (player == null) return true;
                        JailbirdSetting setting = CustomSetting.GetPlayerSetting<JailbirdSetting>(JailbirdSetting.SettingId, player);
                        return setting == null || setting.IsDefault;
                    }, 
                onIntialCreation: (p) =>
                    {
                        p.transform.parent = ev.Player.Transform;
                        Speaker speaker = p.AddSpeaker("Jailbird Speaker", isSpatial: true, minDistance: 5f, maxDistance: 15f);
                        speaker.transform.parent = ev.Player.Transform;
                        speaker.transform.localPosition = Vector3.zero;
                    }
            );
        }

        private void OnVerified(VerifiedEventArgs ev)
        {
            ServerSpecificSettingsSync.SendToPlayer(ev.Player.ReferenceHub);
            SSDropdownSetting ssSetting = ServerSpecificSettingsSync.GetSettingOfUser<SSDropdownSetting>(ev.Player.ReferenceHub, SSSSModule.PronounsDropdownSettingId);
            if (ssSetting != null)
            {
                SetNickname(ssSetting, ev.Player);
            }
        }

        private void SetNickname(SSDropdownSetting setting, Exiled.API.Features.Player player)
        {
            player.DisplayNickname = null;
            switch (setting.SyncSelectionText)
            {
                case "none specified":
                    player.DisplayNickname = null;
                    break;
                case "he/him":
                    player.DisplayNickname = $"{player.Nickname} (he/him)";
                    break;
                case "she/her":
                    player.DisplayNickname = $"{player.Nickname} (she/her)";
                    break;
                case "they/them":
                    player.DisplayNickname = $"{player.Nickname} (they/them)";
                    break;
                case "any pronouns":
                    player.DisplayNickname = $"{player.Nickname} (any pronouns)";
                    break;
                case "ask":
                    player.DisplayNickname = $"{player.Nickname} (ask)";
                    break;
            }
        }
        
        private void SettingRecieved(ReferenceHub hub, ServerSpecificSettingBase settingBase)
        {
            if(!Exiled.API.Features.Player.TryGet(hub, out Exiled.API.Features.Player player)) return;
            
            if(settingBase.SettingId == SSSSModule.PronounsDropdownSettingId)
            {
                SetNickname((SSDropdownSetting) settingBase, player);
            }
        }
    }
}