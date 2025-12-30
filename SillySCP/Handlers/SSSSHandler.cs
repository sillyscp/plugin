using InventorySystem.Items.Jailbird;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;
using LabApi.Loader.Features.Paths;
using SecretAPI.Extensions;
using SecretAPI.Features;
using SecretAPI.Features.UserSettings;
using SillySCP.API.Settings;
using UnityEngine;
using VoiceChat;

namespace SillySCP.Handlers
{
    // ReSharper disable once InconsistentNaming
    public class SSSSHandler : IRegister
    { 
        public void TryRegister()
        {
            // PlayerEvents.Joined += OnVerified;
            // ServerSpecificSettingsSync.ServerOnSettingValueReceived += SettingRecieved;
            
            CustomSetting.Register(new JailbirdSetting(), new IntercomSetting(), new ExclusiveColorSetting(), new PronounSetting(), new RussianRoulette(), new ZombieSnowball());

            string sillyAudiosLocation = Path.Combine(PathManager.Configs.FullName, "Silly Audios");
            
            AudioClipStorage.LoadClip(Path.Combine(sillyAudiosLocation, "kali 1.ogg"), "jailbird meow 1");
            AudioClipStorage.LoadClip(Path.Combine(sillyAudiosLocation, "kali 2.ogg"), "jailbird meow 2");

            // intercom handler
            PlayerEvents.ReceivingVoiceMessage += OnReceivingVoiceMessage;

            // jailbird handler
            PlayerEvents.ProcessedJailbirdMessage += OnJailbirdEvent;
        }

        public void TryUnregister()
        {
            PlayerEvents.ReceivingVoiceMessage -= OnReceivingVoiceMessage;

            PlayerEvents.ProcessedJailbirdMessage -= OnJailbirdEvent;

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

        private static void OnJailbirdEvent(PlayerProcessedJailbirdMessageEventArgs ev)
        {
            if (ev.Message != JailbirdMessageType.AttackTriggered)
                return;
            
            AudioPlayer audioPlayer = AudioPlayer.CreateOrGet($"Jailbird {ev.Player.Nickname}", 
                condition: hub =>
                {
                    LabApi.Features.Wrappers.Player plr = LabApi.Features.Wrappers.Player.Get(hub);
                    if (plr == null) return true;
                    JailbirdSetting setting = CustomSetting.GetPlayerSetting<JailbirdSetting>(JailbirdSetting.SettingId, plr);
                    return setting == null || setting.IsOptionA;
                }, 
                onIntialCreation: p =>
                {
                    p.transform.parent = ev.Player.GameObject?.transform;
                    Speaker speaker = p.AddSpeaker("Jailbird Speaker", isSpatial: true, minDistance: 5f, maxDistance: 15f);
                    speaker.transform.parent = ev.Player.GameObject?.transform;
                    speaker.transform.localPosition = Vector3.zero;
                }
            );
            audioPlayer.AddClip(AudioClipStorage.AudioClips.Values.Where(data => data.Name.Contains("jailbird meow")).GetRandomValue().Name, 3);
        }
    }
}