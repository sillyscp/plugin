using LabApi.Features.Wrappers;
using SecretAPI.Features.UserSettings;
using SillySCP.API.Modules;

namespace SillySCP.API.Features
{
    public class PronounSetting : CustomDropdownSetting
    {
        public PronounSetting()
            : base(834, "Pronouns",
                new[] { "none specified", "she/her", "he/him", "they/them", "any pronouns", "ask" },
                hint: "Select the pronouns which appear next to your name.")
        {}

        public override CustomHeader Header { get; } = SSSSModule.Header;

        protected override CustomSetting CreateDuplicate() => new PronounSetting();

        protected override void HandleSettingUpdate(Player ply)
        {
            Exiled.API.Features.Player player = Exiled.API.Features.Player.Get(ply.ReferenceHub);
            player.DisplayNickname = SelectedOption switch
            {
                "none specified" => null,
                "he/him" => $"{player.Nickname} (he/him)",
                "she/her" => $"{player.Nickname} (she/her)",
                "they/them" => $"{player.Nickname} (they/them)",
                "any pronouns" => $"{player.Nickname} (any pronouns)",
                "ask" => $"{player.Nickname} (ask)",
                _ => null
            };
        }
    }
}