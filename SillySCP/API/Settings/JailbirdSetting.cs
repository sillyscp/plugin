using LabApi.Features.Wrappers;
using SecretAPI.Features.UserSettings;
using SillySCP.API.Modules;

namespace SillySCP.API.Settings
{
    public class JailbirdSetting : CustomTwoButtonSetting
    {
        public static int SettingId { get; private set; }

        public JailbirdSetting()
            : base(null, "Meow on Jailbird swing", "Yes", "No",
                hint: "When you or someone else swings the jailbird, should you hear the meow?")
        {
            SettingId = Id;
        }

        protected override CustomSetting CreateDuplicate() => new JailbirdSetting();

        public override CustomHeader Header { get; } = SSSSModule.Header;

        protected override void HandleSettingUpdate()
        {}
    }
}