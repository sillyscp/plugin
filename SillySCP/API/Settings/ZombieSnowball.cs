using LabApi.Features.Wrappers;
using MapGeneration.Holidays;
using PlayerRoles;
using PlayerRoles.Subroutines;
using SecretAPI.Features.UserSettings;
using SillySCP.API.Modules;
using UnityEngine;

namespace SillySCP.API.Settings;

public class ZombieSnowball : CustomKeybindSetting
{
    public ZombieSnowball()
        : base(null, "Equip Snowball as Zombie", KeyCode.F,
            hint: "What keybind should be pressed to equip a snowball as a zombie.")
    {
        _cooldown = new();
    }

    protected override bool CanView(Player _) => HolidayUtils.GetActiveHoliday() == HolidayType.Christmas;
    
    protected override CustomSetting CreateDuplicate() => new ZombieSnowball();
    
    private AbilityCooldown _cooldown;

    protected override void HandleSettingUpdate()
    {
        if (KnownOwner == null)
            return;

        if (!_cooldown.IsReady)
            return;

        if (KnownOwner.Role != RoleTypeId.Scp0492)
            return;
        
        _cooldown.Trigger(5);
        
        Item item = KnownOwner.AddItem(ItemType.Snowball);

        KnownOwner.CurrentItem = item;
    }

    public override CustomHeader Header { get; } = SSSSModule.Header;
}