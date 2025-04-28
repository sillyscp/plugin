using Exiled.Permissions.Extensions;
using LabApi.Features.Wrappers;
using SecretAPI.Features.UserSettings;
using SillySCP.API.Modules;

namespace SillySCP.API.Features
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
            : base(835, "Colour Setting", Options, hint: "Select a colour for your supporter role")
        {
        }

        protected override CustomSetting CreateDuplicate() => new ExclusiveColorSetting();
        
        protected override bool CanView(Player player)
        {
            Exiled.API.Features.Player plr = Exiled.API.Features.Player.Get(player.ReferenceHub);
            return plr != null && plr.CheckPermission("supporter");
        }

        protected override void HandleSettingUpdate(Player player)
        {
            player.GroupColor = SelectedOption == "default" ? ServerStatic.PermissionsHandler.GetUserGroup(player.UserId).BadgeColor : SelectedOption;
        }

        public override CustomHeader Header { get; } = SSSSModule.ExclusiveHeader;
    }
}