using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Loader.Features.Paths;
using SecretAPI.Extensions;
using SecretAPI.Features;
using SecretAPI.Features.UserSettings;
using SillySCP.API.Settings;
using UnityEngine;
using VoiceChat;

namespace SillySCP.Handlers
{
    public class SSSSHandler : IRegister
    { 
        public void TryRegister()
        {
            // PlayerEvents.Joined += OnVerified;
            // ServerSpecificSettingsSync.ServerOnSettingValueReceived += SettingRecieved;
            
            CustomSetting.Register(new JailbirdSetting(), new IntercomSetting(), new RussianRoulette(), new ExclusiveColorSetting(), new PronounSetting());

            string sillyAudiosLocation = Path.Combine(PathManager.Configs.FullName, "Silly Audios");
            
            AudioClipStorage.LoadClip(Path.Combine(sillyAudiosLocation, "kali 1.ogg"), "meow 1");
            AudioClipStorage.LoadClip(Path.Combine(sillyAudiosLocation, "kali 2.ogg"), "meow 2");
            AudioClipStorage.LoadClip(Path.Combine(sillyAudiosLocation, "cyn 1.ogg"), "meow 3");
            AudioClipStorage.LoadClip(Path.Combine(sillyAudiosLocation, "cyn 2.ogg"), "meow 4");

            // intercom handler
            LabApi.Events.Handlers.PlayerEvents.ReceivingVoiceMessage += OnReceivingVoiceMessage;
        }

        public void TryUnregister()
        {
            LabApi.Events.Handlers.PlayerEvents.ReceivingVoiceMessage -= OnReceivingVoiceMessage;

            foreach (AudioClipData clipData in AudioClipStorage.AudioClips.Values)
            {
                AudioClipStorage.DestroyClip(clipData.Name);
            }
        }

        private void OnReceivingVoiceMessage(PlayerReceivingVoiceMessageEventArgs ev)
        {
            if (ev.Message.Channel != VoiceChatChannel.Intercom) return;
            IntercomSetting setting = CustomSetting.GetPlayerSetting<IntercomSetting>(IntercomSetting.SettingId, ev.Player);
            if (setting == null) return;
            ev.IsAllowed = setting.IsOptionB;
        }

        internal static void OnJailbirdEvent(LabApi.Features.Wrappers.Player player)
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
                    p.transform.parent = player.GameObject.transform;
                    Speaker speaker = p.AddSpeaker("Jailbird Speaker", isSpatial: true, minDistance: 5f, maxDistance: 15f);
                    speaker.transform.parent = player.GameObject.transform;
                    speaker.transform.localPosition = Vector3.zero;
                }
            );
            audioPlayer.AddClip(AudioClipStorage.AudioClips.Values.Where(data => data.Name.Contains("meow")).GetRandomValue().Name, 3);
        }
    }
}