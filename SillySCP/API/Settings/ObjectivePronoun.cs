using JetBrains.Annotations;
using LabApi.Features.Console;
using LabApi.Features.Wrappers;
using SecretAPI.Features.UserSettings;
using SillySCP.API.Modules;

namespace SillySCP.API.Settings;

public class ObjectivePronoun : CustomDropdownSetting
{
    public ObjectivePronoun() 
        : base(null, "Objective Pronoun", [SubjectivePronoun.Unselected, "her", "him", "them", "it"], hint: "Pick your objective pronoun, once selected (and if applicable) the option for a possessive pronoun will appear.")
    {
    }

    protected override CustomSetting CreateDuplicate() => new ObjectivePronoun();
    
    private bool _previouslyVisible;

    public bool NextOptionVisible => GetSetting(KnownOwner!) != null;

    protected override void HandleSettingUpdate()
    {
        if (KnownOwner == null)
            return;
        
        SubjectivePronoun.UpdateNickname(KnownOwner);
        
        if(NextOptionVisible != _previouslyVisible)
            SendSettingsToPlayer(KnownOwner);
        
        _previouslyVisible = NextOptionVisible;
    }

    public override CustomHeader Header { get; } = SSSSModule.Header;

    protected override bool CanView(Player player) => SubjectivePronoun.GetSetting(player) != null;

    [CanBeNull]
    public static ObjectivePronoun GetSetting(Player player)
    {
        if (!TryGetPlayerSetting(player, out ObjectivePronoun setting))
            return null;

        if (setting.SelectedOption == SubjectivePronoun.Unselected)
            return null;

        return setting;
    }
}