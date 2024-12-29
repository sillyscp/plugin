using Exiled.API.Features.Core.UserSettings;

namespace SillySCP.API.Modules
{
    public static class SSSSModule
    {
        public static HeaderSetting Header { get; } = new("SillySCP Settings");
        
        public static int PronounsDropdownSettingId { get; } = 834;

        public static IEnumerable<SettingBase> Settings = new SettingBase[]
        {
            Header,
            new DropdownSetting(PronounsDropdownSettingId, "Pronouns",
                new[] { "none specified", "she/her", "he/him", "they/them", "any pronouns", "ask" },
                hintDescription: "Select the pronouns which appear next to your name."),
        };
    }
}