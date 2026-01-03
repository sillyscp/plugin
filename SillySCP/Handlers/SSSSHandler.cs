using InventorySystem.Items.Jailbird;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;
using LabApi.Loader.Features.Paths;
using SecretAPI.Extensions;
using SecretAPI.Features;
using SecretAPI.Features.UserSettings;
using SecretLabNAudio.Core;
using SecretLabNAudio.Core.Extensions;
using SecretLabNAudio.Core.FileReading;
using SecretLabNAudio.Core.Pools;
using SillySCP.API.Settings;
using VoiceChat;

namespace SillySCP.Handlers
{
    // ReSharper disable once InconsistentNaming
    public class SSSSHandler : IRegister
    {
        private static readonly SpeakerSettings JailbirdSettings = new()
        {
            IsSpatial = true,
            MaxDistance = 15,
            MinDistance = 5,
            Volume = 2
        };

        public void TryRegister()
        {
            // PlayerEvents.Joined += OnVerified;
            // ServerSpecificSettingsSync.ServerOnSettingValueReceived += SettingRecieved;

            CustomSetting.Register(new JailbirdSetting(), new IntercomSetting(), new ExclusiveColorSetting(),
                new PronounSetting(), new RussianRoulette(), new ZombieSnowball());

            string audioLocation = Path.Combine(PathManager.Configs.FullName, "Silly Audios");

            ShortClipCache.AddFromFile(Path.Combine(audioLocation, "kali 1.ogg"), "jailbird meow 1");
            ShortClipCache.AddFromFile(Path.Combine(audioLocation, "kali 2.ogg"), "jailbird meow 2");

            // intercom handler
            PlayerEvents.ReceivingVoiceMessage += OnReceivingVoiceMessage;

            // jailbird handler
            PlayerEvents.ProcessedJailbirdMessage += OnJailbirdEvent;
        }

        public void TryUnregister()
        {
            PlayerEvents.ReceivingVoiceMessage -= OnReceivingVoiceMessage;
            PlayerEvents.ProcessedJailbirdMessage -= OnJailbirdEvent;
        }

        private static void OnReceivingVoiceMessage(PlayerReceivingVoiceMessageEventArgs ev)
        {
            if (ev.Message.Channel != VoiceChatChannel.Intercom) return;
            IntercomSetting setting =
                CustomSetting.GetPlayerSetting<IntercomSetting>(IntercomSetting.SettingId, ev.Player);
            if (setting == null) return;
            ev.IsAllowed = setting.IsOptionB;
        }

        private static void OnJailbirdEvent(PlayerProcessedJailbirdMessageEventArgs ev)
        {
            if (ev.Message != JailbirdMessageType.AttackTriggered)
                return;

            if (!ev.Player.GameObject)
                return;

            AudioPlayerPool.Rent(JailbirdSettings, ev.Player.GameObject.transform)
                .WithFilteredSendEngine(static player => !CustomSetting.TryGetPlayerSetting(player, out JailbirdSetting setting) || setting.IsOptionA)
                .UseExactShortClip(
                    ShortClipCache.Keys
                        .Where(static x => x.StartsWith("jailbird meow"))
                        .GetRandomValue()
                )
                .PoolOnEnd();
        }
    }
}