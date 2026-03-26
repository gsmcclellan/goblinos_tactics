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
                StatName.MaxHitPoints or
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
    
    /// <summary>
    /// Makes flavor based display, possibly contextual based on type.
    /// </summary>
    /// <param name="statName"></param>
    /// <returns></returns>
    public static string GetDisplayName(StatName statName)
    {
        // Might - Smack, Smash, Slash, Stab, Blast
        // Agility - Scurry, Sneak
        // Vitality - Grit, Meatiness
        // Mind - Cunning, Guile, Weird, Trickery
        // Presence - Swagger, Bluster, Moxie, Menace
        // Luck - ??
        return statName.ToString();
    }
    
    /// <summary>
    /// Shortened version of the stat name.
    /// </summary>
    /// <param name="statName"></param>
    /// <returns></returns>
    public static string GetAbbreviatedDisplayName(StatName statName)
    {
        Dictionary<StatName, string> abbreviatedNamesDict = new Dictionary<StatName, string>()
        {
            // Core
            { StatName.Might, "Mgt" },
            { StatName.Agility, "Agi" },
            { StatName.Vitality, "Vit" },
            { StatName.Mind, "Mnd" },
            { StatName.Presence, "Pre" },
            { StatName.Luck, "Lck" },

            // Base
            { StatName.Movement, "Mov" },
            { StatName.MaxHitPoints, "HP" },
            { StatName.Defense, "Def" },
            { StatName.Resistance, "Res" },

            // Derived
            { StatName.AttackSpeed, "AtkSpd" },
            { StatName.Accuracy, "Acc" },
            { StatName.Evasion, "Eva" },
            { StatName.CritChance, "Crit" },
            { StatName.CritDefense, "CritDef" },
            { StatName.PhysicalProtection, "PhysProt" },
            { StatName.MagicProtection, "MagProt" },
            { StatName.ArmorPierce, "ArmPierce" },
            { StatName.MagicPenetration, "MagPen" }
        };
        
        // Might - Smack, Smash, Slash, Stab, Blast
        // Agility - Scurry, Sneak
        // Vitality - Grit, Meatiness
        // Mind - Cunning, Guile, Weird, Trickery
        // Presence - Swagger, Bluster, Moxie, Menace
        // Luck - ??
        if (abbreviatedNamesDict.TryGetValue(statName, out var abbreviation))
            return abbreviation;

        return GetDisplayName(statName);
    }
    
    
}
