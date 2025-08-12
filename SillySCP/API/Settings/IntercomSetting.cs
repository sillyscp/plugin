using SecretAPI.Features.UserSettings;
using SillySCP.API.Modules;
using Player = LabApi.Features.Wrappers.Player;

namespace SillySCP.API.Settings
{
    public class IntercomSetting : CustomTwoButtonSetting
    {
        public static int SettingId { get; private set; }

        public IntercomSetting()
            : base(null, "Mute Intercom?", "Yes", "No", true,
                "When the option Yes is selected, intercom will be muted for you.")
        {
            SettingId = Id;
        }

        protected override CustomSetting CreateDuplicate() => new IntercomSetting();

        public override CustomHeader Header { get; } = SSSSModule.Header;

        protected override void HandleSettingUpdate()
        { }
    }
}