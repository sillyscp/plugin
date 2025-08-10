using LabApi.Features.Wrappers;
using SecretAPI.Features.UserSettings;
using SillySCP.API.Modules;

namespace SillySCP.API.Settings
{
    public class JailbirdSetting : CustomTwoButtonSetting
    {
        public const int SettingId = 837;
        
        public JailbirdSetting()
            : base(SettingId, "Meow on Jailbird swing", "Yes", "No", hint: "When you or someone else swings the jailbird, should you hear the meow?")
        {}

        protected override CustomSetting CreateDuplicate() => new JailbirdSetting();

        public override CustomHeader Header { get; } = SSSSModule.Header;

        protected override void HandleSettingUpdate()
        {}
    }
}