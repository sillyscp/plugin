using InventorySystem.Items.Scp1509;
using PlayerRoles.PlayableScps.Scp1507;
using PlayerRoles.PlayableScps.Scp3114;
using PlayerRoles.PlayableScps.Scp939;
using PlayerStatsSystem;

namespace LogAssistant.Extensions;

public static class DamageHandlerExtensions
{
    extension(DamageHandlerBase handler)
    {
        public string Reason => ConvertToString(handler);
    }
    
    private static readonly Dictionary<byte, string> Translations = new()
    {
        { DeathTranslations.Asphyxiated.Id, "Asphyxiation" },
        { DeathTranslations.Bleeding.Id, "Bleeding" },
        { DeathTranslations.Crushed.Id, "Crushed" },
        { DeathTranslations.Decontamination.Id, "Decontamination" },
        { DeathTranslations.Explosion.Id, "Explosion" },
        { DeathTranslations.Falldown.Id, "Falldown" },
        { DeathTranslations.Poisoned.Id, "Poison" },
        { DeathTranslations.Recontained.Id, "Recontainment" },
        { DeathTranslations.Scp049.Id, "SCP-049" },
        { DeathTranslations.Scp096.Id, "SCP-096" },
        { DeathTranslations.Scp173.Id, "SCP-173" },
        { DeathTranslations.Scp207.Id, "SCP-207" },
        { DeathTranslations.Scp939Lunge.Id, "SCP-939 Lunge" },
        { DeathTranslations.Scp939Other.Id, "SCP-939" },
        { DeathTranslations.Scp3114Slap.Id, "SCP-3114" },
        { DeathTranslations.Tesla.Id, "Tesla" },
        { DeathTranslations.Unknown.Id, "Unknown" },
        { DeathTranslations.Warhead.Id, "Warhead" },
        { DeathTranslations.Zombie.Id, "SCP-049-2" },
        { DeathTranslations.BulletWounds.Id, "Firearm" },
        { DeathTranslations.PocketDecay.Id, "Pocket Decay" },
        { DeathTranslations.SeveredHands.Id, "Severed Hands" },
        { DeathTranslations.FriendlyFireDetector.Id, "Friendly Fire" },
        { DeathTranslations.UsedAs106Bait.Id, "Femur Breaker" },
        { DeathTranslations.MicroHID.Id, "Micro H.I.D." },
        { DeathTranslations.Hypothermia.Id, "Hypothermia" },
        { DeathTranslations.MarshmallowMan.Id, "Marshmellow" },
        { DeathTranslations.Scp1344.Id, "Severed Eyes" },
        { DeathTranslations.Scp127Bullets.Id, "SCP-127" }
    };

    private static string ConvertToString(DamageHandlerBase handler) => handler switch
        {
            CustomReasonDamageHandler or CustomReasonFirearmDamageHandler => "Unknown, plugin specific death.",
            GrayCandyDamageHandler => "Metal Man",
            Scp096DamageHandler => "SCP-096",
            Scp1509DamageHandler => "SCP-1509",
            SilentDamageHandler => "Silent",
            WarheadDamageHandler => "Warhead",
            ExplosionDamageHandler => "Explosion",
            Scp018DamageHandler => "SCP-018",
            RecontainmentDamageHandler => "Recontainment",
            MicroHidDamageHandler => "Micro H.I.D.",
            DisruptorDamageHandler => "Particle Disruptor",
            Scp939DamageHandler => "SCP-939",
            JailbirdDamageHandler => "Jailbird",
            Scp1507DamageHandler => "SCP-1507",
            Scp956DamageHandler => "SCP-956",
            SnowballDamageHandler => "Snowball",
            Scp3114DamageHandler scp3114DamageHandler => scp3114DamageHandler.Subtype switch
            {
                Scp3114DamageHandler.HandlerType.Strangulation => "Strangled",
                Scp3114DamageHandler.HandlerType.SkinSteal => "SCP-3114",
                Scp3114DamageHandler.HandlerType.Slap => "SCP-3114",
                _ => "Unknown",
            },
            Scp049DamageHandler scp049DamageHandler => scp049DamageHandler.DamageSubType switch
            {
                Scp049DamageHandler.AttackType.CardiacArrest => "Cardiac Arrest",
                Scp049DamageHandler.AttackType.Instakill => "SCP-049",
                Scp049DamageHandler.AttackType.Scp0492 => "SCP-049-2",
                _ => "Unknown",
            },
            ScpDamageHandler scpDamageHandler => FromTranslationId(scpDamageHandler._translationId),
            UniversalDamageHandler universal => FromTranslationId(universal.TranslationId),
            FirearmDamageHandler firearm => firearm.Firearm.Name,
            _ => "Unknown"
        };

    private static string FromTranslationId(byte id)
    {
        DeathTranslation translation = DeathTranslations.TranslationsById[id];

        return Translations.GetValueOrDefault(translation.Id, "Unknown");
    }
}