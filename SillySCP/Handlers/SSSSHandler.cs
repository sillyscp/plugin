using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Item;
using Exiled.Events.EventArgs.Player;
using PlayerRoles.PlayableScps.Scp079.Cameras;
using SecretAPI.Features.UserSettings;
using SillySCP.API.Interfaces;
using SillySCP.API.Modules;
using SillySCP.API.Settings;
using UnityEngine;
using UserSettings.ServerSpecific;
using Camera = Exiled.API.Features.Camera;

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

            string sillyAudiosLocation = Path.Combine(Paths.Configs, "Silly Audios");
            
            AudioClipStorage.LoadClip(Path.Combine(sillyAudiosLocation, "kali 1.ogg"), "meow 1");
            AudioClipStorage.LoadClip(Path.Combine(sillyAudiosLocation, "kali 2.ogg"), "meow 2");
            AudioClipStorage.LoadClip(Path.Combine(sillyAudiosLocation, "cyn 1.ogg"), "meow 3");
            AudioClipStorage.LoadClip(Path.Combine(sillyAudiosLocation, "cyn 2.ogg"), "meow 4");
        }
        
        public void Unregister()
        {
            Exiled.Events.Handlers.Player.Verified -= OnVerified;
            
            Exiled.Events.Handlers.Item.Swinging -= OnJailbirdSwing;
        }

        private void OnJailbirdEvent(Exiled.API.Features.Player player)
        {
            AudioPlayer audioPlayer = AudioPlayer.CreateOrGet($"Jailbird {player.Nickname}", 
                condition: hub =>
                {
                    LabApi.Features.Wrappers.Player plr = LabApi.Features.Wrappers.Player.Get(hub);
                    if (plr == null) return true;
                    JailbirdSetting setting = CustomSetting.GetPlayerSetting<JailbirdSetting>(JailbirdSetting.SettingId, plr);
                    return setting == null || setting.IsOptionA;
                }, 
                onIntialCreation: p =>
                {
                    p.transform.parent = player.Transform;
                    Speaker speaker = p.AddSpeaker("Jailbird Speaker", isSpatial: true, minDistance: 5f, maxDistance: 15f);
                    speaker.transform.parent = player.Transform;
                    speaker.transform.localPosition = Vector3.zero;
                }
            );
            audioPlayer.AddClip(AudioClipStorage.AudioClips.Values.GetRandomValue(data => data.Name.Contains("meow")).Name, 3);
        }

        private void OnJailbirdSwing(SwingingEventArgs ev)
        {
            OnJailbirdEvent(ev.Player);
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