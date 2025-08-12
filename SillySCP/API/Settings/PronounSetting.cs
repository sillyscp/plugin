using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;
using LabApi.Features.Console;
using LabApi.Features.Wrappers;
using SecretAPI.Features.UserSettings;
using SillySCP.API.Modules;

namespace SillySCP.API.Settings
{
    public class PronounSetting : CustomDropdownSetting
    {
        public PronounSetting()
            : base(null, "Pronouns", new[] { "none specified", "she/her", "he/him", "they/them", "any pronouns", "ask" }, hint: "Select the pronouns which appear next to your name.") 
        {}

        protected override CustomSetting CreateDuplicate() => new PronounSetting();

        protected override void HandleSettingUpdate()
        {
            if (KnownOwner == null)
                return;
            
            KnownOwner.DisplayName = null!;
            switch (SelectedOption)
            {
                case "none specified":
                    KnownOwner.DisplayName = null!;
                    break;
                case "he/him":
                    KnownOwner.DisplayName = $"{KnownOwner.Nickname} (he/him)";
                    break;
                case "she/her":
                    KnownOwner.DisplayName = $"{KnownOwner.Nickname} (she/her)";
                    break;
                case "they/them":
                    KnownOwner.DisplayName = $"{KnownOwner.Nickname} (they/them)";
                    break;
                case "any pronouns":
                    KnownOwner.DisplayName = $"{KnownOwner.Nickname} (any pronouns)";
                    break;
                case "ask":
                    KnownOwner.DisplayName = $"{KnownOwner.Nickname} (ask)";
                    break;
            }
        }

        public override CustomHeader Header { get; } = SSSSModule.Header;
    }
}