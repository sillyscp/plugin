using System.Text;
using JetBrains.Annotations;
using LabApi.Features.Console;
using LabApi.Features.Wrappers;
using NorthwoodLib.Pools;
using SecretAPI.Features.UserSettings;
using SillySCP.API.Modules;
using UserSettings.ServerSpecific;

namespace SillySCP.API.Settings;

public class SubjectivePronoun : CustomDropdownSetting
{
    public const string Unselected = "none specified";
    
    public SubjectivePronoun() 
        : base(null, "Subjective Pronoun", [Unselected, "she", "he", "they", "it", "any", "ask"], hint: "Pick your subjective pronoun, once selected (and if applicable) the option for an objective pronoun will appear.")
    {
    }

    protected override CustomSetting CreateDuplicate() => new SubjectivePronoun();

    private bool _previouslyVisible;

    public bool NextOptionVisible => GetSetting(KnownOwner!) != null;

    protected override void HandleSettingUpdate()
    {
        if (KnownOwner == null)
            return;
        
        UpdateNickname(KnownOwner);
        
        if(NextOptionVisible != _previouslyVisible)
            SendSettingsToPlayer(KnownOwner);
        
        _previouslyVisible = NextOptionVisible;
    }

    public override CustomHeader Header { get; } = SSSSModule.Header;

    public static void UpdateNickname(Player player)
    {
        if (!TryGetPlayerSetting(player, out SubjectivePronoun subjective) || subjective.SelectedOption is Unselected)
        {
            player.DisplayName = null!;
            return;
        }

        switch (subjective.SelectedOption)
        {
            case "any":
                player.DisplayName = $"{player.Nickname} (any pronouns)";
                return;
            case "ask":
                player.DisplayName = $"{player.Nickname} (ask)";
                return;
        }

        ObjectivePronoun objective = ObjectivePronoun.GetSetting(player);
        PossessivePronoun possessive = PossessivePronoun.GetSetting(player);
        
        StringBuilder builder = StringBuilderPool.Shared.Rent(player.Nickname);

        builder.Append(" (");
        
        builder.Append(subjective.SelectedOption);

        if (objective != null)
        {
            builder.Append("/");
            builder.Append(objective.SelectedOption);
        }

        if (possessive != null)
        {
            builder.Append("/");
            builder.Append(possessive.SelectedOption);
        }
        
        builder.Append(")");

        player.DisplayName = StringBuilderPool.Shared.ToStringReturn(builder);
    }

    [CanBeNull]
    public static SubjectivePronoun GetSetting(Player player)
    {
        if (!TryGetPlayerSetting(player, out SubjectivePronoun setting))
            return null;

        if (setting.SelectedOption is Unselected or "any" or "ask")
            return null;

        return setting;
    }
}