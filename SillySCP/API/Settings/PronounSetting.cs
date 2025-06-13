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
            : base(834, "Pronouns", new[] { "none specified", "she/her", "he/him", "they/them", "any pronouns", "ask" }, hint: "Select the pronouns which appear next to your name.") 
        {}

        protected override CustomSetting CreateDuplicate() => new PronounSetting();

        protected override void HandleSettingUpdate(Player player)
        {
            player.DisplayName = null!;
            switch (SelectedOption)
            {
                case "none specified":
                    player.DisplayName = null!;
                    break;
                case "he/him":
                    player.DisplayName = $"{player.Nickname} (he/him)";
                    break;
                case "she/her":
                    player.DisplayName = $"{player.Nickname} (she/her)";
                    break;
                case "they/them":
                    player.DisplayName = $"{player.Nickname} (they/them)";
                    break;
                case "any pronouns":
                    player.DisplayName = $"{player.Nickname} (any pronouns)";
                    break;
                case "ask":
                    player.DisplayName = $"{player.Nickname} (ask)";
                    break;
            }
        }

        public override CustomHeader Header { get; } = SSSSModule.Header;
    }
}