using Exiled.API.Features;
using SecretAPI.Features.UserSettings;
using SillySCP.API.Modules;
using Player = LabApi.Features.Wrappers.Player;

namespace SillySCP.API.Settings
{
    public class IntercomSetting : CustomTwoButtonSetting
    {
        
        public const int SettingId = 838;
        public IntercomSetting()
            : base(SettingId, "Intercom", "Yes", "No", hint: "When")
        {}

        protected override CustomSetting CreateDuplicate() => new IntercomSetting();

        public override CustomHeader Header { get; } = SSSSModule.Header;

        protected override void HandleSettingUpdate(Player player)
        { }
    }
}