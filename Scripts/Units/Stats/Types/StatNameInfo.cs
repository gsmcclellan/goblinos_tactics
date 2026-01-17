using System;
using System.Collections.Generic;
using System.Linq;

namespace Goblinos.Scripts.Units.Stats.Types;

public static class StatNameInfo
{
    public static StatTier GetTier(StatName statName)
    {
        return statName switch
        {
            // Core
            StatName.Might or
                StatName.Agility or
                StatName.Vitality or
                StatName.Mind or
                StatName.Presence or
                StatName.Luck => StatTier.Core,

            // Base
            StatName.Movement or
                StatName.MaxHealth or
                StatName.Defense or
                StatName.Resistance => StatTier.Base,

            // Derived
            StatName.AttackSpeed or
                StatName.Accuracy or
                StatName.Evasion or
                StatName.CritChance or
                StatName.CritDefense or
                StatName.PhysicalProtection or
                StatName.MagicProtection or
                StatName.ArmorPierce or
                StatName.MagicPenetration => StatTier.Derived,

            _ => throw new ArgumentOutOfRangeException(nameof(statName), statName, "Unhandled StatName.")
        };
    }

    public static IReadOnlyList<StatName> CoreStats { get; } = Enum.GetValues(typeof(StatName))
            .Cast<StatName>()
            .Where(sn => GetTier(sn) == StatTier.Core)
            .ToList();

    public static IReadOnlyList<StatName> BaseStats { get; } = Enum.GetValues(typeof(StatName))
            .Cast<StatName>()
            .Where(sn => GetTier(sn) == StatTier.Base)
            .ToList();

    public static IReadOnlyList<StatName> DerivedStats { get; } = Enum.GetValues(typeof(StatName))
            .Cast<StatName>()
            .Where(sn => GetTier(sn) == StatTier.Derived)
            .ToList();
}
