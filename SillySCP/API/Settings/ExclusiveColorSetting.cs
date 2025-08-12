using GameCore;
using LabApi.Features.Console;
using LabApi.Features.Permissions;
using LabApi.Features.Wrappers;
using SecretAPI.Features.UserSettings;
using SillySCP.API.Modules;

namespace SillySCP.API.Settings
{
    public class ExclusiveColorSetting : CustomDropdownSetting
    {
        private new static readonly string[] Options = new[]
        {
            "default",
            "red",
            "silver",
            "light_green",
            "cyan",
            "aqua",
            "deep_pink",
            "tomato",
            "magenta",
            "orange",
            "lime",
            "green",
            "emerald",
            "nickel",
            "mint",
            "army_green",
            "pumpkin"
        };

        public ExclusiveColorSetting()
            : base(null, "Colour Setting", Options, hint: "Select a colour for your supporter role")
        {
        }

        protected override CustomSetting CreateDuplicate() => new ExclusiveColorSetting();
        
        protected override bool CanView(Player player) => player.HasPermissions("supporter");

        protected override void HandleSettingUpdate()
        {
            if (KnownOwner == null)
                return;
            
            KnownOwner.GroupColor = SelectedOption == "default" ? ServerStatic.PermissionsHandler.GetUserGroup(KnownOwner.UserId).BadgeColor : SelectedOption;
        }

        public override CustomHeader Header { get; } = SSSSModule.ExclusiveHeader;
    }
}