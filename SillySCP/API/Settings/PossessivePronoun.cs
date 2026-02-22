using JetBrains.Annotations;
using LabApi.Features.Console;
using LabApi.Features.Wrappers;
using SecretAPI.Features.UserSettings;
using SillySCP.API.Modules;

namespace SillySCP.API.Settings;

public class PossessivePronoun : CustomDropdownSetting
{
    public PossessivePronoun() 
        : base(null, "Possessive Pronoun", [SubjectivePronoun.Unselected, "hers", "his", "theirs", "its"], hint: "Pick your possessive pronoun.")
    {
    }

    protected override CustomSetting CreateDuplicate() => new PossessivePronoun();

    protected override void HandleSettingUpdate()
    {
        if (KnownOwner == null)
            return;
        
        SubjectivePronoun.UpdateNickname(KnownOwner);
    }

    public override CustomHeader Header { get; } = SSSSModule.Header;

    protected override bool CanView(Player player) => SubjectivePronoun.GetSetting(player) != null && ObjectivePronoun.GetSetting(player) != null;

    [CanBeNull]
    public static PossessivePronoun GetSetting(Player player)
    {
        if (!TryGetPlayerSetting(player, out PossessivePronoun setting))
            return null;

        if (setting.SelectedOption == SubjectivePronoun.Unselected)
            return null;

        return setting;
    }
}